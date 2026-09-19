using Core;
using GameServer.MadeByDaRkFox;
using System;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class MsgTopFight : ITournament
    {

        public uint King = 0;
        public uint Prince = 0;
        public uint Duke = 0;
        public uint Earl = 0;
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public uint Seconds = 60;
        public Role.GameMap Map1;
        public Role.GameMap Map2;
        public Role.GameMap Map3;
        public Role.GameMap Map4;
        public uint DinamicMap1 = 0;
        public uint DinamicMap2 = 0;
        public uint DinamicMap3 = 0;
        public uint DinamicMap4 = 0;
        public KillerSystem KillSystem;
        public TournamentType Type { get; set; }
        public MsgTopFight(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
            King = EventsRewards.EventReward("MsgTopFightKing").RewardValue;
            Prince = EventsRewards.EventReward("MsgTopFightPrince").RewardValue;
            Duke = EventsRewards.EventReward("MsgTopFightDuke").RewardValue;
            Earl = EventsRewards.EventReward("MsgTopFightEarl").RewardValue;
        }

        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) return; // [feature-gate]
            if (Process == ProcesType.Dead)
            {
                KillSystem = new KillerSystem();
                StartTimer = DateTime.Now;
                MsgSchedules.SendInvitation("TopNobilityWar", "ConquerPoints", 447, 352, 1002, 0, 60);

                if (Map1 == null)
                {
                    Map1 = Pool.ServerMaps[700];
                    DinamicMap1 = Map1.GenerateDynamicID();
                }
                if (Map2 == null)
                {
                    Map2 = Pool.ServerMaps[700];
                    DinamicMap2 = Map2.GenerateDynamicID();
                }
                if (Map3 == null)
                {
                    Map3 = Pool.ServerMaps[700];
                    DinamicMap3 = Map3.GenerateDynamicID();
                }
                if (Map4 == null)
                {
                    Map4 = Pool.ServerMaps[700];
                    DinamicMap4 = Map4.GenerateDynamicID();
                }
                InfoTimer = DateTime.Now;
                Seconds = 60*4;// 4 Minutes
                Process = ProcesType.Idle;
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.King)
                {
                    ushort x = 0;
                    ushort y = 0;
                    Map1.GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map1.ID, DinamicMap1); return true;
                }
                else if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.Prince)
                {
                    ushort x = 0;
                    ushort y = 0;
                    Map2.GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map2.ID, DinamicMap2); return true;
                }
                else if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.Duke)
                {
                    ushort x = 0;
                    ushort y = 0;
                    Map3.GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map3.ID, DinamicMap3); return true;
                }
                else if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.Earl)
                {
                    ushort x = 0;
                    ushort y = 0;
                    Map4.GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map4.ID, DinamicMap4); return true;
                }
            }
            return false;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(4))
                {
                    MsgSchedules.SendSysMesage("Nobility Tournament has begun! registrations are already closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                }
                else if (DateTime.Now > InfoTimer.AddSeconds(10))
                {
                    Seconds -= 10;
                    MsgSchedules.SendSysMesage("[Nobility Tournament] The fight finish in " + Seconds.ToString() + " Seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now;
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(60))
                {
                    foreach (var user in MapPlayers1())
                    {
                        MsgSchedules.SendSysMesage("Nobility Tournament has purposes, nobody wins.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

                    }
                    foreach (var user in MapPlayers2())
                    {
                        MsgSchedules.SendSysMesage("Nobility tournament has purposes, nobody wins.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

                    }
                    foreach (var user in MapPlayers3())
                    {
                        MsgSchedules.SendSysMesage("Nobility tournament has purposes, nobody wins.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

                    }
                    foreach (var user in MapPlayers4())
                    {
                        MsgSchedules.SendSysMesage("Nobility tournament has purposes, nobody wins.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

                    }

                    Process = ProcesType.Dead;
                }
                if (MapPlayers1().Length == 1)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var winner = MapPlayers1().First();
                        MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won Nobility Tournament King/Queen#52 , He/She got " + King.ToString() + " CPs.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        winner.Player.ConquerPoints += King;
                        winner.Player.OnlinePoints += 20;
                        winner.Inventory.Add(rec.GetStream(), 3003748, 1);
                        winner.Teleport(430, 378, 1002);
                    }
                }
                if (MapPlayers2().Length == 1)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var winner = MapPlayers2().First();
                        MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won Tournament Nobility Prince#52  He/She got " + Prince.ToString() + " CPs.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        winner.Player.ConquerPoints += Prince;
                        winner.Player.OnlinePoints += 20;
                        winner.Inventory.Add(rec.GetStream(), 3003748, 1);
                        winner.Teleport(430, 378, 1002);
                    }
                }
                if (MapPlayers3().Length == 1)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var winner = MapPlayers3().First();
                        MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won Tournament Nobility Duke#52  He/She got " + Duke.ToString() + " CPs.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        winner.Player.ConquerPoints += Duke;
                        winner.Player.OnlinePoints += 20;
                        winner.Inventory.Add(rec.GetStream(), 3003748, 1);
                        winner.Teleport(430, 378, 1002);
                    }
                }
                if (MapPlayers4().Length == 1)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var winner = MapPlayers4().First();
                        MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won Tournament Nobility Earl#52  He/She got " + Earl.ToString() + " CPs.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        winner.Player.ConquerPoints += Duke;
                        winner.Player.OnlinePoints += 20;
                        winner.Inventory.Add(rec.GetStream(), 3003748, 1);
                        winner.Teleport(430, 378, 1002);
                    }
                }
                DateTime Timer = DateTime.Now;
                foreach (var user in MapPlayers1())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                        {
                            user.Teleport(430, 378, 1002);
                        }
                    }
                }
                foreach (var user in MapPlayers2())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                        {
                            user.Teleport(430, 378, 1002);
                        }
                    }
                }
                foreach (var user in MapPlayers3())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                        {
                            user.Teleport(430, 378, 1002);
                        }
                    }
                }
                foreach (var user in MapPlayers4())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                        {
                            user.Teleport(430, 378, 1002);
                        }
                    }
                }
                if (MapPlayers1().Length == 0 && MapPlayers2().Length == 0 && MapPlayers3().Length == 0 && MapPlayers4().Length == 0)
                {
                    Process = ProcesType.Dead;
                }

            }
        }

        public Client.GameClient[] MapPlayers1()
        {
            return Map1.Values.Where(p => p.Player.DynamicID == DinamicMap1 && p.Player.Map == Map1.ID).ToArray();
        }
        public Client.GameClient[] MapPlayers2()
        {
            return Map2.Values.Where(p => p.Player.DynamicID == DinamicMap2 && p.Player.Map == Map2.ID).ToArray();
        }
        public Client.GameClient[] MapPlayers3()
        {
            return Map3.Values.Where(p => p.Player.DynamicID == DinamicMap3 && p.Player.Map == Map3.ID).ToArray();
        }
        public Client.GameClient[] MapPlayers4()
        {
            return Map4.Values.Where(p => p.Player.DynamicID == DinamicMap4 && p.Player.Map == Map4.ID).ToArray();
        }
        public bool InTournament(Client.GameClient user)
        {
            if (Map1 == null) return false;
            if (Map2 == null) return false;
            if (Map3 == null) return false;
            if (Map4 == null) return false;
            return user.Player.Map == Map1.ID && user.Player.DynamicID == DinamicMap1 || user.Player.Map == Map2.ID && user.Player.DynamicID == DinamicMap2 || user.Player.Map == Map3.ID && user.Player.DynamicID == DinamicMap3 || user.Player.Map == Map4.ID && user.Player.DynamicID == DinamicMap4;
        }
    }
}
