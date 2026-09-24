using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Pool;

namespace GameServer.Game.MsgTournaments
{
    public class MsgSchedules
    {
        public static ITournament CurrentTournament;
        public static CityWars CityWar;
        public static MsgCouples CouplesPKWar;
        internal static MsgGuildWar GuildWar;
        internal static MsgEliteGuildWar EliteGuildWar;
        internal static MsgArena Arena;
        internal static MsgPoleDomination PoleDomination;
        internal static MsgClassicClanWar ClassicClanWar;
        internal static MsgPoleDominationBI PoleDominationBI;
        internal static MsgPoleDominationDC PoleDominationDC;
        internal static MsgPoleDominationPC PoleDominationPC;
        internal static MsgTeamArena TeamArena;
        internal static MsgClassPKWar ClassPkWar;
        internal static MsgEliteTournament ElitePkTournament;
        internal static MsgTeamPkTournament TeamPkTournament;
        internal static MsgSkillTeamPkTournament SkillTeamPkTournament;
        internal static MsgCaptureTheFlag CaptureTheFlag;
        internal static MsgClanWar ClanWar;
        internal static MsgPkWar PkWar;
        private static bool ConfirmTime24()
        {
            return (DateTime.Now.Hour == 2 ||
                DateTime.Now.Hour == 4 ||
                DateTime.Now.Hour == 6 ||
                DateTime.Now.Hour == 8 ||
                DateTime.Now.Hour == 10 ||
                DateTime.Now.Hour == 12 ||
                DateTime.Now.Hour == 14 ||
                DateTime.Now.Hour == 16 ||
                DateTime.Now.Hour == 18 ||
                DateTime.Now.Hour == 20 ||
                DateTime.Now.Hour == 22 ||
                DateTime.Now.Hour == 24);
        }
        internal static void Create()
        {
            Tournaments.Add(TournamentType.QuizShow, new MsgQuizShow(TournamentType.QuizShow));
            Tournaments.Add(TournamentType.DragonWar, new DragonWar(TournamentType.DragonWar));
            Tournaments.Add(TournamentType.PassTheBomb, new PassTheBomb(TournamentType.PassTheBomb));
            Tournaments.Add(TournamentType.TopFight, new MsgTopFight(TournamentType.TopFight));

            Tournaments.Add(TournamentType.LastManStand, new MsgLastManStand(TournamentType.LastManStand));
            Tournaments.Add(TournamentType.TreasureThief, new MsgTreasureThief(TournamentType.TreasureThief));
            Tournaments.Add(TournamentType.KingOfTheHill, new MsgKingOfTheHill(TournamentType.KingOfTheHill));
            Tournaments.Add(TournamentType.FrozenSky, new FrozenSky(TournamentType.FrozenSky));
            Tournaments.Add(TournamentType.FiveNOut, new Fivenout(TournamentType.FiveNOut));

            CurrentTournament = Tournaments[TournamentType.LastManStand];

            GuildWar = new MsgGuildWar();
            EliteGuildWar = new MsgEliteGuildWar();
            ClassicClanWar = new MsgClassicClanWar();
            Arena = new MsgArena();
            TeamArena = new MsgTeamArena();
            ClassPkWar = new MsgClassPKWar(ProcesType.Dead);
            #region guild war
            PoleDomination = new MsgPoleDomination();
            PoleDominationBI = new MsgPoleDominationBI();
            PoleDominationDC = new MsgPoleDominationDC();
            PoleDominationPC = new MsgPoleDominationPC();
            #endregion
            ElitePkTournament = new MsgEliteTournament();
            CaptureTheFlag = new MsgCaptureTheFlag();
            CityWar = new CityWars();
            PkWar = new MsgPkWar();
            CouplesPKWar = new MsgCouples();
            TeamPkTournament = new MsgTeamPkTournament();
            SkillTeamPkTournament = new MsgSkillTeamPkTournament();
            MsgBroadcast.Create();
        }

        /// <summary>Era 1 (5017): o Frozen Grotto nao existe; a Lava Beast nasce no Labyrinth 4.</summary>
        public const uint LavaBeastMap = 1354;

        public static void SpawnLavaBeast(bool firstwork = false)
        {
            Role.GameMap Map;
            if (!ServerMaps.TryGetValue(LavaBeastMap, out Map) || Pool.LavaBeast.Count == 0)
                return;
            // Sorteia uma posicao livre; descarta posicoes que nao sao andaveis neste mapa.
            int Loc = -1;
            for (int tries = 0; tries < Pool.LavaBeast.Count * 2 && Loc < 0; tries++)
            {
                int candidate = Pool.GetRandom.Next(0, Pool.LavaBeast.Count);
                var c = Pool.LavaBeast[candidate];
                if (Map.ValidLocation((ushort)c.X, (ushort)c.Y))
                    Loc = candidate;
            }
            if (Loc < 0)
            {
                Console.WriteLine("LavaBeast: nenhuma posicao valida no mapa " + LavaBeastMap);
                return;
            }
            var spawnLoc = Pool.LavaBeast[Loc];
            LavaBeast.RemoveAt(Loc);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                string msg = $"LavaBeast has spawned in the Labyrinth (4th floor) at {spawnLoc.X},{spawnLoc.Y}! Hurry find it and kill it.";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.TopLeft).GetArray(stream));
                Database.Server.AddMapMonster(stream, Map, 20055, (ushort)spawnLoc.X, (ushort)spawnLoc.Y, 1, 1, 1);
                if (!firstwork)
                    Console.WriteLine($"Spawned Lava Beast at {spawnLoc.X},{spawnLoc.Y}");
            }
            GC.Collect();
        }

        internal static void SendInvitation(string Name, string Prize, ushort X, ushort Y, ushort map, ushort DinamicID, int Seconds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            string Message = " " + Name + " is about to begin! Will you join it?";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();


                var packet = new Game.MsgServer.MsgMessage(Message + Prize, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream);
                foreach (var client in GamePoll.Values)
                {
                    if (!client.Player.OnMyOwnServer || client.IsConnectedInterServer())
                        continue;
                    client.Player.MessageBox(Message, new Action<Client.GameClient>(user => user.Teleport(X, Y, map, DinamicID)), null, Seconds, messaj);
                }
            }
        }
        internal static void SendInvitation2(string Name, ushort X, ushort Y, ushort map, ushort DinamicID, int Seconds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None, byte minlevel = 1, byte maxlevel = 140)
        {
            string Message = " " + Name + " has spawned! Hurry and kill it.";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var client in GamePoll.Values)
                {
                    if (client.Player.Level >= minlevel && client.Player.Level <= maxlevel)
                    {
                        if (!client.Player.OnMyOwnServer || client.IsConnectedInterServer())
                            continue;
                        client.Player.MessageBox(Message, new Action<Client.GameClient>(user => user.Teleport(X, Y, map, DinamicID)), null, Seconds, messaj);
                    }
                }
            }
        }
        internal unsafe static void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.TopLeft
           , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.red, bool SendScren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var packet = new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream);
                foreach (var client in GamePoll.Values)
                    client.Send(packet);
            }
        }
        static List<string> SystemMsgs = new List<string>() {
           "Selling/trading cps outside the game will lead to your accounts banned forever.",
            "Join our discord group to be in touch with the community and suggest/report stuff.",
            "Administrators have [GM/PM] in their names,do not trust anyone else claiming to be a [GM/PM].",
            "Refer our server and gain rewards! (contact GM/PM).",
            "Thanks for supporting us! we will keep on working to provide the best for you!",
            "Check out Guide in TwinCity for information about the game.",
            "Sharing accounts is done at your own risk. You alone are responsible for your own accounts, Support will not be given on cases for shared accounts.",
            "Always treat the STAFF of TrinityConquer with the utmost respect. No insulting/cursing about them or the server.",
            "It's forbidden to advertise any other servers. Your account will be permanently banned without prior notice/warning, Repeated offenses will result in your IP Address being permanently banned.",
            "It's forbidden to abuse bugs or any kind of bug/glitch found in the game, If a player discovers a bug/glitch in the game, it must be reported in Facebook or to the first STAFF member you can find.",
            "It's forbidden to use Bots/Hacks/Cheats in-game. If you find any working Bots/Hacks/Cheats please report them to our STAFF.",
            "Mouse clickers are allowed as long as you're not away-from-keyboard. If you're found using any mouse clicker or macro while away you'll be botjailed.",
            "Only English is allowed in the world chat.",
            "Selling/Trading accounts/items/gold outside the game for real life currencies, for items in other servers or for any other exchange or just the attempt of doing so, will result in all your accounts being permanently banned."
        };

        public static bool WeeklyDrop = false;

        public static DateTime WaterLordStillTime = DateTime.Now;

        public static DateTime WaterLordTime = DateTime.Now;

        internal static DateTime NextLavaBeast;
        private static int NextBoss = 0;
        internal static int LavaBeastsCount = 0;
        public static bool TCBossInv = false, TCBossLaunched = false, SwordINV = false, TeratoINV = false, TeratoLaunched, SwordLaunched = false, HourlyBossInv = false, HourlyBossLaunched = false;
        internal static void CheckUp(DateTime clock)
        {
            DateTime Now64 = DateTime.Now;
            if (!FullLoading)
                return;
            if (Arena.Proces == ProcesType.Dead)
            {
                Arena.Proces = ProcesType.Alive;
            }
            if (TeamArena.Proces == ProcesType.Dead)
            {
                TeamArena.Proces = ProcesType.Alive;
            }
            try
            {
                #region SystemMsgs in game
                if (Now64.Minute % 10 == 0 && Now64.Second > 58)
                {
                    var rndMsg = SystemMsgs[Pool.GetRandom.Next(0, SystemMsgs.Count)];
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(rndMsg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    }
                }
                #endregion

                if (Now64.Minute == 26 && Now64.Second <= 3)
                    NextBoss = Role.Core.Random.Next(0, 2);

                if (CaptureTheFlag.Proces == ProcesType.Alive)
                {
                    CaptureTheFlag.UpdateMapScore();
                    CaptureTheFlag.CheckUpX2();
                    CaptureTheFlag.SpawnFlags();
                }

                PkWar.CheckUp();
                if (global::Core.Features.FeatureRegistry.IsKept("events.couples")) // [feature-gate events.couples]
                    CouplesPKWar.CheckUp();
                CurrentTournament.CheckUp();


                #region Hourly PVP Events
                if (global::Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) // [feature-gate events.custom-minigames]
                {
                Random Rand = new Random();
                if (CurrentTournament.Process == ProcesType.Dead)
                {
                    if (Now64.Minute == 01 && Now64.Second < 1 || Now64.Minute == 10 && Now64.Second < 1 || Now64.Minute == 55 && Now64.Second < 1)
                    {
                        var X = Rand.Next(0, 6);
                        {
                            #region PassTheBomb
                            if (X == 0)
                            {
                                CurrentTournament.Open();
                                CurrentTournament = Tournaments[TournamentType.PassTheBomb];

                            }
                            #endregion
                            #region DragonWar
                            if (X == 1)
                            {
                                CurrentTournament = Tournaments[TournamentType.DragonWar];
                                CurrentTournament.Open();
                            }
                            #endregion
                            #region KingOfTheHill 2
                            if (X == 2)
                            {
                                CurrentTournament = Tournaments[TournamentType.KingOfTheHill];
                                CurrentTournament.Open();
                            }
                            #endregion
                            #region FiveNOut
                            if (X == 3)
                            {
                                CurrentTournament = Tournaments[TournamentType.FiveNOut];
                                CurrentTournament.Open();
                            }
                            #endregion
                            #region FrozenSky
                            if (X == 4)
                            {
                                CurrentTournament = Tournaments[TournamentType.FrozenSky];
                                CurrentTournament.Open();
                            }
                            #endregion
                            #region LastManStand
                            if (X == 5)
                            {
                                CurrentTournament = Tournaments[TournamentType.LastManStand];
                                CurrentTournament.Open();

                            }
                            #endregion
                        }
                    }


                }
                } // [feature-gate events.custom-minigames]
                #endregion

                #region Poles
                if (global::Core.Features.FeatureRegistry.IsKept("events.poledomination")) // [feature-gate events.poledomination]
                {
                #region PoleDomination
                if ((Now64.Hour == 01 || Now64.Hour == 05 || Now64.Hour == 09 || Now64.Hour == 13 || Now64.Hour == 17 || Now64.Hour == 21) && Now64.Minute == 10)
                {
                    if (PoleDomination.Proces == ProcesType.Dead)
                        PoleDomination.Start();
                    if (PoleDomination.Proces == ProcesType.Idle)
                    {
                        if (Now64 > PoleDomination.StampRound)
                            PoleDomination.Began();
                    }
                    if (PoleDomination.Proces != ProcesType.Dead)
                    {
                        if (DateTime.Now > PoleDomination.StampShuffleScore)
                        {
                            PoleDomination.ShuffleGuildScores();
                        }
                    }

                    if (PoleDomination.SendInvitation == false && Now64.Minute == 10)
                    {
                        SendInvitation("ApeCity PoleDomination", "ConquerPoints", 576, 623, 1020, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        PoleDomination.SendInvitation = true;
                        
                    }
                }
                if ((Now64.Hour == 01 || Now64.Hour == 05 || Now64.Hour == 09 || Now64.Hour == 13 || Now64.Hour == 17 || Now64.Hour == 21) && Now64.Minute == 13)
                {
                    if (PoleDomination.Proces == ProcesType.Alive || PoleDomination.Proces == ProcesType.Idle)
                        PoleDomination.CompleteEndGuildWar();
                }
                #endregion
                #region PoleDomination
                if ((Now64.Hour == 02 || Now64.Hour == 06 || Now64.Hour == 10 || Now64.Hour == 14 || Now64.Hour == 18 || Now64.Hour == 22) && Now64.Minute == 10)
                {
                    if (PoleDominationBI.Proces == ProcesType.Dead)
                        PoleDominationBI.Start();
                    if (PoleDominationBI.Proces == ProcesType.Idle)
                    {
                        if (Now64 > PoleDominationBI.StampRound)
                            PoleDominationBI.Began();
                    }
                    if (PoleDominationBI.Proces != ProcesType.Dead)
                    {
                        if (DateTime.Now > PoleDominationBI.StampShuffleScore)
                        {
                            PoleDominationBI.ShuffleGuildScores();
                        }
                    }

                    if (PoleDominationBI.SendInvitation == false && Now64.Minute == 10)
                    {
                        SendInvitation("BirdIland PoleDomination", "ConquerPoints", 718, 573, 1015, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        PoleDominationBI.SendInvitation = true;
                    }
                }
                if ((Now64.Hour == 02 || Now64.Hour == 06 || Now64.Hour == 10 || Now64.Hour == 14 || Now64.Hour == 18 || Now64.Hour == 22) && Now64.Minute == 13)
                {
                    if (PoleDominationBI.Proces == ProcesType.Alive || PoleDominationBI.Proces == ProcesType.Idle)
                        PoleDominationBI.CompleteEndGuildWar();
                }
                #endregion
                #region PoleDomination
                if ((Now64.Hour == 03 || Now64.Hour == 07 || Now64.Hour == 11 || Now64.Hour == 15 || Now64.Hour == 19 || Now64.Hour == 23) && Now64.Minute == 10)
                {
                    if (PoleDominationDC.Proces == ProcesType.Dead)
                        PoleDominationDC.Start();
                    if (PoleDominationDC.Proces == ProcesType.Idle)
                    {
                        if (Now64 > PoleDominationDC.StampRound)
                            PoleDominationDC.Began();
                    }
                    if (PoleDominationDC.Proces != ProcesType.Dead)
                    {
                        if (DateTime.Now > PoleDominationDC.StampShuffleScore)
                        {
                            PoleDominationDC.ShuffleGuildScores();
                        }
                    }

                    if (PoleDominationDC.SendInvitation == false && Now64.Minute == 10)
                    {
                        SendInvitation("DesertCity PoleDomination", "ConquerPoints", 469, 657, 1000, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        PoleDominationDC.SendInvitation = true;
                    }
                }
                if ((Now64.Hour == 03 || Now64.Hour == 07 || Now64.Hour == 11 || Now64.Hour == 15 || Now64.Hour == 19 || Now64.Hour == 23) && Now64.Minute == 13)
                {
                    if (PoleDominationDC.Proces == ProcesType.Alive || PoleDominationDC.Proces == ProcesType.Idle)
                        PoleDominationDC.CompleteEndGuildWar();
                }
              
                #endregion
                #region PoleDomination
                if ((Now64.Hour == 04 || Now64.Hour == 08 || Now64.Hour == 12 || Now64.Hour == 16 || Now64.Hour == 20 || Now64.Hour == 00) && Now64.Minute == 10)
                {
                    if (PoleDominationPC.Proces == ProcesType.Dead)
                        PoleDominationPC.Start();
                    if (PoleDominationPC.Proces == ProcesType.Idle)
                    {
                        if (Now64 > PoleDominationPC.StampRound)
                            PoleDominationPC.Began();
                    }
                    if (PoleDominationPC.Proces != ProcesType.Dead)
                    {
                        if (DateTime.Now > PoleDominationPC.StampShuffleScore)
                        {
                            PoleDominationPC.ShuffleGuildScores();
                        }
                    }

                    if (PoleDominationPC.SendInvitation == false && Now64.Minute == 10)
                    {
                        SendInvitation("PhoenixCastle PoleDomination", "ConquerPoints", 275, 288, 1011, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        PoleDominationPC.SendInvitation = true;
                    }
                }
                if ((Now64.Hour == 04 || Now64.Hour == 08 || Now64.Hour == 12 || Now64.Hour == 16 || Now64.Hour == 20 || Now64.Hour == 00) && Now64.Minute == 13)

                {
                    if (PoleDominationPC.Proces == ProcesType.Alive || PoleDominationPC.Proces == ProcesType.Idle)
                        PoleDominationPC.CompleteEndGuildWar();
                }

                #endregion
                } // [feature-gate events.poledomination]
                #endregion

                #region Fortress
                if (global::Core.Features.FeatureRegistry.IsKept("events.fortress")) // [feature-gate events.fortress]
                {
                MsgFortressWar.CheckUP();
                } // [feature-gate events.fortress]
                #endregion

                #region TeamPkTournament (18:45 Saturday)
                if (global::Core.Features.FeatureRegistry.IsKept("events.skilltournament")) // [feature-gate events.skilltournament]
                {
                if ((Now64.DayOfWeek == DayOfWeek.Saturday) && Now64.Hour == 18 && Now64.Minute == 55)
                {
                    TeamPkTournament.Start();
                }
                } // [feature-gate events.skilltournament]
                #endregion

                #region SkillTeamTournament (19:45 Wednesday)
                if (global::Core.Features.FeatureRegistry.IsKept("events.skilltournament")) // [feature-gate events.skilltournament]
                {
                if ((Now64.DayOfWeek == DayOfWeek.Wednesday) && Now64.Hour == 19 && Now64.Minute == 55)
                {
                    SkillTeamPkTournament.Start();
                }
                } // [feature-gate events.skilltournament]
                #endregion

                #region LavaBeasts
                if (DateTime.Now > NextLavaBeast && LavaBeastsCount > 0)
                {
                    LavaBeastsCount--;
                    if (LavaBeastsCount > 0)
                        NextLavaBeast = DateTime.Now.AddMinutes(3);
                    SpawnLavaBeast();
                }
                #endregion

                #region TreasureThief
                if (global::Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) // [feature-gate events.custom-minigames]
                {
                if (Now64.Minute == 45)
                {
                    CurrentTournament = Tournaments[TournamentType.TreasureThief];
                    CurrentTournament.Open();
                }
                } // [feature-gate events.custom-minigames]
                #endregion

                #region CityWar
                if (global::Core.Features.FeatureRegistry.IsKept("events.citywar") // [feature-gate events.citywar]
                    && Now64.Hour == 11 && Now64.Minute == 00 && Now64.Second == 00)
                {
                    if (CityWar.Proces == ProcesType.Dead)
                        CityWar.Start();
                    if (CityWar.Proces == ProcesType.Idle)
                    {
                        if (Now64 > CityWar.StampRound)
                            CityWar.Began();
                    }
                    if (CityWar.Proces != ProcesType.Dead)
                    {
                        if (DateTime.Now > CityWar.StampRound)
                        {//null
                           
                            //CityWar.ShuffleGuildScores(client.Player.GuildID);
                        }
                    }

                    if (CityWar.SendInvitation == false && Now64.Minute == 00 && Now64.Second == 05)
                    {
                        SendInvitation("CityWar", "ConquerPoints", 425, 368, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        CityWar.SendInvitation = true;
                    }

                }

                if (Now64.Hour == 12 && Now64.Minute == 00 && Now64.Second == 00)
                {
                    if (CityWar.Proces == ProcesType.Alive || CityWar.Proces == ProcesType.Idle)
                        CityWar.CompleteEndGuildWar();
                }
                #endregion

                #region Boss
                #region LavaBeasts
                if (DateTime.Now > NextLavaBeast && LavaBeastsCount > 0)
                {
                    LavaBeastsCount--;
                    if (LavaBeastsCount > 0)
                        NextLavaBeast = DateTime.Now.AddMinutes(3);
                    SpawnLavaBeast();
                }
                #endregion

                #region Boss
                if (ConfirmTime24() && Now64.Minute == 17 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.SnowBanshee);

                if (!ConfirmTime24() && Now64.Minute == 35 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.TeratoDragon);

                if (Now64.Minute == 45 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.ThrillingSpook);

                if (Now64.Minute == 50 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.CornDevil);

                if (Now64.Minute == 40 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.Ganoderma);

                if (Now64.Minute == 6 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.MummySkeleton);

                if (Now64.Minute == 20 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.DarkSpearman);

                if (Now64.Minute == 55 && Now64.Second < 1)
                    MobsHandler.Generate(IDMonster.DarkmoonDemon);
                #endregion



                #endregion

                #region NobilityTournament
                if (global::Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) // [feature-gate events.custom-minigames]
                {
                if (Now64.Hour != 19 && Now64.Minute == 32 && Now64.Second < 4)
                {
                    CurrentTournament = Tournaments[TournamentType.TopFight];
                    CurrentTournament.Open();
                }
                } // [feature-gate events.custom-minigames]
                #endregion

                #region CaptureTheFlag
                if (global::Core.Features.FeatureRegistry.IsKept("events.ctf")) // [feature-gate events.ctf]
                {
                if (Now64.DayOfWeek == DayOfWeek.Wednesday)
                {
                    if (Now64.Hour == 18 && Now64.Second == 05)
                    {
                        CaptureTheFlag.Start();
                    }
                    if (Now64.Hour == 19)
                    {
                        CaptureTheFlag.CheckFinish();
                    }
                }
                } // [feature-gate events.ctf]
                #endregion

                #region ElitePkTournament // Friday 19:55 PM
                if (global::Core.Features.FeatureRegistry.IsKept("events.elitepk")) // [feature-gate events.elitepk]
                {
                if (Now64.Hour == 19 && Now64.Minute == 55 && Now64.Second < 3)
                {
                    ElitePkTournament.Start();
                }
                } // [feature-gate events.elitepk]
                #endregion

                if (Now64.DayOfWeek == DayOfWeek.Sunday) // Each Sunday
                {
                    #region GuildWar
                    if (Now64.Hour < 19) // 19 PM End GuildWar
                    {
                        if (GuildWar.Proces == ProcesType.Dead)
                            GuildWar.Start();
                        if (GuildWar.Proces == ProcesType.Idle)
                        {
                            if (Now64 > GuildWar.StampRound)
                                GuildWar.Began();
                        }
                        if (GuildWar.Proces != ProcesType.Dead)
                        {
                            if (DateTime.Now > GuildWar.StampShuffleScore)
                            {
                                GuildWar.ShuffleGuildScores();
                            }
                        }
                        if (Now64.Hour == 18)
                        {
                            if (GuildWar.FlamesQuest.ActiveFlame10 == false)
                            {
                                SendSysMesage("The Flame Stone 9 is Active now. Light up the Flame Stone (62,59) near the Stone Pole in the Guild City.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                                GuildWar.FlamesQuest.ActiveFlame10 = true;
                            }
                        }
                        else if (GuildWar.SendInvitation == false && Now64.Hour == 18)
                        {
                            SendInvitation("GuildWar", "ConquerPoints, Prizes", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                            GuildWar.SendInvitation = true;
                        }

                    }
                    else
                    {
                        if (GuildWar.Proces == ProcesType.Alive || GuildWar.Proces == ProcesType.Idle)
                        {
                            if (!GuildWar.ManualStarted)
                            {
                                GuildWar.CompleteEndGuildWar();
                            }
                        }
                    }
                    #endregion
                }

                #region KnightGame
                if (global::Core.Features.FeatureRegistry.IsKept("events.knightgame") // [feature-gate events.knightgame]
                    && Now64.Minute == 3 && Now64.Second == 00)
                {
                    SendInvitation("KnightGame", "ConquerPoints, Prizes", 404, 292, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                }
                #endregion

                #region ClassicClanWar


                if (global::Core.Features.FeatureRegistry.IsKept("events.clanwar") // [feature-gate events.clanwar]
                    && Now64.Hour >= 14 && Now64.Hour < 15)
                {
                    if (ClassicClanWar.Proces == ProcesType.Dead)
                        ClassicClanWar.Start();
                    if (ClassicClanWar.Proces == ProcesType.Idle)
                    {
                        if (Now64 > ClassicClanWar.StampRound)
                            ClassicClanWar.Began();
                    }
                    if (ClassicClanWar.Proces != ProcesType.Dead)
                    {
                        if (DateTime.Now > ClassicClanWar.StampShuffleScore)
                        {
                            ClassicClanWar.ShuffleGuildScores();
                        }
                    }

                    if (ClassicClanWar.SendInvitation == false && (Now64.Hour == 14) && Now64.Minute == 00 && Now64.Second >= 00)
                    {
                        SendInvitation("ClassicClanWar", "ConquerPoints", 424, 251, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        ClassicClanWar.SendInvitation = true;
                    }
                }
                else
                {
                    if (ClassicClanWar.Proces == ProcesType.Alive || ClassicClanWar.Proces == ProcesType.Idle)
                        ClassicClanWar.CompleteEndGuildWar();
                }
                
                #endregion

                #region ClassPK // Monday 18:00 PM
                if (global::Core.Features.FeatureRegistry.IsKept("events.classpkwar")) // [feature-gate events.classpkwar]
                {
                if (Now64.DayOfWeek == DayOfWeek.Monday)
                {
                    if (Now64.Hour == 18 && Now64.Minute == 0)
                    {
                        ClassPkWar.Start();
                    }
                    if (Now64.Hour == 18 && Now64.Minute >= 10)
                    {
                        foreach (var war in ClassPkWar.PkWars)
                            foreach (var map in war)
                            {
                                var players_in_map = GamePoll.Values.Where(e => e.Player.DynamicID == map.DinamicID && e.Player.Alive);
                                if (players_in_map.Count() == 1)
                                {
                                    var winner = players_in_map.SingleOrDefault();
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        map.GetMyReward(winner, stream);
                                    }
                                }
                            }
                    }
                }
                } // [feature-gate events.classpkwar]
                #endregion

                #region EliteGuildWar
                if (global::Core.Features.FeatureRegistry.IsKept("events.eliteguildwar")) // [feature-gate events.eliteguildwar]
                {
                if (CaptureTheFlag.Proces != ProcesType.Alive)
                {
                    if (Now64.Hour >= 20 && Now64.Hour < 21)
                    {
                        if (EliteGuildWar.Proces == ProcesType.Dead)
                            EliteGuildWar.Start();
                        if (EliteGuildWar.Proces == ProcesType.Idle)
                        {
                            if (Now64 > EliteGuildWar.StampRound)
                                EliteGuildWar.Began();
                        }
                        if (EliteGuildWar.Proces != ProcesType.Dead)
                        {
                            if (DateTime.Now > EliteGuildWar.StampShuffleScore)
                            {
                                EliteGuildWar.ShuffleGuildScores();
                            }
                        }
                        if (EliteGuildWar.SendInvitation == false && (Now64.Hour == 20) && Now64.Minute == 30 && Now64.Second >= 30)
                        {
                            SendInvitation("EliteGuildWar", "ConquerPoints", 437, 249, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                            EliteGuildWar.SendInvitation = true;
                        }
                    }
                    else
                    {
                        if (EliteGuildWar.Proces == ProcesType.Alive || EliteGuildWar.Proces == ProcesType.Idle)
                            EliteGuildWar.CompleteEndGuildWar();
                    }
                }
                } // [feature-gate events.eliteguildwar]
                #endregion
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
        }
    }
}
