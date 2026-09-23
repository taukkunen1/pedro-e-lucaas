using Core;
using Core.Models.SharedConfig;
using GameServer.Cryptography;
using GameServer.Database;
using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GameServer
{
    using static global::GameServer.Pool;
    using PacketInvoker = CachedAttributeInvocation<Action<Client.GameClient, ServerSockets.Packet>, PacketAttribute, ushort>;

    public class Program
    {
        public static FastRandom Random = new();
        public static string StartupPath = "";

        public static bool ChanceSuccess(int percent)
        {
            if (percent == 0)
                return false;
            return (Random.Next(0, 100) < percent);
        }
        public static int WorldEvent = 0;
        public static int WorldBoss = 0;
        public static int ExpBallsDropped = 0;
        public static int Plus8, Super2Soc, Super1Soc, SuperNoSoc;
        public static bool TestServer = false;
        public static string ServerMode = "PRODUCTION";
        public static ulong CPsHuntedSinceRestart = 0;
        public static bool OnMainternance = false;
        public static TransferCipher transferCipher;
        public static ServerSockets.SocketPoll SocketsGroup;
        public static ShowChatItems GlobalItems;
        public static SendGlobalPacket SendGlobalPackets;
#if USE_WINDOWS_API
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleHandlerDelegate handler, bool add);
        private delegate bool ConsoleHandlerDelegate(int type);
        private static ConsoleHandlerDelegate handlerKeepAlive;
#endif
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
                Telemetry.ServerLog.Error("unhandled_exception", exception);
            else
                Telemetry.ServerLog.System("unhandled_exception", e.ToString());
            Console.WriteLine(e.ToString());
        }
        public static ServerSockets.ServerSocket COServer;

        public static bool ProcessConsoleEvent(int type)
        {
            try
            {
                if (ServerConfig.IsInterServer)
                {
                    foreach (var client in GamePoll.Values)
                    {
                        try
                        {
                            if (client.Socket != null)//for my fake accounts !
                                client.Socket.Disconnect();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    return true;
                }
                try
                {
                   
                    if (COServer != null)
                        COServer.Close();


                }
                catch (Exception e) { Console.SaveException(e); }

                Console.WriteLine("Saving Database...");


                foreach (var client in Pool.GamePoll.Values)
                {
                    try
                    {
                        if (client.Socket != null)//for my fake accounts !
                            client.Socket.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                Role.Instance.Clan.ProcessChangeNames();

                Database.Server.SaveDatabase();
                if (Database.ServerDatabase.LoginQueue.Finish())
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Database Save Succefull.");
                }
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
            return true;
        }

        public static ServerSockets.ThreadPool CallBack;
        public static Threading.Basic ServerCallback;
        public static bool RunningOnWindows = false;
        public static GameServerConfig GSConfig { get; set; }

        public static void Main(string[] args)
        {
            if (args.Length > 0 && string.Equals(args[0], "era1-selftest", StringComparison.OrdinalIgnoreCase))
            {
                Game.MsgNpc.Dialogs.Era1JobCenter.RunSelfTest();
                Game.Era1.Era1Progression.RunSelfTest();
                return;
            }

            bool InitAutoMaintenance = false;
            try
            {
                if (args.Length > 0)
                {
                    if (args[0] == "automaintenance")
                    {
                        InitAutoMaintenance = true;
                    } else
                    {
                        RestApiHelper.ApiPort = ushort.Parse(args[0]);
                        RestApiHelper.Init();
                        Console.WriteLine($"[API URI: {RestApiHelper.ApiRequestBaseURI}]");
                    }
                }
                RunningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                StartupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                GSConfig = RestApiHelper.GetRequest<GameServerConfig>("GetGSConfig");
                ConfigurableDropSystem.Init(); // Configurable Drop
                EventsRewards.Init(); // Configurable Rewards for Events
                BulletinManager.Init(); // Init Bulletin
                SquamaManager.Init(); // Init Squama
                #region ItemCounterUID
                Console.WriteLine($"Starting ItemCounterUID with: {DefaultItemCounterUID}", ConsoleColor.DarkGreen);
                ItemUIDCounter = new(DefaultItemCounterUID);
                #endregion
                if (!Utils.CanConnect())
                {
                    Console.WriteLine("Cannot connect to the database auth with your configuration. Press Any key for close.");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return;
                }
                if (!ServerConfig.DbFromFiles)
                {
                    Utils.GSMigrationInit();
                }
#if USE_WINDOWS_API
                if (RunningOnWindows) {
                    Console.DissableButton();
                }
#endif
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                ServerSockets.Packet.SealString = "TQServer";
                System.Console.ForegroundColor = ConsoleColor.White;
                MsgInvoker = new PacketInvoker(PacketAttribute.Translator);
                DHKeyExchange.KeyExchange.CreateKeys();
                Game.MsgTournaments.MsgSchedules.Create();
                Database.Server.Initialize();
                SendGlobalPackets = new SendGlobalPacket();
                AuthCryptography.PrepareAuthCryptography();
                Database.Server.LoadDatabase();
                Poker.Database.Load();
                //foreach (var kvP in Pool.ServerMaps.Base)
                //{
                //    var mapManager = new MapManager(kvP.Value);
                //}
#if USE_WINDOWS_API
                handlerKeepAlive = ProcessConsoleEvent;
                if (RunningOnWindows)
                {
                    SetConsoleCtrlHandler(handlerKeepAlive, true);
                }
#endif

                if (InitAutoMaintenance)
                {
                    Thread.Sleep(1000 * 10);//10s
                    ConsoleCMD(args[0]);
                }

                // Get the server by name
                Dictionary<string, string> argGetServerByName = new Dictionary<string, string>();
                argGetServerByName.Add("serverName", ServerConfig.ServerName);
                Core.Models.Server serverLoad = RestApiHelper.GetRequest<Core.Models.Server>("GetServerByName", argGetServerByName);

                if (serverLoad == null) {
                    Console.WriteLine($"Cannot start server because not have {ServerConfig.ServerName} in servers table.", ConsoleColor.DarkRed);
                    Console.ReadKey();
                    return;
                }

                TransferCipher.Key = Encoding.UTF8.GetBytes(serverLoad.TransferKey);
                TransferCipher.Salt = Encoding.UTF8.GetBytes(serverLoad.TransferSalt);
                transferCipher = new TransferCipher("127.0.0.1");
                if (ServerConfig.IsInterServer == false)
                {
                    COServer = new ServerSockets.ServerSocket(
                        new Action<ServerSockets.SecuritySocket>(p => new Client.GameClient(p))
                        , Game_Receive, Game_Disconnect);
                    COServer.Initilize(ServerConfig.Port_SendSize, ServerConfig.Port_ReceiveSize, 1, 3);
                    COServer.Open(ServerConfig.IPAddres, ServerConfig.GamePort, ServerConfig.Port_BackLog);
                }
                GlobalItems = new ShowChatItems();
                SaveServerDatabase = DateTime.Now.AddMinutes(3);
                //Database.NpcServer.LoadServerTraps();
                SmartNPCManager.Load();
                SocketsGroup = new ServerSockets.SocketPoll("ConquerServer", COServer);
                new MapGroupThread(300, "ConquerServer3").Start();

                Console.WriteLine("Starting the server...");
                new Catching();
                CallBack = new ServerSockets.ThreadPool();
                ServerCallback = new global::GameServer.Threading.Basic(ServerSockets.ThreadPool.ServerCallBack, 250);
                Game.MsgTournaments.MsgSchedules.ClanWar = new Game.MsgTournaments.MsgClanWar();
                for (int i = 0; i < 10; i++)
                    Game.MsgTournaments.MsgSchedules.SpawnLavaBeast(true);
               
              
                Console.WriteLine("The server is ready for incoming connections!\n", ConsoleColor.Green);
              
               
            }
            catch (Exception e) { Console.WriteException(e); }

            System.Threading.Tasks.Task.Run(async() =>
            {
                while (true)
                {
                    using (NamedPipeServerStream server = new NamedPipeServerStream("TrinityConquerGSPipe"))
                    {
                        await server.WaitForConnectionAsync();

                        using (StreamReader reader = new StreamReader(server))
                        {
                            string message = await reader.ReadLineAsync();
                            Console.WriteLine($"[TrinityConquerHelper] Command executed from Helper: {message}", ConsoleColor.DarkGreen);
                            ConsoleCMD(message);
                            Console.WriteLine("[TrinityConquerHelper] Finished.", ConsoleColor.White);
                        }
                    }
                }
            });

            for (; ; )
                ConsoleCMD(Console.ReadLine());
        }
#region Fix
        public static PacketInvoker MsgInvoker;

        public unsafe static void Game_Disconnect(ServerSockets.SecuritySocket obj)
        {

            if (obj.Game != null && obj.Game.Player != null)
            {
                try
                {
                    Client.GameClient client;
                    if (Pool.GamePoll.TryGetValue(obj.Game.Player.UID, out client))
                    {
                        Pool.DisconnectPool.TryAdd(client.Player.UID, client);
                        try
                        {
                            PokerHandler.Shutdown(client);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        if (client.OnInterServer)
                            return;
                        if ((client.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                        {
                            client.ClientFlag |= Client.ServerFlag.QueuesSave;
                            if (obj.Game.PipeClient != null)
                                obj.Game.PipeClient.Disconnect();

                            ServerSockets.ThreadPool.Unregister(client);

                            client.ClientFlag |= Client.ServerFlag.QueuesSave;
                            Database.ServerDatabase.LoginQueue.TryEnqueue(obj.Game);

                            //Database.ServerDatabase.SaveClientItems(obj.Game);
                            Console.WriteLine(client.Player.Name + " has logged out.", ConsoleColor.Red);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                try
                                {
                                    client.EndQualifier();
                                    if (client.Team != null)
                                        client.Team.Remove(client, true);
                                    if (client.Player.MyClanMember != null)
                                        client.Player.MyClanMember.Online = false;
                                    if (client.IsVendor)
                                        client.MyVendor.StopVending(stream);
                                    if (client.InTrade)
                                        client.MyTrade.CloseTrade();
                                    if (client.Player.MyGuildMember != null)
                                        client.Player.MyGuildMember.IsOnline = false;
                                    if (client.Player.ObjInteraction != null)
                                    {
                                        client.Player.InteractionEffect.AtkType = Game.MsgServer.MsgAttackPacket.AttackID.InteractionStopEffect;

                                        InteractQuery action = InteractQuery.ShallowCopy(client.Player.InteractionEffect);

                                        client.Send(stream.InteractionCreate(&action));

                                        client.Player.ObjInteraction.Player.OnInteractionEffect = false;
                                        client.Player.ObjInteraction.Player.ObjInteraction = null;
                                    }


                                    client.Player.View.Clear(stream);


                                }
                                catch (Exception e)
                                {
                                    Console.WriteException(e);
                                    client.Player.View.Clear(stream);
                                }
                                finally
                                {
                                    client.ClientFlag &= ~Client.ServerFlag.LoginFull;
                                    client.ClientFlag |= Client.ServerFlag.Disconnect;
                                    client.ClientFlag |= Client.ServerFlag.QueuesSave;
                                }

                                try
                                {
                                    client.Player.Associate.OnDisconnect(stream, client);

                                    //remove mentor and apprentice
                                    if (client.Player.MyMentor != null)
                                    {
                                        Client.GameClient me;
                                        client.Player.MyMentor.OnlineApprentice.TryRemove(client.Player.UID, out me);
                                        client.Player.MyMentor = null;
                                    }
                                    client.Player.Associate.Online = false;
                                    lock (client.Player.Associate.MyClient)
                                        client.Player.Associate.MyClient = null;
                                    foreach (var clien in client.Player.Associate.OnlineApprentice.Values)
                                        clien.Player.SetMentorBattlePowers(0, 0);
                                    client.Player.Associate.OnlineApprentice.Clear();
                                    if (client.Map != null)
                                    {
                                        client.Map.Denquer(client);
                                    }
                                    //done remove
                                }
                                catch (Exception e) { Console.WriteLine(e.ToString()); }
                            }
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            else if (obj.Game != null)
            {
                if (obj.Game.ConnectionUID != 0)
                {

                    try
                    {
                        PokerHandler.Shutdown(obj.Game);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    Client.GameClient client;
                    Pool.GamePoll.TryRemove(obj.Game.ConnectionUID, out client);
                }
            }
        }

        public unsafe static void Game_Receive(ServerSockets.SecuritySocket obj, ServerSockets.Packet stream)//ServerSockets.Packet data)
        {
#if TEST
            Console.WriteLine("Game_Receive Init.");
#endif
            ushort PacketID = 0;
            if (!obj.SetDHKey)
            { CreateDHKey(obj, stream); }
            else
            {
                try
                {
                    if (obj.Game == null)
                        return;
                    PacketID = stream.ReadUInt16();

                    if (obj.Game.Player.CheckTransfer)
                        goto jmp;
                    if (obj.Game.PipeClient != null && PacketID != Game.GamePackets.Achievement)
                    {
                        if (PacketID == (ushort)Game.GamePackets.MsgOsShop || PacketID == (ushort)Game.GamePackets.SecondaryPassword)
                            goto jmp;

                        stream.Seek(stream.Size);
                        obj.Game.PipeClient.Send(stream);

                        if (PacketID != 1009)
                        {
                            return;
                        }
                        stream.Seek(4);
                    }

                    if (Program.TestServer || System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.WriteLine("[Debugger] Receive -> PacketID: " + PacketID);
                    }
                    //   Database.ServerDatabase.LoginQueue.Enqueue("[CallStack]" + MyConsole.log1(obj.Game.Player.Name, stream.Memory, stream.Size));
                    jmp:
                    if (PacketID == 2171 || PacketID == 2088 || PacketID == 2096 || PacketID == 2090 || PacketID == 2093)
                    {
                        PokerHandler.Handler(obj.Game, stream);
                    }
                    else
                    {
                        Action<Client.GameClient, ServerSockets.Packet> hinvoker;
                        if (!Game.MsgServer.PacketGuards.ValidateClientPacket(obj.Game, stream, PacketID, out string guardReason))
                        {
                            Telemetry.ServerLog.Packet("blocked_packet", obj.Game, PacketID, stream);
                            Telemetry.ServerLog.System("blocked_packet_reason", guardReason, new Dictionary<string, object>
                            {
                                ["packetId"] = PacketID,
                                ["connectionUid"] = obj.Game.ConnectionUID
                            });
                            if (Program.TestServer || System.Diagnostics.Debugger.IsAttached)
                            {
                                Console.WriteLine("[PacketGuard] Blocked " + PacketID + ": " + guardReason);
                            }
                            return;
                        }
                        if (MsgInvoker.TryGetInvoker(PacketID, out hinvoker))
                        {
                            hinvoker(obj.Game, stream);
                        }
                        else
                        {
                            Telemetry.ServerLog.Packet("unknown_packet", obj.Game, PacketID, stream);
                            if (Program.TestServer)
                            {
                                Console.WriteLine("[Debugger] Not found packet ----> " + PacketID);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Telemetry.ServerLog.Packet("packet_exception", obj != null ? obj.Game : null, PacketID, stream, e);
                    Console.WriteException(e);
                }
                finally
                {
                    ServerSockets.PacketRecycle.Reuse(stream);
                }
            }

        }

#endregion
        public unsafe static void ConsoleCMD(string cmd)
        {
            if (cmd == null) return;
            try
            {
                string[] line = cmd.Split(' ');

                switch (line[0])
                {
                    #region Custom Commands Made by DaRkFox
                    case "reload":
                        {
                            bool reloadItems = true;
                            bool reloadSobNpcs = true;
                            bool reloadMaps = true;
                            bool reloadLotteryItems = true;
                            if (line.Length >= 2)
                            {
                                reloadItems = line[1] == "--items";
                                reloadSobNpcs = line[1] == "--sobnpcs";
                                reloadMaps = line[1] == "--maps";
                                reloadLotteryItems = line[1] == "--lottery";
                            }
                            if (reloadItems || reloadSobNpcs || reloadMaps || reloadLotteryItems)
                            {
                                if (reloadItems)
                                {
                                    Pool.ItemsBase.Loading(true);
                                    Console.WriteLine("Itemtype Reloaded", ConsoleColor.DarkGreen);
                                }
                                if (reloadSobNpcs)
                                {
                                    foreach (var kvP in Pool.ServerMaps.Base)
                                    {
                                        uint MapId = kvP.Value.ID;
                                        NpcServer.LoadSobNpcs(MapId);
                                    }
                                    Console.WriteLine("Sob NPCs Reloaded", ConsoleColor.DarkGreen);
                                }
                                if (reloadMaps)
                                {
                                    Role.GameMap.LoadMaps();
                                    Console.WriteLine("Maps Reloaded", ConsoleColor.DarkGreen);
                                }
                                if (reloadLotteryItems)
                                {
                                    Pool.NewLottery.LoadLotteryItems();
                                    Console.WriteLine("Lottery items Reloaded", ConsoleColor.DarkGreen);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Specify what need reload or leave second parameter empty. Example: @reload --items or @reload --sobnpcs or @reload maps", ConsoleColor.DarkGreen);
                            }
                            break;
                        }
                    case "monsterscheck":
                        {
                            if (line.Length >= 2 && line[1] == "--confirm")
                            {
                                FixMonstersFromMonsterDat.FixFromDat();
                                Console.WriteLine($"Monster check process completed and all monster .ini files has been repaired successfully", ConsoleColor.DarkGreen);
                            }
                            else
                            {
                                Console.WriteLine($"This action check all monsters from 'monster.txt' and fix all monster .ini files. Confirm run with {line[0]} --confirm", ConsoleColor.DarkRed);
                            }
                            break;
                        }
                    case "migratetomysql":
                        {
                            if (line.Length >= 2 && line[1] == "--confirm")
                            {
                                SqlMigrator sqlMigrator = new SqlMigrator();
                                bool migrationOK = sqlMigrator.InitMigration();
                                if (migrationOK)
                                {
                                    // After migrate current data to mysql
                                    ServerConfig.DbFromFiles = false;
                                    // Save GSConfig changed
                                    GSConfig.DbFromFiles = ServerConfig.DbFromFiles;
                                    RestApiHelper.PostRequestSuccessful("SetGSConfig", GSConfig);
                                    Console.WriteLine($"Using data from mysql now", ConsoleColor.DarkGreen);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"This action migrate all data for use it in Mysql Database. WARNING: This is in development yet and not recommended for use in production. Confirm run with {line[0]} --confirm", ConsoleColor.DarkRed);
                            }
                            break;
                        }
                    #endregion
                    case "clear":
                        {
                            System.Console.Clear();
                            break;
                        }
                    case "save":
                        {
                            Database.Server.SaveDatabase();
                            if (Pool.FullLoading && !ServerConfig.IsInterServer)
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.OnInterServer)
                                        continue;
                                    if ((user.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                                    {
                                        user.ClientFlag |= Client.ServerFlag.QueuesSave;
                                        Database.ServerDatabase.LoginQueue.TryEnqueue(user);
                                    }
                                }
                                Console.WriteLine("Database got saved ! ");
                            }
                            if (Database.ServerDatabase.LoginQueue.Finish())
                            {
                                System.Threading.Thread.Sleep(1000);
                                Console.WriteLine("Database saved successfully.");
                            }
                            break;
                        }
                    case "ctfon":
                        {
                            Game.MsgTournaments.MsgSchedules.CaptureTheFlag.Start();
                            break;
                        }
                    case "kick":
                        {

                            foreach (var user in Pool.GamePoll.Values)
                            {
                                if (user.Player.Name.Contains(line[1]))
                                {
                                    user.EndQualifier();
                                }
                            }
                            break;
                        }
                    case "pk":
                        {
                            Game.MsgTournaments.MsgSchedules.ElitePkTournament.Start();
                            var array = Pool.GamePoll.Values.ToArray();
                            foreach (var client in array)
                            {
                                Game.MsgTournaments.MsgSchedules.ElitePkTournament.SignUp(client);
                            }
                            break;
                        }
                    case "classpk":
                        {
                            Game.MsgTournaments.MsgSchedules.ClassPkWar.Start();
                            break;
                        }
                    case "teampk":
                        {
                            Game.MsgTournaments.MsgSchedules.TeamPkTournament.Start();
                            var array = Pool.GamePoll.Values.ToArray();


                            for (int x = 0; x < array.Length - 5; x += 5)
                            {
                                if (array[x].Team == null)
                                {
                                    try
                                    {
                                        array[x].Team = new Role.Instance.Team(array[x]);
                                        Game.MsgTournaments.MsgSchedules.TeamPkTournament.SignUp(array[x]);
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            array[x + 1].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 1]);
                                            Game.MsgTournaments.MsgSchedules.TeamPkTournament.SignUp(array[x + 1]);

                                            array[x + 2].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 2]);
                                            Game.MsgTournaments.MsgSchedules.TeamPkTournament.SignUp(array[x + 2]);

                                            array[x + 3].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 3]);
                                            Game.MsgTournaments.MsgSchedules.TeamPkTournament.SignUp(array[x + 3]);

                                            array[x + 4].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 4]);
                                            Game.MsgTournaments.MsgSchedules.TeamPkTournament.SignUp(array[x + 4]);
                                        }

                                    }
                                    catch { }
                                }
                            }
                            break;
                        }
                    case "skillteam":
                        {
                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Start();
                            var array = Pool.GamePoll.Values.ToArray();


                            for (int x = 0; x < array.Length - 5; x += 5)
                            {
                                if (array[x].Team == null)
                                {
                                    try
                                    {
                                        array[x].Team = new Role.Instance.Team(array[x]);
                                        Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x]);
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            array[x + 1].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 1]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 1]);

                                            array[x + 2].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 2]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 2]);

                                            array[x + 3].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 3]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 3]);

                                            array[x + 4].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 4]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 4]);
                                        }

                                    }
                                    catch { }
                                }
                            }
                            break;
                        }
                    case "search":
                        {
                            try
                            {
                                IniFileHelper ini = new IniFileHelper();
                                foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                                {
                                    try
                                    {
                                        ini.LoadFile(fname);

                                        string Name = ini.ReadString("Character", "Name", "None");
                                        if (Name.ToLower() == line[1].ToLower() || Name.Contains(line[1]))
                                        {
                                            Console.WriteLine(ini.ReadUInt32("Character", "UID", 0));
                                            break;
                                        }
                                    }
                                    catch
                                    {

                                    }

                                }
                            }
                            catch
                            {
                                Console.WriteLine("Exception thrown.");
                            }
                            break;
                        }
                    case "searchcps":
                        {
                            IniFileHelper ini = new IniFileHelper();
                            Dictionary<string, uint> Scammers = new Dictionary<string, uint>();
                            foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                            {
                                ini.LoadFile(fname);

                                string Name = ini.ReadString("Character", "Name", "None");
                                uint ConquerPoints = ini.ReadUInt32("Character", "ConquerPoints", 0);
                                uint gold = ini.ReadUInt32("Character", "Money", 0);
                                Scammers.Add(Name, ConquerPoints);

                            }
                            var sortedScammers = Scammers.OrderByDescending(e => e.Value);
                            break;
                        }
                    case "resetdragon":
                        {
                            IniFileHelper ini = new IniFileHelper();
                            foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                            {
                                ini.LoadFile(fname);

                                var yesterday = DateTime.Today.AddDays(-1);
                                ini.Write<long>("Character", "LastDragonPill", yesterday.Ticks);
                            }
                            Console.WriteLine("Reseted Dragon");
                            break;
                        }
                    case "resetnobility":
                        {
                            Console.WriteLine("Are u sure u want to reset nobility? (y/n)");
                            var input = Console.ReadLine();

                            if (input == "y")
                            {
                                IniFileHelper ini = new IniFileHelper();
                                foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                                {
                                    ini.LoadFile(fname);

                                    ulong nobility = ini.ReadUInt64("Character", "DonationNobility", 0);
                                    ini.Write<ulong>("Character", "DonationNobility", 0);
                                }
                            }

                            break;
                        }
                    case "resetvip":
                        {
                            IniFileHelper ini = new IniFileHelper();
                            foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                            {
                                ini.LoadFile(fname);

                                ini.Write<byte>("Character", "VipLevel", 3);
                                ini.Write<long>("Character", "VipTime", new DateTime(DateTime.Now.Year, 4, 10, 0, 0, 0).Ticks);
                            }

                            break;
                        }
                    case "check":
                        {
                            IniFileHelper ini = new IniFileHelper();
                            foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                            {
                                ini.LoadFile(fname);

                                long nobility = ini.ReadInt64("Character", "Money", 0);
                                if (nobility < 0)
                                {
                                    Console.WriteLine("");
                                }

                            }
                            break;
                        }
                    case "fixedgamemap":
                        {
                            Dictionary<int, string> maps = new Dictionary<int, string>();
                            using (var gamemap = new BinaryReader(new FileStream(Path.Combine(ServerConfig.CO2Folder, "ini", "GameMap.dat"), FileMode.Open)))
                            {

                                var amount = gamemap.ReadInt32();
                                for (var i = 0; i < amount; i++)
                                {

                                    var id = gamemap.ReadInt32();
                                    var fileName = Encoding.UTF8.GetString(gamemap.ReadBytes(gamemap.ReadInt32()));
                                    var puzzleSize = gamemap.ReadInt32();
                                    if (id == 1017)
                                    {
                                        Console.WriteLine(puzzleSize);
                                    }
                                    if (!maps.ContainsKey(id))
                                        maps.Add(id, fileName);
                                    else
                                        maps[id] = fileName;
                                }
                            }
                            break;
                        }
                    case "startgw":
                        {
                            Game.MsgTournaments.MsgSchedules.GuildWar.Proces = Game.MsgTournaments.ProcesType.Alive;
                            Game.MsgTournaments.MsgSchedules.GuildWar.ManualStarted = true;
                            Game.MsgTournaments.MsgSchedules.GuildWar.Start();
                            break;
                        }
                    case "finishgw":
                        {
                            Game.MsgTournaments.MsgSchedules.GuildWar.Proces = Game.MsgTournaments.ProcesType.Dead;
                            Game.MsgTournaments.MsgSchedules.GuildWar.ManualStarted = true;
                            Game.MsgTournaments.MsgSchedules.GuildWar.CompleteEndGuildWar();
                            break;
                        }
                    case "elite":
                        {
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces = Game.MsgTournaments.ProcesType.Alive;
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Start();
                            break;
                        }
                    case "eliteoff":
                        {
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces = Game.MsgTournaments.ProcesType.Dead;
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.CompleteEndGuildWar();
                            break;
                        }
                    case "dropconfig":
                        {
                            if (line.Length >= 2)
                            {
                                string paramSubcommand = line[1];
                                if (paramSubcommand == "reload")
                                {
                                    ConfigurableDropSystem.Init();
                                    Console.WriteLine("Dropconfig reloaded.");
                                }
                            } else
                            {
                                Console.WriteLine("Cannot use this command without the parameters. Usage: dropconfig reload (For reload the config)");
                            }
                            break;
                        }
                    case "exit":
                        {
                            new Thread(new ThreadStart(Maintenance)).Start();
                            break;
                        }
                    case "forceexit":
                        {
                            ProcessConsoleEvent(0);
                            Environment.Exit(0);
                            break;
                        }
                    case "restart":
                        {
                            ProcessConsoleEvent(0);
                            System.Diagnostics.Process hproces = new System.Diagnostics.Process();
                            hproces.StartInfo.FileName = "GameServer.exe";
                            hproces.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                            hproces.Start();
                            Environment.Exit(0);
                            break;
                        }
                    case "automaintenance":
                        {
                            Console.WriteLine("Auto Maintenance [Enabled]");
                            Thread.Sleep(1000);
                            new Thread(new ThreadStart(WaitForMaintenance)).Start();
                            break;
                        }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static DateTime MaintenanceTime = DateTime.Now.AddSeconds(15); // AddHours(24) for real
        public static void WaitForMaintenance()
        {
            if (MaintenanceTime > DateTime.Now)
            {
                new Thread(new ThreadStart(MaintenanceAndRestart)).Start();
            }
        }

        public static void MaintenanceAndRestart()
        {
            int RestartSecondsMessage = 1;//30
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                OnMainternance = true;
                Console.WriteLine("The server will be brought down for maintenance in (5 Minutes). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (5 Minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (4 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (4 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (4 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (4 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (3 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (3 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (3 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (3 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (2 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (2 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (2 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (2 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (1 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (1 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (1 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (1 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (0 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (0 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage - 10);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                MsgMessage msg = new MsgMessage("Server maintenance(few minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * RestartSecondsMessage - 20);
            ProcessConsoleEvent(0);
            System.Diagnostics.Process hproces = new System.Diagnostics.Process();
            hproces.StartInfo.FileName = "GameServer.exe";
            hproces.StartInfo.Arguments = "automaintenance";
            hproces.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            hproces.Start();
            Environment.Exit(0);
        }
        public static void Maintenance()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                OnMainternance = true;
                Console.WriteLine("The server will be brought down for maintenance in (5 Minutes). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (5 Minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (4 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (4 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (4 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (4 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (3 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (3 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (3 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (3 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (2 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (2 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (2 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (2 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (1 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (1 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (1 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (1 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (0 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (0 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 20);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                MsgMessage msg = new MsgMessage("Server maintenance(few minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 10);
            ProcessConsoleEvent(0);
            Environment.Exit(0);
        }

        public unsafe static void CreateDHKey(ServerSockets.SecuritySocket obj, ServerSockets.Packet Stream)
        {
            try
            {
#if TEST
                Console.WriteLine("CreateDHKey");
#endif
                int SizeBuffer = 36;
                byte[] buffer = new byte[SizeBuffer];
                bool extra = false;
                string text = Encoding.UTF8.GetString(obj.DHKeyBuffer.buffer, 0, obj.DHKeyBuffer.Length());
                if (!text.EndsWith("TQClient"))
                {
                    System.Buffer.BlockCopy(obj.EncryptedDHKeyBuffer.buffer, obj.EncryptedDHKeyBuffer.Length() - SizeBuffer, buffer, 0, SizeBuffer);
                    extra = true;
                }
                string key;
                if (Stream.GetHandshakeReplyKey(out key))
                {
#if TEST
                    Console.WriteLine("GetHandshakeReplyKey");
#endif
                    obj.SetDHKey = true;
                    obj.Game.DHKey.HandleResponse(key);
                    var compute_key = obj.Game.DHKeyExchance.PostProcessDHKey(obj.Game.DHKey.ToBytes());
                    //obj.Game.Crypto.SetIVs(new byte[8], new byte[8]);
#if TEST
                    Console.WriteLine("GenerateKey");
#endif
                    obj.Game.Crypto.GenerateKey(compute_key);
                    obj.Game.Crypto.Reset();
                }
                else
                {
                    obj.Disconnect();
                    return;
                }
                if (extra)
                {

                    Stream.Seek(0);
                    obj.Game.Crypto.Decrypt(buffer, 0, Stream.Memory, 0, SizeBuffer);
                    Stream.Size = buffer.Length;
                    Stream.Seek(2);
                    ushort PacketID = Stream.ReadUInt16();
                    Action<Client.GameClient, ServerSockets.Packet> hinvoker;
                    if (MsgInvoker.TryGetInvoker(PacketID, out hinvoker))
                    {
                        //Console.WriteLine($"Packet ID: {PacketID}");
                        hinvoker(obj.Game, Stream);
                    }
                    else
                    {
                        obj.Disconnect();
                        Console.WriteLine("DH KEY Not found the packet ----> " + PacketID);
                    }
                }

            }
            catch (Exception e) { Console.WriteException(e); }
        }
        public static bool NameStrCheck(string name, bool ExceptedSize = true)
        {
            if (name == null)
                return false;
            if (name == "")
                return false;
            string ValidChars = "[^A-Za-z0-9ء-ي*~.&.$]$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(ValidChars);
            if (r.IsMatch(name))
                return false;
            if (name.ToLower().Contains("none"))
                return false;
            if (name.ToLower().Contains("Vs"))
                return false;
            if (name.ToLower().Contains("gm"))
                return false;
            if (name.ToLower().Contains("pm"))
                return false;
            if (name.ToLower().Contains("p~m"))
                return false;
            if (name.ToLower().Contains("p!m"))
                return false;
            if (name.ToLower().Contains("g~m"))
                return false;
            if (name.ToLower().Contains("g!m"))
                return false;
            if (name.ToLower().Contains("help"))
                return false;
            if (name.ToLower().Contains("desk"))
                return false;
            if (name.Contains('/'))
                return false;
            if (name.Contains(@"\"))
                return false;
            if (name.Contains(@"'"))
                return false;
            if (name.Contains("GM") ||
                name.Contains("PM") ||
                name.Contains("SYSTEM") ||
                name.Contains("{") || name.Contains("}") || name.Contains("[") || name.Contains("]"))
                return false;
            if (name.Length > 16 && ExceptedSize)
                return false;
            for (int x = 0; x < name.Length; x++)
                if (name[x] == 25)
                    return false;
            return true;
        }
        public static bool StringCheck(string pszString)
        {
            for (int x = 0; x < pszString.Length; x++)
            {
                if (pszString[x] > ' ' && pszString[x] <= '~')
                    return false;
            }
            return true;
        }
    }
}
