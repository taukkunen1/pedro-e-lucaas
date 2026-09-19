using Core.Interfaces.GameServer;
using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class MsgClanWar
    {
        public class CityWar
        {
            public class ClanWarScrore
            {
                public const int ConquerPointsReward = 0;

                public uint ClainID;
                public string Name;
                public uint Score;
                public uint Reward;
                public uint ClaimReward = 0;
                public uint NextReward;
                public uint OccupationDays;
            }
            public ushort RequestMap, RequestX, RequestY;
            public Role.GameMap Map;
            public Role.SobNpc Pole = new Role.SobNpc();
            public ProcesType Proces;
            public ConcurrentDictionary<uint, ClanWarScrore> ScoreList;
            public ClanWarScrore BestWinner;
            public ClanWarScrore Winner;
            public DateTime StampShuffleScore = new DateTime();
            public uint DynamicID = 0;
            public CityType Type;
            public ClanWarsManager DataManager;
            public CityWar(CityType _type, Role.GameMap _map, ushort npc_x, ushort npc_y, ushort Request_map, ushort Teleport_x, ushort Teleport_y)
            {
                Proces = ProcesType.Dead;
                Type = _type;
                ScoreList = new ConcurrentDictionary<uint, ClanWarScrore>();
                RequestMap = Request_map;
                RequestX = Teleport_x;
                RequestY = Teleport_y;
                Map = _map;

                DynamicID = Map.GenerateDynamicID();
                Winner = new ClanWarScrore() { Name = "None" };
                BestWinner = new ClanWarScrore() { Name = "None" };
                AddNpc(npc_x, npc_y);
                DataManager = new ClanWarsManager();
            }
            public void LoadInfo()
            {
                DataManager.Load(this);
            }
            public void SaveInfo()
            {
                DataManager.Save(this);
            }
            public void Open()
            {
                if (!Core.Features.FeatureRegistry.IsKept("events.clanwar")) return; // [feature-gate]
                if (Proces == ProcesType.Dead)
                {
                    Proces = ProcesType.Alive;
                    MsgSchedules.SendInvitation("" + Type.ToString() + "City ClanWar", "Silver", RequestX, RequestY, RequestMap, 0, 60);
                }
            }
            public void Join(Client.GameClient user)
            {
                if (Proces == ProcesType.Alive)
                {
                    Teleport(user);
                }
            }
            public void Teleport(Client.GameClient user)
            {
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DynamicID);
            }
            internal void UpdateScore(Role.Player client, uint Damage)
            {
                if (client.MyClan == null)
                    return;
                if (Proces == ProcesType.Alive)
                {
                    if (!ScoreList.ContainsKey(client.ClanUID))
                    {
                        ScoreList.TryAdd(client.ClanUID, new ClanWarScrore() { ClainID = client.MyClan.ID, Name = client.MyClan.Name, Score = Damage });
                    }
                    else
                    {
                        ScoreList[client.ClanUID].Score += Damage;
                    }

                    if (Pole.HitPoints == 0)
                        FinishRound();
                }
            }
            public void FinishRound()
            {
                ShuffleGuildScores(true);

                ScoreList.Clear();
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar with a score of " + Winner.Score.ToString() + ""
                       , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar with a score of " + Winner.Score.ToString() + ""
                        , MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    ResetPole();
                }
            }
            public void CompleteWar()
            {
                if (Proces == ProcesType.Alive)
                {
                    ShuffleGuildScores();
                    Proces = ProcesType.Dead;
                    ScoreList.Clear();
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar with a score of " + Winner.Score.ToString() + ""
                                               , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar with a score of " + Winner.Score.ToString() + ""
                           , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.BroadcastMessage).GetArray(stream));

                        Winner.OccupationDays = 1;
                        Winner.NextReward = Winner.Reward = GetItemReward(Type);
                        if (Winner.OccupationDays > BestWinner.OccupationDays)
                        {
                            BestWinner.Name = Winner.Name;
                            BestWinner.ClaimReward = Winner.ClaimReward;
                            BestWinner.ClainID = Winner.ClainID;
                            BestWinner.NextReward = Winner.NextReward;
                            BestWinner.OccupationDays = Winner.OccupationDays;
                            BestWinner.Reward = Winner.Reward;
                        }
                        GetRewards();
                    }
                }
            }
            public static uint GetItemReward(CityType typ)
            {
                switch (typ)
                {
                    case CityType.Twin:
                        return 722454;
                    case CityType.Desert:
                        return 722459;
                    case CityType.Bird:
                        return 722464;
                    case CityType.Ape:
                        return 722469;
                    case CityType.Phoenix:
                        return 722474;
                }
                return 0;
            }
            internal unsafe void ShuffleGuildScores(bool createWinned = false)
            {
                if (Proces != ProcesType.Dead)
                {
                    StampShuffleScore = DateTime.Now.AddSeconds(8);
                    var Array = ScoreList.Values.ToArray();
                    var DescendingList = Array.OrderByDescending(p => p.Score).ToArray();
                    for (int x = 0; x < DescendingList.Length; x++)
                    {
                        var element = DescendingList[x];
                        if (x == 0 && createWinned)
                            Winner = element;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + element.Name + " (" + element.Score.ToString() + ")"
                                , MsgServer.MsgMessage.MsgColor.yellow, x == 0 ? MsgServer.MsgMessage.ChatMode.FirstRightCorner : MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(msg.GetArray(stream));

                        }
                        if (x == 4)
                            break;
                    }
                }
            }
            public void SendMapPacket(ServerSockets.Packet stream)
            {
                foreach (var user in Map.Values)
                {
                    if (user.Player.DynamicID == DynamicID && user.Player.Map == Map.ID)
                        user.Send(stream);
                }
            }
            public bool InWar(Client.GameClient user)
            {
                return user.Player.Map == Map.ID && user.Player.DynamicID == DynamicID;
            }
            public void AddNpc(ushort x, ushort y)
            {
                if (Map.View.Contain(890, x, y))
                    return;
                Pole = new Role.SobNpc();
                Pole.X = x;
                Pole.Map = Map.ID;
                Pole.ObjType = Role.MapObjectType.SobNpc;
                Pole.Y = y;
                Pole.DynamicID = DynamicID;
                Pole.UID = 890;//3333444
                Pole.Type = Role.Flags.NpcType.Stake;
                Pole.Mesh = (Role.SobNpc.StaticMesh)1137;
                Pole.Name = Winner.Name;
                Pole.HitPoints = 30000000;

                Pole.MaxHitPoints = 30000000;
                Pole.Sort = 17;
                Map.View.EnterMap<Role.IMapObj>(Pole);
                Map.SetFlagNpc(Pole.X, Pole.Y);
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in Map.View.Roles(Role.MapObjectType.Player, Pole.X, Pole.Y))
                    {
                        user.Send(Pole.GetArray(stream, false));
                    }
                }
            }
            public void ResetPole()
            {
                Pole.Name = Winner.Name;
                Pole.HitPoints = 30000000;
                Pole.MaxHitPoints = 30000000;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in Map.View.Roles(Role.MapObjectType.Player, Pole.X, Pole.Y))
                    {
                        user.Send(Pole.GetArray(stream, false));
                    }
                }
            }
            public void GetRewards()
            {
                foreach (var user in Map.Values)
                {
                    if (user.Player.DynamicID == DynamicID && user.Player.Map == Map.ID)
                    {
                        if (user.Player.ClanUID == Winner.ClainID)
                        {
                            if (user.Player.MyClan != null && user.Player.MyClanMember != null)
                            {
                                if (user.Player.MyClanMember.Rank == Role.Instance.Clan.Ranks.Leader)
                                {
                                    var value = 3500000 * (uint)Pool.GamePoll.Count;
                                    if (Pool.GamePoll.Count > 40)
                                        value = 3500000 * 40;
                                    user.Player.Money += value;
                                    user.CreateBoxDialog("You received " + value + " Silver.");
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        user.Inventory.Add(stream, Winner.Reward, 1);
                                    }
                                }
                                else
                                {
                                    var value = 3500000 * (uint)Pool.GamePoll.Count;
                                    if (Pool.GamePoll.Count > 40)
                                        value = 3500000 * 40;
                                    user.Player.Money += value / 2;
                                    user.CreateBoxDialog("You received " + value / 2 + " Money.");
                                }
                            }
                        }
                        if (Type == CityType.Bird)
                            user.Teleport(RequestX, RequestY, RequestMap);
                    }
                }
            }
        }

        public CityWar GetWinnerWar(uint ID)
        {
            if (WarCities[CityType.Twin].Winner.ClainID == ID)
                return WarCities[CityType.Twin];
            else if (WarCities[CityType.Phoenix].Winner.ClainID == ID)
                return WarCities[CityType.Phoenix];
            else if (WarCities[CityType.Ape].Winner.ClainID == ID)
                return WarCities[CityType.Ape];
            else if (WarCities[CityType.Desert].Winner.ClainID == ID)
                return WarCities[CityType.Desert];
            else if (WarCities[CityType.Bird].Winner.ClainID == ID)
                return WarCities[CityType.Bird];
            return null;
        }
        public CityWar GetBestWinnerWar(uint ID)
        {
            if (WarCities[CityType.Twin].BestWinner.ClainID == ID)
                return WarCities[CityType.Twin];
            else if (WarCities[CityType.Phoenix].BestWinner.ClainID == ID)
                return WarCities[CityType.Phoenix];
            else if (WarCities[CityType.Ape].BestWinner.ClainID == ID)
                return WarCities[CityType.Ape];
            else if (WarCities[CityType.Desert].BestWinner.ClainID == ID)
                return WarCities[CityType.Desert];
            else if (WarCities[CityType.Bird].BestWinner.ClainID == ID)
                return WarCities[CityType.Bird];
            return null;
        }
        public CityWar CurentWar
        {
            get { return WarCities.Values.Where(p => p.Proces == ProcesType.Alive).FirstOrDefault(); }
        }
        public bool InClanWar(Client.GameClient client)
        {
            var war = CurentWar;
            if (war != null)
                return war.InWar(client);
            return false;

        }
        private CityType GetTournament(uint NpcUID)
        {
            if (NpcUID == 101903)
                return CityType.Twin;
            if (NpcUID == 101907)
                return CityType.Phoenix;
            if (NpcUID == 101911)
                return CityType.Ape;
            if (NpcUID == 101921)
                return CityType.Desert;
            if (NpcUID == 101915)
                return CityType.Bird;
            return CityType.Count;
        }
        public CityWar GetNpcTournament(uint NpcUID)
        {
            try
            {
                var npc = GetTournament(NpcUID);
                if (npc == CityType.Count)
                    return null;
                return WarCities[npc];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        private CityType GetNpcInfo(uint NpcUID)
        {
            if (NpcUID == 101905)
                return CityType.Twin;
            if (NpcUID == 101909)
                return CityType.Phoenix;
            if (NpcUID == 101913)
                return CityType.Ape;
            if (NpcUID == 101919)
                return CityType.Desert;
            if (NpcUID == 101917)
                return CityType.Bird;
            return CityType.Count;
        }
        public CityWar GetNpcInformation(uint NpcUID)
        {
            try
            {
                return WarCities[GetNpcInfo(NpcUID)];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        public ProcesType Process;
        public Dictionary<CityType, CityWar> WarCities;
        public MsgClanWar()
        {
            Process = ProcesType.Dead;
            WarCities = new Dictionary<CityType, CityWar>
            {
                { CityType.Twin, new CityWar(CityType.Twin, Pool.ServerMaps[1505], 165, 219, 1002, 455, 238) },
                { CityType.Phoenix, new CityWar(CityType.Phoenix, Pool.ServerMaps[1509], 82, 119, 1011, 210, 260) },
                { CityType.Ape, new CityWar(CityType.Ape, Pool.ServerMaps[1506], 108, 124, 1020, 568, 583) },
                { CityType.Desert, new CityWar(CityType.Desert, Pool.ServerMaps[1508], 125, 143, 1000, 496, 673) },
                { CityType.Bird, new CityWar(CityType.Bird, Pool.ServerMaps[1507], 95, 113, 1015, 707, 571) }
            };
            if (Pool.FreePkMap.Contains(1505) == false)
                Pool.FreePkMap.Add(1505);
            if (Pool.FreePkMap.Contains(1509) == false)
                Pool.FreePkMap.Add(1509);
            if (Pool.FreePkMap.Contains(1506) == false)
                Pool.FreePkMap.Add(1506);
            if (Pool.FreePkMap.Contains(1508) == false)
                Pool.FreePkMap.Add(1508);
            if (Pool.FreePkMap.Contains(1507) == false)
                Pool.FreePkMap.Add(1507);
            foreach (var war in WarCities.Values)
                war.LoadInfo();
        }
        public void Save()
        {
            foreach (var war in WarCities.Values)
                war.SaveInfo();
        }
        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.clanwar")) return; // [feature-gate]
            if (Process == ProcesType.Dead)
            {
                Process = ProcesType.Alive;
                WarCities[CityType.Twin].Open();
            }
        }

        public void CheckUp(DateTime Timer)
        {
            if (Process == ProcesType.Alive)
            {
                if (CurentWar != null)
                    CurentWar.ShuffleGuildScores(false);

                if (Timer.Minute == 10)
                    if (CurentWar != null)
                        CurentWar.CompleteWar();
                if (Timer.Minute == 11)
                    WarCities[CityType.Phoenix].Open();
                if (Timer.Minute == 21)
                    if (CurentWar != null)
                        CurentWar.CompleteWar();
                if (Timer.Minute == 22)
                    WarCities[CityType.Ape].Open();
                if (Timer.Minute == 33)
                    if (CurentWar != null)
                        CurentWar.CompleteWar();
                if (Timer.Minute == 34)
                    WarCities[CityType.Desert].Open();
                if (Timer.Minute == 45)
                    if (CurentWar != null)
                        CurentWar.CompleteWar();
                if (Timer.Minute == 46)
                    WarCities[CityType.Bird].Open();
                if (Timer.Minute == 57)
                {
                    Process = ProcesType.Dead;
                    if (CurentWar != null)
                        CurentWar.CompleteWar();
                }

            }
        }


    }
}
