using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class MsgClassicClanWar
    {
        public uint DynamicId = 0;
        public bool SendInvitation = false;
        public class GuildWarScore
        {
            public uint ConquerPointsReward = 5000;

            public uint GuildID;
            public string Name;
            public uint Score;

            //for reward
            public int LeaderReward = 1;
            public int DeputiLeaderReward = 7;

            public GuildWarScore() {
                ConquerPointsReward = EventsRewards.EventReward("MsgClassicClanWar").RewardValue;
            }
        }

        public List<uint> RewardLeader = new List<uint>();
        public List<uint> RewardDeputiLeader = new List<uint>();

        public DateTime StampRound = new DateTime();
        public DateTime StampShuffleScore = new DateTime();

        public ProcesType Proces { get; set; }

        public Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc> Furnitures { get; set; }
        public ConcurrentDictionary<uint, GuildWarScore> ScoreList;
        public GuildWarScore Winner;
        public Role.GameMap Map;
        public MsgClassicClanWar()
        {
            Proces = ProcesType.Dead;
            Furnitures = new Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc>();
            ScoreList = new ConcurrentDictionary<uint, GuildWarScore>();
            Winner = new GuildWarScore() { Name = "None", Score = 100, GuildID = 0 };

        }
        public void Create()
        {
            Map = Pool.ServerMaps[1011];
            DynamicId = Map.GenerateDynamicID();

            AddNpc(190, 271);
        }
        public Role.SobNpc Pole;
        public void AddNpc(ushort x, ushort y)
        {
            if (Map.View.Contain(890, x, y))
                return;
            Pole = new Role.SobNpc();
            Pole.X = x;
            Pole.Map = Map.ID;
            Pole.DynamicID = DynamicId;
            Pole.ObjType = Role.MapObjectType.SobNpc;
            Pole.Y = y;
            Pole.DynamicID = DynamicId;
            Pole.UID = 890;//3333444
            Pole.Type = Role.Flags.NpcType.Stake;
            Pole.Mesh = (Role.SobNpc.StaticMesh)8686;
            Pole.Name = Winner.Name;
            Pole.HitPoints = 30000000;

            Pole.MaxHitPoints = 30000000;
            Pole.Sort = 21;
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
            Furnitures.Add(Role.SobNpc.StaticMesh.Pole, Pole);
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
        internal unsafe void ResetFurnitures(ServerSockets.Packet stream)
        {
            ResetPole();
        }
        internal unsafe void SendMapPacket(ServerSockets.Packet packet)
        {
            foreach (var client in Pool.GamePoll.Values)
            {
                if (client.Player.Map == 1011 || client.Player.Map == 6001)
                {
                    client.Send(packet);
                }
            }
        }
        internal unsafe void CompleteEndGuildWar()
        {
            SendInvitation = false;
            ShuffleGuildScores();
            Proces = ProcesType.Dead;
            ScoreList.Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar."
                  , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar."
                   , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.BroadcastMessage).GetArray(stream));
            }

            RewardLeader.Clear();
            Winner.LeaderReward = 1;
        }

        internal unsafe void Start()
        {
            Proces = ProcesType.Alive;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                ResetFurnitures(stream);
                ScoreList.Clear();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("ClanWar war has started!", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
            }
        }

        internal unsafe void FinishRound()
        {
            ShuffleGuildScores(true);
            Furnitures[Role.SobNpc.StaticMesh.Pole].Name = Winner.Name;
            Proces = ProcesType.Idle;
            ScoreList.Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar round with a score of " + Winner.Score.ToString() + ""
                   , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the ClanWar round with a score of " + Winner.Score.ToString() + ""
                    , MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                ResetFurnitures(stream);
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
                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("ClanWar has began!", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
            }
        }
        internal void UpdateScore(Role.Player client, uint Damage)
        {
            if (client.MyGuild == null)
                return;
            if (Proces == ProcesType.Alive)
            {
                if (!ScoreList.ContainsKey(client.ClanUID))
                {
                    ScoreList.TryAdd(client.ClanUID, new GuildWarScore() { GuildID = client.MyClan.ID, Name = client.MyClan.Name, Score = Damage });
                }
                else
                {
                    ScoreList[client.MyClan.ID].Score += Damage;
                }

                if (Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    FinishRound();
            }
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
    }
}
