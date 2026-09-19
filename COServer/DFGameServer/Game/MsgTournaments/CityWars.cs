using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class CityWars
    {
        public bool SendInvitation = false;
        public const uint CityMAP = 8839;
        public IDataManager DataManager;


        public class GuildWarScore
        {
            public uint GuildID;
            public string Name;
            public uint Score;

            //for reward
            public int LeaderReward = 1;
            public int DeputiLeaderReward = 7;
        }

        public List<uint> RewardLeader = new List<uint>();
        public List<uint> RewardDeputiLeader = new List<uint>();

        public DateTime StampRound = new DateTime();
        public DateTime StampShuffleScore1 = new DateTime();
        public DateTime StampShuffleScore2 = new DateTime();
        public DateTime StampShuffleScore3 = new DateTime();
        public DateTime StampShuffleScore4 = new DateTime();
        public DateTime StampShuffleScore5 = new DateTime();

        public ProcesType Proces { get; set; }

        public Dictionary<uint, Role.SobNpc> Furnitures { get; set; }
        public ConcurrentDictionary<uint, ConcurrentDictionary<uint, GuildWarScore>> ScoreList;
        public GuildWarScore WinnerTC, WinnerPC, WinnerAC, WinnerDC, WinnerBI;
        void ResetWinners()
        {
            WinnerTC = new GuildWarScore() { Name = "None", Score = 100, GuildID = 0 };
            WinnerPC = new GuildWarScore() { Name = "None", Score = 100, GuildID = 0 };
            WinnerAC = new GuildWarScore() { Name = "None", Score = 100, GuildID = 0 };
            WinnerDC = new GuildWarScore() { Name = "None", Score = 100, GuildID = 0 };
            WinnerBI = new GuildWarScore() { Name = "None", Score = 100, GuildID = 0 };
        }
        public CityWars()
        {
            Proces = ProcesType.Dead;
            Furnitures = new Dictionary<uint, Role.SobNpc>();
            ScoreList = new ConcurrentDictionary<uint, ConcurrentDictionary<uint, GuildWarScore>>();
            ResetWinners();
            DataManager = new CityWarsManager();
        }

        internal void Save()
        {
            DataManager.Save();

        }

        internal void Load()
        {
            DataManager.Load();
        }
        public unsafe void CreateFurnitures()
        {
            Furnitures.Add(821, Pool.ServerMaps[CityMAP].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 821));
            Furnitures.Add(822, Pool.ServerMaps[CityMAP].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 822));
            Furnitures.Add(823, Pool.ServerMaps[CityMAP].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 823));
            Furnitures.Add(824, Pool.ServerMaps[CityMAP].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 824));
            Furnitures.Add(825, Pool.ServerMaps[CityMAP].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 825));
            Load();
        }
        internal unsafe void ResetFurnitures(uint UID, ServerSockets.Packet stream)
        {
            if (UID == 0)
            {
                foreach (var npc in Furnitures.Values)
                    npc.HitPoints = npc.MaxHitPoints;
            }
            else
            {
                Furnitures[UID].HitPoints = Furnitures[UID].MaxHitPoints;

            }
            foreach (var client in Pool.GamePoll.Values)
            {
                if (client.Player.Map == CityMAP)
                {
                    foreach (var npc in Furnitures.Values)
                    {
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, npc.X, npc.Y) <= Role.SobNpc.SeedDistrance)
                        {
                            MsgServer.MsgUpdate upd = new MsgServer.MsgUpdate(stream, npc.UID, 2);
                            stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Mesh, (long)npc.Mesh);
                            stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Hitpoints, npc.HitPoints);
                            stream = upd.GetArray(stream);
                            client.Send(stream);
                            if ((Role.SobNpc.StaticMesh)npc.Mesh == Role.SobNpc.StaticMesh.Pole)
                                client.Send(npc.GetArray(stream, false));
                        }
                    }
                }
            }
        }
        internal unsafe void SendMapPacket(uint UID, ServerSockets.Packet packet)
        {
            foreach (var client in Pool.GamePoll.Values.Where(e => e.Player.Map == CityMAP))
            {
                switch (UID)
                {
                    case 821:
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 131, 127) <= 18)
                            client.Send(packet);
                        break;
                    case 822:
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 133, 253) <= 18)
                            client.Send(packet);
                        break;
                    case 823:
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 253, 253) <= 18)
                            client.Send(packet);
                        break;
                    case 824:
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 253, 125) <= 18)
                            client.Send(packet);
                        break;
                    case 825:
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 195, 207) <= 18)
                            client.Send(packet);
                        break;
                }

            }
        }
        internal unsafe void CompleteEndGuildWar()
        {
            SendInvitation = false;
            ShuffleGuildScores(821);
            ShuffleGuildScores(822);
            ShuffleGuildScores(823);
            ShuffleGuildScores(824);
            ShuffleGuildScores(825);
            Proces = ProcesType.Dead;
            ScoreList.Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                string msg = "";
                if (WinnerTC.Name != "None" && WinnerTC.Score != 100)
                    msg = "Congratulations to " + WinnerTC.Name + " they've won the TwinCity Pole in CityWar";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                msg = "";
                if (WinnerPC.Name != "None" && WinnerPC.Score != 100)
                    msg = "Congratulations to " + WinnerPC.Name + " they've won the PhoenixCity Pole in CityWar";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                msg = "";
                if (WinnerAC.Name != "None" && WinnerAC.Score != 100)
                    msg = "Congratulations to " + WinnerAC.Name + " they've won the ApeCity Pole in CityWar";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                msg = "";
                if (WinnerDC.Name != "None" && WinnerDC.Score != 100)
                    msg = "Congratulations to " + WinnerDC.Name + " they've won the DesertCiy Pole in CityWar";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                msg = "";
                if (WinnerBI.Name != "None" && WinnerBI.Score != 100)
                    msg = "Congratulations to " + WinnerBI.Name + " they've won the BirdsIsland Pole in CityWar";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("CityWars has ended.", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.BroadcastMessage).GetArray(stream));


            }

            RewardLeader.Clear();
            WinnerTC.LeaderReward = 1;
            WinnerPC.LeaderReward = 1;
            WinnerAC.LeaderReward = 1;
            WinnerDC.LeaderReward = 1;
            WinnerBI.LeaderReward = 1;
        }

        internal unsafe void Start()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.citywar")) return; // [feature-gate]
            ResetWinners();
            Proces = ProcesType.Alive;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                ResetFurnitures(0, stream);
                ScoreList.Clear();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("CityWars has started!", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
            }
        }

        internal unsafe void FinishRound(uint UID)
        {


            ShuffleGuildScores(UID, true);
            GuildWarScore Winner = null;
            string pole = "";
            switch (UID)
            {
                case 821: Winner = WinnerTC; pole = "TwinCity"; break;
                case 822: Winner = WinnerPC; pole = "PhoenixCity"; break;
                case 823: Winner = WinnerAC; pole = "ApeCity"; break;
                case 824: Winner = WinnerDC; pole = "DesertCity"; break;
                case 825: Winner = WinnerBI; pole = "BirdsIsland"; break;
            }
            Furnitures[UID].Name = Winner.Name;
            Proces = ProcesType.Idle;
            ScoreList[UID].Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                string msg = $"{Winner.Name} has owned the {pole} pole for this round.";
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg
                   , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg
                    , MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                ResetFurnitures(UID, stream);
            }
            StampRound = DateTime.Now.AddSeconds(3);
        }
        internal unsafe void Began()
        {
            if (Proces == ProcesType.Idle)
            {
                Proces = ProcesType.Alive;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("CityWars has began!", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
            }
        }
        internal void UpdateScore(Role.Player client, uint Damage, uint UID)
        {
            if (client.MyGuild == null)
                return;
            if (Proces == ProcesType.Alive)
            {
                if (!ScoreList.ContainsKey(UID))
                    ScoreList.TryAdd(UID, new ConcurrentDictionary<uint, GuildWarScore>());
                if (!ScoreList[UID].ContainsKey(client.GuildID))
                {
                    ScoreList[UID].TryAdd(client.GuildID, new GuildWarScore() { GuildID = client.MyGuild.Info.GuildID, Name = client.MyGuild.GuildName, Score = Damage });
                }
                else
                {
                    ScoreList[UID][client.MyGuild.Info.GuildID].Score += Damage;
                }

                if (Furnitures[UID].HitPoints == 0)
                    FinishRound(UID);
            }
        }

        internal unsafe void ShuffleGuildScores(uint UID, bool createWinned = false)
        {
            if (Proces != ProcesType.Dead)
            {
                switch (UID)
                {
                    case 821: StampShuffleScore1 = DateTime.Now.AddSeconds(10); break;
                    case 822: StampShuffleScore2 = DateTime.Now.AddSeconds(10); break;
                    case 823: StampShuffleScore3 = DateTime.Now.AddSeconds(10); break;
                    case 824: StampShuffleScore4 = DateTime.Now.AddSeconds(10); break;
                    case 825: StampShuffleScore5 = DateTime.Now.AddSeconds(10); break;

                }

                if (!ScoreList.ContainsKey(UID)) return;
                var Array = ScoreList[UID].Values.ToArray();
                var DescendingList = Array.OrderByDescending(p => p.Score).ToArray();
                if (DescendingList.Length != 0)
                {
                    string msgString = "";
                    switch (UID)
                    {
                        case 821: msgString = "TwinCity Pole"; break;
                        case 822: msgString = "PhoenixCity Pole"; break;
                        case 823: msgString = "ApeCity Pole"; break;
                        case 824: msgString = "DesertCity Pole"; break;
                        case 825: msgString = "BirdsIsland Pole"; break;
                    }
                    Game.MsgServer.MsgMessage msg2 = new MsgServer.MsgMessage(msgString, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendMapPacket(UID, msg2.GetArray(stream));

                        SendMapPacket(UID, new MsgServer.MsgMessage($"TwinCity Owner: {WinnerTC.Name}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                        SendMapPacket(UID, new MsgServer.MsgMessage($"PhoenixCity Owner: {WinnerPC.Name}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                        SendMapPacket(UID, new MsgServer.MsgMessage($"ApeCity Owner: {WinnerAC.Name}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                        SendMapPacket(UID, new MsgServer.MsgMessage($"DesertCity Owner: {WinnerDC.Name}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                        SendMapPacket(UID, new MsgServer.MsgMessage($"Birds Owner: {WinnerBI.Name}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                        SendMapPacket(UID, new MsgServer.MsgMessage($"******************************", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));


                    }
                }
                for (int x = 0; x < DescendingList.Length; x++)
                {
                    var element = DescendingList[x];
                    if (x == 0 && createWinned)
                    {
                        switch (UID)
                        {
                            case 821: WinnerTC = element; break;
                            case 822: WinnerPC = element; break;
                            case 823: WinnerAC = element; break;
                            case 824: WinnerDC = element; break;
                            case 825: WinnerBI = element; break;
                        }
                    }
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + element.Name + " (" + element.Score.ToString() + ")"
                           , MsgServer.MsgMessage.MsgColor.yellow, /*x == 0 ? MsgServer.MsgMessage.ChatMode.FirstRightCorner : */MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                        SendMapPacket(UID, msg.GetArray(stream));

                    }
                    if (x == 4)
                        break;
                }
            }
        }
    }
}
