using Core;
using Core.Models.GameServer;
using Core.Models.SharedConfig;
using GameServer.Game.MsgFloorItem;
using GameServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GameServer.Pool;
namespace GameServer.Database
{
    public class Server
    {
        public static string LastChar = "None.";

        public static unsafe void Reset(DateTime Clock)
        {
            //if (Clock > ResetStamp)
            {
                if (DateTime.Now.Hour != 0)
                    ResetedAlready = false;
                if (!ResetedAlready && DateTime.Now.Hour == 0)
                {
                    try
                    {
                        Console.WriteLine("Reseting Arena.");
                        ResetedAlready = true;

                        Arena.ResetArena();
                        TeamArena.ResetArena();

                        foreach (var flowerclient in Role.Instance.Flowers.ClientPoll.Values)
                        {
                            foreach (var flower in flowerclient)
                                flower.Amount2day = 0;
                        }
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            foreach (var client in GamePoll.Values)
                            {
                                client.Player.TodayChampionPoints = 0;
                                client.Player.UseChiToken = 0;
                                client.MiningAttempts = 500;
                                client.Player.QuestGUI.RemoveQuest(6126);
                                client.Player.OpenHousePack = 0;
                                if (client.Player.DailyMonth == 0)
                                    client.Player.DailyMonth = (byte)DateTime.Now.Month;
                                if (client.Player.DailyMonth != DateTime.Now.Month)
                                {
                                    client.Player.DailySignUpRewards = 0;
                                    client.Player.DailySignUpDays = 0;
                                    client.Player.DailyMonth = (byte)DateTime.Now.Month;
                                }
                                client.Player.DbTry = false;
                                client.Player.LotteryEntries = 0;
                                client.Player.QuestEntries = 0;
                                client.Player.Quest2Entries = 0;
                                client.Player.Quest3Entries = 0;
                                client.Player.MonsterEntries = 0;
                                client.Player.MonsterEntries2 = 0;
                                client.Player.VirtuteEntries = 0;
                                client.Player.Day = DateTime.Now.DayOfYear;
                                client.Player.BDExp = 0;
                                client.Player.TCCaptainTimes = 0;
                                client.Player.ExpBallUsed = 0;
                                client.Player.MysteryFruit = 0;
                                client.DemonExterminator.FinishToday = 0;
                                if (ServerConfig.EnabledChi && client.Player.MyChi != null) // [feature-gate progression.chi]
                                {
                                    client.Player.MyChi.ChiPoints = client.Player.MyChi.ChiPoints + 300;
                                    MsgChiInfo.MsgHandleChi.SendInfo(client, MsgChiInfo.Action.Upgrade);
                                }
                                if (client.Player.Flowers != null)
                                {
                                    client.Player.Flowers.FreeFlowers = 1;
                                    foreach (var flower in client.Player.Flowers)
                                        flower.Amount2day = 0;
                                    if (client.Player.Flowers.FreeFlowers > 0)
                                    {
                                        client.Send(stream.FlowerCreate(Role.Core.IsBoy(client.Player.Body)
                                            ? MsgFlower.FlowerAction.FlowerSender
                                            : MsgFlower.FlowerAction.Flower
                                            , 0, 0, client.Player.Flowers.FreeFlowers));
                                    }
                                }
                                if (client.Player.Level >= 90)
                                {
                                    client.Player.Enilghten = ServerDatabase.CalculateEnlighten(client.Player);
                                    client.Player.SendUpdate(stream, client.Player.Enilghten, MsgUpdate.DataType.EnlightPoints);
                                }
                                //client.Player.QuestGUI.RemoveQuest(35024);
                                //client.Player.QuestGUI.RemoveQuest(35007);
                                //client.Player.QuestGUI.RemoveQuest(35025);
                                //client.Player.QuestGUI.RemoveQuest(35028);
                                //client.Player.QuestGUI.RemoveQuest(35034);

                                ////---- reset Quests
                                //client.Player.QuestGUI.RemoveQuest(6390);
                                //client.Player.QuestGUI.RemoveQuest(6329);
                                //client.Player.QuestGUI.RemoveQuest(6245);
                                //client.Player.QuestGUI.RemoveQuest(6049);
                                //client.Player.QuestGUI.RemoveQuest(6366);
                                //client.Player.QuestGUI.RemoveQuest(6014);
                                //client.Player.QuestGUI.RemoveQuest(2375);
                                //client.Player.QuestGUI.RemoveQuest(6126);
                                client.Player.DailyHeavenChance = client.Player.DailyMagnoliaChance
                                    = client.Player.DailyMagnoliaItemId
                                    = client.Player.DailyHeavenChance = client.Player.DailySpiritBeadCount = client.Player.DailyRareChance = 0;
                                //
                            }
                        }
                        ResetServerDay = (uint)DateTime.Now.DayOfYear;
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }
                //ResetStamp.Value = Clock.Value + KernelThread.ResetDayStamp;
            }
        }

        public static void ResetSquamas()
        {
            // Reset squama traps for for all maps
            foreach (var kvP in Pool.ServerMaps.Base)
            {
                uint MapId = kvP.Value.ID;
                NpcServer.LoadServerTraps(MapId, true);
            }
        }
        public static void Initialize()
        {
            ServerMaps = new MapDictionary<uint, Role.GameMap>();
            GamePoll = new ConcurrentDictionary<uint, Client.GameClient>();
            NameUsed = new List<int>();

            GameServerConfig GSConfig = Program.GSConfig;
            string serverMode = Environment.GetEnvironmentVariable("TRINITY_SERVER_MODE");
            if (string.IsNullOrWhiteSpace(serverMode))
                serverMode = string.IsNullOrWhiteSpace(GSConfig.ServerMode) ? "Production" : GSConfig.ServerMode;

            Program.ServerMode = serverMode.Equals("Dev", StringComparison.OrdinalIgnoreCase)
                || serverMode.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? "DEV"
                : "PRODUCTION";

            Program.TestServer = GSConfig.GeneralTestMode || Program.ServerMode == "DEV";
            Console.WriteLine($"Server Mode: {Program.ServerMode}", ConsoleColor.Yellow);
            Console.WriteLine($"Test-Server Mode: {Program.TestServer}", ConsoleColor.Yellow);
            ServerConfig.DbFromFiles =  false;
            if (GSConfig.DbFromFiles != null)
            {
                ServerConfig.DbFromFiles = (bool)GSConfig.DbFromFiles;
            }
            ServerConfig.IPAddres = GSConfig.ServerIPAddres;
            ServerConfig.GamePort = GSConfig.ServerGamePort;
            ServerConfig.ServerName = GSConfig.ServerName;
            ServerConfig.OfficialWebSite = GSConfig.ServerWebsite;
            ServerConfig.ServerOwner = GSConfig.ServerOwner;

            ServerConfig.Port_BackLog = GSConfig.GeneralPortBacklog;
            ServerConfig.Port_ReceiveSize = GSConfig.GeneralPortReceiveSize;
            ServerConfig.Port_SendSize = GSConfig.GeneralPortSendSize;

            ServerConfig.DbLocation = GSConfig.GeneralDatabaseLocation;
            ServerConfig.CO2Folder = GSConfig.GeneralDatabaseLocation;
            Console.WriteLine($"Database: {ServerConfig.DbLocation}", ConsoleColor.Yellow);
            if (!Directory.Exists(ServerConfig.DbLocation))
                throw new DirectoryNotFoundException($"Database folder not found: {ServerConfig.DbLocation} (rebuild the project or set TRINITY_DATABASE)");
            Core.Features.FeatureRegistry.Load(ServerConfig.DbLocation);
            Console.WriteLine(Core.Features.FeatureRegistry.Summary(), ConsoleColor.Yellow);
            GameServer.Telemetry.ServerLog.Init(ServerConfig.DbLocation, Program.ServerMode);
            GameServer.Telemetry.Economy.Init(ServerConfig.DbLocation);
            ServerConfig.EnabledChi = GSConfig.EnabledChi && Core.Features.FeatureRegistry.IsKept("progression.chi");
            ServerConfig.EnabledSubclass = GSConfig.EnabledSubclass && Core.Features.FeatureRegistry.IsKept("progression.subclass");
            ServerConfig.EnabledMentor = GSConfig.EnabledMentor && Core.Features.FeatureRegistry.IsKept("social.mentor");

            // Database Auth Read Config
            AccountServerConfig ASConfig = RestApiHelper.GetRequest<AccountServerConfig>("GetASConfig");
            ServerConfig.DatabaseAuthHostname = ASConfig.DatabaseHostname;
            ServerConfig.DatabaseAuthPort = ASConfig.DatabasePort;
            ServerConfig.DatabaseAuthUsername = ASConfig.DatabaseUsername;
            ServerConfig.DatabaseAuthPassword = ASConfig.DatabasePassword;
            ServerConfig.DatabaseAuthName = ASConfig.DatabaseName;

            ServerConfig.InterServerAddress = GSConfig.InterServerAddress;
            ServerConfig.InterServerPort = GSConfig.InterServerPort;
            ServerConfig.IsInterServer = GSConfig.IsInterServer && Core.Features.FeatureRegistry.IsKept("extras.interserver");

            RebornInfo = new RebornInfomations();
            RebornInfo.Load();

            //ITEM_Counter.Set(GSConfig.GeneralItemUID);
            //uint nextitem = ITEM_Counter.Next;
            ClientCounter.Set(GSConfig.GeneralClientUID);
            uint nextclient = ClientCounter.Next;
            ResetServerDay = GSConfig.GeneralDay;
            Game.MsgTournaments.MsgSchedules.PkWar.WinnerUID = GSConfig.GeneralPKWarWinnerUID;

            ItemsBase = new ItemType();
            RefineryItems = new Refinery();
            DBRerinaryBoxes = new RefinaryBoxes();
            ItemsBase.Loading();
            //Database.Lottery.TestLoad();
            // Database.Lottery.TestLoad();

            //-------------------------- Load shops -------------------
            //Shops.ChampionShop.Load();
            Shops.EShopFile.Load();
            Shops.EShopV2File.Load();
            Shops.HonorShop.Load();
            Shops.RacePointShop.Load();
            Shops.ShopFile.Load();
            // [feature-gate items] tira das lojas os itens de features removidas.
            // Economy V3 também aplica a boundary Era 1 aos NPC shops de Gold.
            {
                Func<uint, bool> blk = itemId =>
                    Core.Features.FeatureRegistry.IsBlockedItem(itemId)
                    || Game.Era1.Era1Items.IsBlockedEquipment(itemId);

                foreach (var sh in Shops.EShopFile.Shops.Values) { sh.Items.RemoveAll(i => blk(i)); sh.BoundItems.RemoveAll(i => blk(i)); }
                foreach (var sh in Shops.ShopFile.Shops.Values) { sh.Items.RemoveAll(i => blk(i)); sh.BoundItems.RemoveAll(i => blk(i)); }
                foreach (var k in Shops.HonorShop.Shop.Items.Keys.Where(k => blk(k)).ToList()) Shops.HonorShop.Shop.Items.Remove(k);
                foreach (var k in Shops.RacePointShop.Shop.Items.Keys.Where(k => blk(k)).ToList()) Shops.RacePointShop.Shop.Items.Remove(k);
            }
            //--------------------------
            SystemBanned.Load();
            SystemBannedAccount.Load();
            ShareVIP.Load();
            LoadExpInfo();
            DataCore.AtributeStatus.Load();
            Role.GameMap.LoadMaps();
            Magic.Load();
            LoadMonsters();
            // Economy V4: classic Market is player vending. The legacy Booths.txt
            // layer is a custom infinite-supply shop system and stays disabled.
            if (Game.Era1.Era1Services.EnablePost5017StaticBooths)
                Booth.Load();
            Tranformation.Int();
            QuestInfo.Init();
            SubClassInfo.Load();
            ChiTable.Load();
            FlowersTable.Load();
            NobilityTable.Load();
            Role.Instance.AssociateGS.Load();
            Game.MsgTournaments.MsgSchedules.GuildWar.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.GuildWar.Load();
            #region
            Game.MsgTournaments.MsgSchedules.PoleDomination.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.PoleDominationBI.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.PoleDominationDC.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.PoleDominationPC.CreateFurnitures();

#endregion
            Game.MsgTournaments.MsgFortressWar.Load();
            Game.MsgTournaments.MsgSchedules.CityWar.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.ClassicClanWar.Create();
            Game.MsgTournaments.MsgSchedules.EliteGuildWar.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Load();
            MsgFilter.Load();
            GuildTable.Load();
            ClanTable.Load();
            QuizShow.Load();
            Game.MsgTournaments.MsgSchedules.ClassPkWar.Load();
            Game.MsgTournaments.MsgSchedules.CouplesPKWar.Load();
            Game.MsgTournaments.MsgSchedules.ElitePkTournament.Load();
            Game.MsgTournaments.MsgSchedules.TeamPkTournament.Load();
            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Load();
            TheCrimeTable.Load();
            Role.Statue.Load();
            Role.KOBoard.KOBoardRanking.Load();
            Disdain.Load();
            Arena.Load();
            TeamArena.Load();
            TutorInfo.Load();
            InfoDemonExterminators.Create();
            NewLottery.LoadLotteryItems();
            QueueContainer.Load();
            GroupServerList.Load();
            FullLoading = true;
        }
        public static byte NameChangeCount(byte vipLevel)
        {
            byte chance = 1;
            switch (vipLevel)
            {
                case 1:
                    chance = 2;
                    break;
                case 2:
                    chance = 3;
                    break;
                case 3:
                    chance = 4;
                    break;
                case 4:
                    chance = 5;
                    break;
                case 5:
                    chance = 10;
                    break;

                case 6:
                    chance = 30;
                    break;
            }
            return chance;
        }
        public static void LoadMapName(uint id)
        {
            string gameMapExIniPath = Path.Combine(ServerConfig.DbLocation, "GameMapEx.ini");
            if (File.Exists(gameMapExIniPath))
            {
                IniFileHelper ini = new(gameMapExIniPath);
                ServerMaps[id].Name = ini.ReadString(id.ToString(), "Name", ServerConfig.ServerName);
            }
        }
        public static void LoadExpInfo()
        {
            if (File.Exists(Path.Combine(ServerConfig.DbLocation, "levexp.txt")))
            {
                using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "levexp.txt")))
                {
                    while (true)
                    {
                        string GetLine = read.ReadLine();
                        if (GetLine == null) return;
                        string[] line = GetLine.Split(' ');
                        DBLevExp exp = new DBLevExp();
                        exp.Action = (DBLevExp.Sort)byte.Parse(line[0]);
                        exp.Level = byte.Parse(line[1]);
                        exp.Experience = ulong.Parse(line[2]);
                        exp.UpLevTime = int.Parse(line[3]);
                        exp.MentorUpLevTime = int.Parse(line[4]);

                        if (!LevelInfo.ContainsKey(exp.Action))
                            LevelInfo.Add(exp.Action, new Dictionary<byte, DBLevExp>());

                        LevelInfo[exp.Action].Add(exp.Level, exp);

                    }
                }
            }
            GC.Collect();
        }
        public static void LoadMyMonsters(uint id)
        {
            try
            {
                using (var reader = new StreamReader(Path.Combine(ServerConfig.DbLocation, "Spawns.txt")))
                {
                    var values = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in values)
                    {
                        var data = line.Split(',');
                        uint ID = uint.Parse(data[0]);
                        uint MapId = uint.Parse(data[1]);
                        if (id != MapId) continue;
                        if (MapId == 1013 || MapId == 1014 || MapId == 1016)
                            continue;
                        Game.MsgMonster.MobCollection colletion = new Game.MsgMonster.MobCollection(MapId);
                        if (colletion.ReadMap())
                        {

                            colletion.LocationSpawn = "";
                            Game.MsgMonster.MonsterFamily famil;
                            if (!MonsterFamilies.TryGetValue(ID, out famil))
                            {
                                continue;
                            }
                            if (Game.MsgMonster.MonsterRole.SpecialMonsters.Contains(famil.ID))
                                continue;
                            Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                            Monster.SpawnX = ushort.Parse(data[2]);
                            Monster.SpawnY = ushort.Parse(data[3]);
                            Monster.MaxSpawnX = (ushort)(Monster.SpawnX + ushort.Parse(data[4]));
                            Monster.MaxSpawnY = (ushort)(Monster.SpawnY + ushort.Parse(data[5]));
                            Monster.MapID = MapId;
                            Monster.SpawnCount = int.Parse(data[6]);
                            Monster.rest_secs = int.Parse(data[7]);
                            if (Monster.MapID == 1011 || Monster.MapID == 3071 || Monster.MapID == 1770 || Monster.MapID == 1771 || Monster.MapID == 1772
                                || Monster.MapID == 1773 || Monster.MapID == 1774 || Monster.MapID == 1775 || Monster.MapID == 1777
                                || Monster.MapID == 1782 || Monster.MapID == 1785 || Monster.MapID == 1786 || Monster.MapID == 1787
                                || Monster.MapID == 1794)
                                Monster.SpawnCount = int.Parse(data[8]);
                            //if (Monster.MapID == 1015 || Monster.MapID == 1020) // Fix monster spawn in bird island or ape city
                            //{
                            //    int limit = 15;
                            //    if (Monster.SpawnCount > limit)
                            //    {
                            //        int oldSpawnCount = Monster.SpawnCount;
                            //        Monster.SpawnCount = limit;
                            //        Console.WriteLine($"Changed SpawnCount x{oldSpawnCount} in monster: {Monster.Name} to x{Monster.SpawnCount}");
                            //    }
                            //}
                            colletion.Add(Monster);
                        }
                    }
                }
                GC.Collect();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void LoadMonsters()
        {
            try
            {
                IniFileHelper ini = new();
                Console.WriteLine($"Loading Monsters...", ConsoleColor.DarkYellow);
                foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Monsters")))
                {
                    ini.LoadFile(fname);
                    Game.MsgMonster.MonsterFamily Family = new Game.MsgMonster.MonsterFamily();
                    Family.ID = ini.ReadUInt32("cq_monstertype", "id", 0);
                    Family.Name = ini.ReadString("cq_monstertype", "name", "INVALID_MOB");

                    Family.Level = ini.ReadUInt16("cq_monstertype", "level", 0);
                    Family.MaxAttack = ini.ReadInt32("cq_monstertype", "attack_max", 0);
                    Family.MinAttack = ini.ReadInt32("cq_monstertype", "attack_min", 0);
                    if (Family.Name == "INVALID_MOB" || Family.Level == 0 || Family.ID == 0 || Family.MinAttack > Family.MaxAttack)
                    {
                        Console.WriteLine("[Error] Error Reading Monster File: " + fname + "", ConsoleColor.Red);
                        continue;
                    }
                    Family.Defense = ini.ReadUInt16("cq_monstertype", "defence", 0);
                    Family.Mesh = (ushort)ini.ReadUInt32("cq_monstertype", "lookface", 0);
                    Family.MaxHealth = ini.ReadInt32("cq_monstertype", "life", 0);
                    Family.ViewRange = 16;
                    Family.AttackRange = ini.ReadSByte("cq_monstertype", "attack_range", 0);
                    Family.Dodge = ini.ReadByte("cq_monstertype", "dodge", 0);
                    Family.DropBoots = ini.ReadByte("cq_monstertype", "drop_shoes", 0);
                    Family.DropNecklace = ini.ReadByte("cq_monstertype", "drop_necklace", 0);
                    Family.DropRing = ini.ReadByte("cq_monstertype", "drop_ring", 0);
                    Family.DropArmet = ini.ReadByte("cq_monstertype", "drop_armet", 0);
                    Family.DropArmor = ini.ReadByte("cq_monstertype", "drop_armor", 0);
                    Family.DropShield = ini.ReadByte("cq_monstertype", "drop_shield", 0);
                    Family.DropWeapon = ini.ReadByte("cq_monstertype", "drop_weapon", 0);
                    Family.DropMoney = (ushort)ini.ReadUInt32("cq_monstertype", "drop_money", 0);
                    Family.DropHPItem = ini.ReadUInt32("cq_monstertype", "drop_hp", 0);
                    Family.DropMPItem = ini.ReadUInt32("cq_monstertype", "drop_mp", 0);
                    Family.Boss = ini.ReadByte("cq_monstertype", "Boss", 0);
                    Family.Defense2 = ini.ReadInt32("cq_monstertype", "defence2", 0);
                    if (Family.Boss != 0)
                        Family.AttackRange = 3;

                    Family.MoveSpeed = ini.ReadInt32("cq_monstertype", "move_speed", 0);
                    Family.AttackSpeed = ini.ReadInt32("cq_monstertype", "attack_speed", 0);
                    Family.SpellId = ini.ReadUInt32("cq_monstertype", "magic_type", 0);

                    Family.ExtraCritical = ini.ReadUInt32("cq_monstertype", "critical", 0);
                    Family.ExtraBreack = ini.ReadUInt32("cq_monstertype", "break", 0);

                    Family.extra_battlelev = ini.ReadInt32("cq_monstertype", "extra_battlelev", 0);
                    Family.extra_exp = ini.ReadInt32("cq_monstertype", "extra_exp", 0);
                    Family.extra_damage = ini.ReadInt32("cq_monstertype", "extra_damage", 0);

                    Family.MagicDefense = ini.ReadUInt32("cq_monstertype", "magic_def", 0);

                    if (Family.Boss == 0 && Family.MaxAttack > 3000)
                    {
                        Family.MaxAttack = Family.MaxAttack / 2;
                        Family.MinAttack = Family.MinAttack / 2;
                    }

                    Family.DropSpecials = new Game.MsgMonster.SpecialItemWatcher[ini.ReadInt32("SpecialDrop", "Count", 0)];
                    for (int i = 0; i < Family.DropSpecials.Length; i++)
                    {
                        //string[] Data = ini.ReadString("SpecialDrop", i.ToString(), "", 32).Split(',');
                        string[] Data = ini.ReadString("SpecialDrop", i.ToString(), "").Split(',');

                        Family.DropSpecials[i] = new Game.MsgMonster.SpecialItemWatcher(uint.Parse(Data[0]), int.Parse(Data[1]));
                    }

                    Family.CreateItemGenerator();
                    Family.CreateMonsterSettings();
                    try
                    {
                        MonsterFamilies.Add(Family.ID, Family);
                    }
                    catch { Console.WriteLine("Error In File " + fname, ConsoleColor.DarkRed); }
                }
                Console.WriteLine($"Monsters loaded ({MonsterFamilies.Count}) successfully.", ConsoleColor.DarkGreen);
                GC.Collect();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void LoadMobSpawns(uint id)
        {
            try
            {
                // CQ Generator only for MoonBox maps and Rascagruta F3, F4, F5, F6
                if (id >= 1043 && id <= 1048 || id == 1762 || id >= 2054 && id <= 2056)
                {
                    IniFileHelper ini = new();
                    Game.MsgMonster.MobCollection colletion = new Game.MsgMonster.MobCollection(id);
                    if (colletion.ReadMap())
                    {
                        foreach (string fmobtype in Directory.GetDirectories(Path.Combine(ServerConfig.DbLocation, "MobSpawns", id.ToString())))
                        {
                            foreach (string ffile in Directory.GetFiles(fmobtype))
                            {
                                ini.LoadFile(ffile);
                                colletion.LocationSpawn = ffile;

                                uint ID = ini.ReadUInt32("cq_generator", "npctype", 0);

                                Game.MsgMonster.MonsterFamily famil;
                                if (!MonsterFamilies.TryGetValue(ID, out famil))
                                {
                                    continue;
                                }
                                if (Game.MsgMonster.MonsterRole.SpecialMonsters.Contains(famil.ID))
                                    continue;
                                Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                                Monster.SpawnX = ini.ReadUInt16("cq_generator", "bound_x", 0);
                                Monster.SpawnY = ini.ReadUInt16("cq_generator", "bound_y", 0);
                                Monster.MaxSpawnX = (ushort)(Monster.SpawnX + ini.ReadUInt16("cq_generator", "bound_cx", 0));
                                Monster.MaxSpawnY = (ushort)(Monster.SpawnY + ini.ReadUInt16("cq_generator", "bound_cy", 0));
                                Monster.MapID = ini.ReadUInt32("cq_generator", "mapid", 0);

                                Monster.SpawnCount = ini.ReadByte("cq_generator", "max_per_gen", 0);//"maxnpc", 0);//max_per_gen", 0);
                                Monster.rest_secs = ini.ReadInt32("cq_generator", "rest_secs", 0);


                                if (Monster.MapID == 1011 || Monster.MapID == 3071 || Monster.MapID == 1770 || Monster.MapID == 1771 || Monster.MapID == 1772
                                    || Monster.MapID == 1773 || Monster.MapID == 1774 || Monster.MapID == 1775 || Monster.MapID == 1777
                                    || Monster.MapID == 1782 || Monster.MapID == 1785 || Monster.MapID == 1786 || Monster.MapID == 1787
                                    || Monster.MapID == 1794)
                                    Monster.SpawnCount = ini.ReadByte("cq_generator", "maxnpc", 0);
                                if (Monster.MapID == 1351 || Monster.MapID == 1352 || Monster.MapID == 1353 || Monster.MapID == 1354)
                                {
                                    Monster.SpawnCount += 4;
                                    Monster.MaxSpawnX += 60;
                                    Monster.MaxSpawnY += 35;
                                    Monster.RespawnTime += 5;
                                    Monster.rest_secs += 7;
                                }
                                if (Monster.MapID == 1002)
                                {
                                    if (Monster.Name == "Pheasant" || Monster.Name == "Turtledove")
                                    {
                                        Monster.SpawnCount += 15;
                                        Monster.MaxSpawnX += 60;
                                        Monster.MaxSpawnY += 45;
                                    }
                                    else continue;
                                }
                                if (Monster.MapID == 1015)
                                {
                                    if (Monster.Name == "Birdman")
                                    {
                                        Monster.SpawnCount = ini.ReadByte("cq_generator", "maxnpc", 0);
                                        Monster.SpawnCount += 90;
                                        Monster.MaxSpawnX += 100;
                                        Monster.MaxSpawnY += 50;
                                    }
                                    if (Monster.Name == "HawKing")
                                    {
                                        Monster.SpawnCount = ini.ReadByte("cq_generator", "maxnpc", 0);
                                        Monster.SpawnCount += 140;
                                        Monster.MaxSpawnX += 100;
                                        Monster.MaxSpawnY += 50;
                                    }
                                }
                                colletion.Add(Monster);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Console.SaveException(e); }
        }
        public static void LoadMapMonsters(uint id, string file)
        {
            if (File.Exists(Path.Combine(ServerConfig.DbLocation, "MobSpawns", file)))
            {
                using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "MobSpawns", file)))
                {
                    while (true)
                    {

                        string aline = read.ReadLine();
                        if (aline != null && aline != "")
                        {
                            try
                            {
                                string[] line = aline.Split(',');
                                uint body = uint.Parse(line[1]);
                                string name = line[2];
                                if (name.Contains("Titan"))
                                    continue;
                                if (name == "WhiteTiger")
                                {

                                }
                                ushort X = ushort.Parse(line[3]);
                                ushort Y = ushort.Parse(line[4]);
                                uint Map = uint.Parse(line[5]);
                                if (id != Map) continue;
                                var GMap = ServerMaps[Map];
                                if (GMap.MonstersCollection == null)
                                {
                                    GMap.MonstersCollection = new Game.MsgMonster.MobCollection(GMap.ID);
                                }
                                else if (GMap.MonstersCollection.DMap == null)
                                    GMap.MonstersCollection.DMap = GMap;
                                foreach (var _monster in MonsterFamilies.Values)
                                {
                                    if (_monster.Name.ToLower() == name.ToLower())
                                    {
                                        Game.MsgMonster.MonsterFamily Monster = _monster.Copy();

                                        Monster.SpawnX = X;
                                        Monster.SpawnY = Y;
                                        Monster.MaxSpawnX = (ushort)(X + 1);
                                        Monster.MaxSpawnY = (ushort)(Y + 1);
                                        Monster.MapID = GMap.ID;
                                        Monster.SpawnCount = 1;

                                        Game.MsgMonster.MonsterRole rolemonster = GMap.MonstersCollection.Add(Monster, false, 0, true);
                                        break;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                break;
                            }
                        }
                        else
                            break;

                    }
                }
            }
        }
        public unsafe static void AddMapMonster(ServerSockets.Packet stream, Role.GameMap map, uint ID, ushort x, ushort y, ushort max_x, ushort max_y, byte count, uint DinamicID = 0, bool RemoveOnDead = true
            , MsgItemPacket.EffectMonsters m_effect = MsgItemPacket.EffectMonsters.None, string streffect = "")
        {
            if (map.MonstersCollection == null)
            {
                map.MonstersCollection = new Game.MsgMonster.MobCollection(map.ID);
            }
            if (map.MonstersCollection.ReadMap())
            {

                Game.MsgMonster.MonsterFamily famil;
                if (MonsterFamilies.TryGetValue(ID, out famil))
                {
                    Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                    Monster.SpawnX = x;
                    Monster.SpawnY = y;
                    Monster.MaxSpawnX = (ushort)(x + max_x);
                    Monster.MaxSpawnY = (ushort)(y + max_y);
                    Monster.MapID = map.ID;
                    Monster.SpawnCount = count;
                    Monster.LavaX = x;
                    Monster.LavaY = y;
                    Game.MsgMonster.MonsterRole rolemonster;
                    ushort nTries = 0;
                    do
                    {
                        rolemonster = map.MonstersCollection.Add(Monster, RemoveOnDead, DinamicID, true);
                        if (rolemonster == null)
                        {
                            nTries++;
                            Monster.SpawnX++;
                            Monster.SpawnY++;
                            Monster.MaxSpawnX++;
                            Monster.MaxSpawnY++;
                        }
                    } while (rolemonster == null && nTries < 5); // 5 tries for respawn each mob
                    if (rolemonster == null)
                    {
                        Console.WriteLine($"Cannot spawn a monster {Monster.Name} on Cords {Monster.SpawnX},{Monster.SpawnY} after 5 tries.");
                        return;
                    }
                    //   map.View.EnterMap<Role.IMapObj>(rolemonster);

                    ActionQuery action = new ActionQuery()
                    {
                        ObjId = rolemonster.UID,
                        Type = ActionType.RemoveEntity
                    };
                    rolemonster.Send(stream.ActionCreate(&action));
                    rolemonster.Send(rolemonster.GetArray(stream, false));

                    if (streffect != null)
                    {
                        rolemonster.SendString(stream, MsgStringPacket.StringID.Effect, streffect);
                    }



                    if (m_effect != MsgItemPacket.EffectMonsters.None && rolemonster != null)
                    {
                        MsgItemPacket effect = MsgItemPacket.Create();
                        effect.m_UID = (uint)m_effect;
                        effect.m_X = rolemonster.X;
                        effect.m_Y = rolemonster.Y;
                        effect.DropType = MsgDropID.Earth;
                        rolemonster.Send(stream.ItemPacketCreate(effect));
                        rolemonster.SendString(stream, MsgStringPacket.StringID.Effect, "glebesword");
                    }
                    if (rolemonster.HitPoints > 65535)
                    {
                        MsgUpdate Upd = new MsgUpdate(stream, rolemonster.UID, 2);
                        stream = Upd.Append(stream, MsgUpdate.DataType.MaxHitpoints, rolemonster.Family.MaxHealth);
                        stream = Upd.Append(stream, MsgUpdate.DataType.Hitpoints, rolemonster.HitPoints);
                        stream = Upd.GetArray(stream);
                        rolemonster.Send(stream);
                    }
                }
            }
        }
        public unsafe static bool AddFloor(ServerSockets.Packet stream, Role.GameMap map, uint ID, ushort x, ushort y, ushort spelllevel, MagicType.Magic dbspell, Client.GameClient Owner, uint GuildID, uint OwnerUID, uint DinamicID = 0, string Name = "", bool RemoveOnDead = true)
        {
            try
            {
                if (map.MonstersCollection == null)
                {
                    map.MonstersCollection = new Game.MsgMonster.MobCollection(map.ID);
                }
                if (map.MonstersCollection.ReadMap())
                {

                    Game.MsgMonster.MonsterFamily famil;
                    if (MonsterFamilies.TryGetValue(1, out famil))
                    {
                        Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                        Monster.SpawnX = x;
                        Monster.SpawnY = y;
                        Monster.MaxSpawnX = (ushort)(x + 1);
                        Monster.MaxSpawnY = (ushort)(y + 1);
                        Monster.MapID = map.ID;
                        Monster.SpawnCount = 1;
                        Game.MsgMonster.MonsterRole rolemonster = map.MonstersCollection.Add(Monster, RemoveOnDead, DinamicID, true);
                        if (rolemonster == null)
                        {
                            //invalid x ,y
                            return false;
                        }
                        rolemonster.Family.ID = ID;
                        rolemonster.IsFloor = true;
                        rolemonster.FloorStampTimer = DateTime.Now.AddSeconds(7);
                        rolemonster.Family.Settings = Game.MsgMonster.MonsterSettings.Lottus;

                        rolemonster.FloorPacket = new MsgItemPacket();
                        rolemonster.FloorPacket.m_UID = rolemonster.UID;
                        rolemonster.FloorPacket.m_ID = ID;
                        rolemonster.FloorPacket.m_X = x;
                        rolemonster.FloorPacket.m_Y = y;
                        rolemonster.FloorPacket.MaxLife = 25;
                        rolemonster.FloorPacket.Life = 25;
                        rolemonster.FloorPacket.DropType = MsgDropID.Effect;
                        rolemonster.FloorPacket.m_Color = 13;
                        rolemonster.FloorPacket.ItemOwnerUID = OwnerUID;
                        rolemonster.FloorPacket.GuildID = GuildID;
                        rolemonster.FloorPacket.FlowerType = 2;//2;
                        rolemonster.FloorPacket.Timer = Role.Core.TqTimer(rolemonster.FloorStampTimer);
                        rolemonster.FloorPacket.Name = Name;
                        rolemonster.DBSpell = dbspell;
                        rolemonster.Family.MaxHealth = 25;
                        rolemonster.HitPoints = 25;
                        rolemonster.OwnerFloor = Owner;
                        rolemonster.SpellLevel = spelllevel;

                        if (rolemonster == null)
                        {
                            Console.WriteLine("Error monster spawn. Server.");
                            return false;
                        }
                        map.View.EnterMap<Role.IMapObj>(rolemonster);
                        rolemonster.Send(rolemonster.GetArray(stream, false));
                        return true;
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
            return false;

        }
        public unsafe static void LoadDatabase()
        {
            try
            {
                if (ServerConfig.DbFromFiles)
                {
                    string usersPath = Path.Combine(ServerConfig.DbLocation, "Users");
                    foreach (string fname in Directory.GetFiles(usersPath))
                    {
                        IniFileHelper ini = new(fname);
                        string name = ini.ReadString("Character", "Name", "");
                        NameUsed.Add(name.GetHashCode());
                    }
                } else
                {
                    List<Player> players = RestApiHelper.GetPlayers();
                    foreach (var player in players)
                    {
                        if (player.Name != null && player.Name != "")
                        {
                            NameUsed.Add(player.Name.GetHashCode());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }

        public unsafe static void AddStake(Role.GameMap Map, ushort x, ushort y, uint uid)
        {

            Role.SobNpc Pole = new Role.SobNpc();

            if (!Map.View.Contain(uid, x, y))
            {
                Pole = new Role.SobNpc();
                Pole.ObjType = Role.MapObjectType.SobNpc;
                Pole.UID = uid;
                Pole.Name = "Stake[Mine]";
                Pole.Type = Role.Flags.NpcType.Stake;
                Pole.Mesh = (Role.SobNpc.StaticMesh)427;
                Pole.Map = Map.ID;
                Pole.X = x;
                Pole.Y = y;
                Pole.HitPoints = 20000;
                Pole.MaxHitPoints = 20000;
                Pole.Sort = 17;
                Map.View.EnterMap<Role.IMapObj>(Pole);
                Map.SetFlagNpc(Pole.X, Pole.Y);
            }
        }
        public unsafe static void SaveDatabase()
        {
            if (!FullLoading)
                return;
            try
            {
                try
                {
                    Save(new Action(Role.Instance.AssociateGS.Save));
                }
                catch (Exception e) { Console.SaveException(e); }
                try
                {
                    Save(new Action(GuildTable.Save));
                }
                catch (Exception e) { Console.SaveException(e); }
                IniFileHelper IniFile = new IniFileHelper();
                //IniFile.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "database.cfg"));
                //IniFile.Write<uint>("Database", "ItemUID", ITEM_Counter.Count);
                //IniFile.Write<uint>("Database", "ClientUID", ClientCounter.Count);
                //IniFile.Write<uint>("Database", "Day", ResetServerDay);
                //IniFile.Write<uint>("Tournaments", "PkWarWinner", Game.MsgTournaments.MsgSchedules.PkWar.WinnerUID);
                Save(new Action(ClanTable.Save));
                Save(new Action(QueueContainer.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.GuildWar.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.EliteGuildWar.Save));

                //Save(new Action(Game.MsgTournaments.MsgSchedules.SuperGuildWar.Save));
                Save(new Action(TheCrimeTable.Save));
                Save(new Action(Arena.Save));
                Save(new Action(TeamArena.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.ClassPkWar.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.ElitePkTournament.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.CityWar.Save));

                Save(new Action(Game.MsgTournaments.MsgSchedules.TeamPkTournament.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Save));
                //Save(new Action((Game.MsgTournaments.MsgSchedules.Tournaments[Game.MsgTournaments.TournamentType.BattleField]
                //    as Game.MsgTournaments.MsgBattleField).Save));
                Save(new Action(SystemBanned.Save));
                Save(new Action(SystemBannedAccount.Save));
                Save(new Action(ShareVIP.Save));
                //Save(new Action(VoteSystem.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.ClanWar.Save));
                IniFile = new IniFileHelper();
                //IniFile.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "database.cfg"));
                //IniFile.Write<uint>("Database", "ItemUID", ITEM_Counter.Count);
                //IniFile.Write<uint>("Database", "ClientUID", ClientCounter.Count);
                //IniFile.Write<uint>("Database", "Day", ResetServerDay);
                //IniFile.Write<uint>("Tournaments", "PkWarWinner", Game.MsgTournaments.MsgSchedules.PkWar.WinnerUID);
                Save(new Action(Role.Statue.Save));
                Save(new Action(Role.KOBoard.KOBoardRanking.Save));
            }
            catch (Exception e) { Console.WriteException(e); }
        }
        public static void Save(Action obj)
        {
            try
            {
                obj.Invoke();
            }
            catch (Exception e) { Console.SaveException(e); }
        }
        public static void LoadPortals(uint id = 0)
        {
            if (ServerConfig.DbFromFiles)
            {
                if (File.Exists(Path.Combine(ServerConfig.DbLocation, "portals.ini")))
                {
                    using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "portals.ini")))
                    {
                        ushort count = 0;
                        while (true)
                        {
                            string lines = read.ReadLine();
                            if (lines == null)
                                break;
                            ushort Map = ushort.Parse(lines.Split('[')[1].ToString().Split(']')[0]);
                            ushort Count = ushort.Parse(read.ReadLine().Split('=')[1]);
                            for (ushort x = 0; x < Count; x++)
                            {
                                Role.Portal portal = new Role.Portal();
                                string[] line = read.ReadLine().Split('=')[1].Split(' ');
                                portal.MapID = ushort.Parse(line[0]);
                                portal.X = ushort.Parse(line[1]);
                                portal.Y = ushort.Parse(line[2]);

                                string[] dline = read.ReadLine().Split('=')[1].Split(' ');
                                portal.Destiantion_MapID = ushort.Parse(dline[0]);
                                portal.Destiantion_X = ushort.Parse(dline[1]);
                                portal.Destiantion_Y = ushort.Parse(dline[2]);
                                if (id != 0 && id != Map) continue;
                                if (ServerMaps.ContainsKey(portal.MapID))
                                    ServerMaps[portal.MapID].Portals.Add(portal);
                                count++;
                            }
                        }
                        Console.WriteLine($"Loaded {count} portals in Map with ID {id}", ConsoleColor.DarkGray);
                    }
                }
                GC.Collect();
            }
            else
            {
                IQueryable<Portal> portals = RestApiHelper.GetRequest<List<Portal>>("Portals/Get").AsQueryable();

                if (id > 0)
                {
                    portals = portals.Where(x => x.MapID == id);
                }

                foreach (var portal in portals.ToList())
                {
                    if (id != 0 && id != portal.MapID) continue;
                    if (ServerMaps.ContainsKey(portal.MapID))
                    {
                        ServerMaps[portal.MapID].Portals.Add(new Role.Portal() { Destiantion_MapID = portal.DestinationMapID, Destiantion_X = portal.DestinationX, Destiantion_Y = portal.DestinationY, MapID = portal.MapID, X = portal.X, Y = portal.Y });
                    }
                }
                Console.WriteLine($"Loaded {portals.Count()} portals in Map with ID {id}", ConsoleColor.DarkGray);
            }
        }
    }
}
