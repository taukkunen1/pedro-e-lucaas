using Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{


    public class MsgFortressWar
    {
        public static SafeDictionaryAlt<uint, Info> EmperrosList = new SafeDictionaryAlt<uint, Info>();

        public static Info RoundOwner;

        public static ProcesType Proces = ProcesType.Dead;

        public static Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc> Furnitures = new Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc>();
        /// <summary>
        /// SobNpc 
        /// 
        /// ,FortressWar,10,1137,6521,95,113,20000000,20000000,17,1
        /// Npc 1002, 283, 290
        /// 6525,2,9696,1002,283,290
        /// </summary>
        public static ushort MapID = 501;

        public class Info
        {
            public uint UID;
            public uint Score;
            public string Name;
        }

        public static void Load()
        {
            Furnitures.Add(Role.SobNpc.StaticMesh.Pole, Pool.ServerMaps[MapID].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 852));
        }

        public static void Start()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.fortress")) return; // [feature-gate]
            RoundOwner = null;
            EmperrosList = new SafeDictionaryAlt<uint, Info>();
            Proces = ProcesType.Alive;
            MsgSchedules.SendInvitation("FortressWar ", "ConquerPoints", 454, 387, 1002, 0, 100);
            MsgSchedules.SendSysMesage("FortressWar  has been begun.. #19 #19");
        }

        public static void End()
        {
            if (Proces == ProcesType.Alive)
            {
                Proces = ProcesType.Dead;
                MsgSchedules.SendSysMesage(RoundOwner == null ? "FortressWar  has ended and there is no winner" : "" + RoundOwner.Name + ", He is the winner of FortressWar #10 #99.", MsgServer.MsgMessage.ChatMode.Center);
                EmperrosList.Clear();
            }
        }

        public static void CheckUP()
        {
            DateTime Now = DateTime.Now;

            if (Proces == ProcesType.Dead && Now.Minute == 30)//44 mt8yrhash
                Start();

            if (Proces == ProcesType.Alive)
            {
                if (Now.Minute == 38 && Now.Second <= 2)
                    MsgSchedules.SendSysMesage("FortressWar will ended after 2 Minutes Hury For it.", MsgServer.MsgMessage.ChatMode.Center);

                if (Now.Minute == 40 && Now.Second == 0)
                    End();
            }
        }

        public static void Reset(ServerSockets.Packet stream)
        {
            EmperrosList = new SafeDictionaryAlt<uint, Info>();

            foreach (var npc in Furnitures.Values)
                npc.HitPoints = npc.MaxHitPoints;

            var Pole = Furnitures[Role.SobNpc.StaticMesh.Pole];
            var users = Pool.GamePoll.Values.Where(u => Role.Core.GetDistance(u.Player.X, u.Player.Y, Pole.X, Pole.Y) <= Role.SobNpc.SeedDistrance).ToArray();
            if (users != null)
            {
                foreach (var user in users)
                {
                    MsgServer.MsgUpdate upd = new MsgServer.MsgUpdate(stream, Pole.UID, 2);
                    stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Mesh, (long)Pole.Mesh);
                    stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Hitpoints, Pole.HitPoints);
                    stream = upd.GetArray(stream);
                    user.Send(stream);
                    if ((Role.SobNpc.StaticMesh)Pole.Mesh == Role.SobNpc.StaticMesh.Pole)
                        user.Send(Pole.GetArray(stream, false));
                }
            }
        }
        public static void FinishRound(ServerSockets.Packet stream)
        {
            SortScores(true);
            Furnitures[Role.SobNpc.StaticMesh.Pole].Name = RoundOwner.Name;
            MsgSchedules.SendSysMesage("The Round Ownered by #19 #19 #19 #19 #19 #19 #19 #19 " + RoundOwner.Name + ".", MsgServer.MsgMessage.ChatMode.Center);
            Reset(stream);
        }
        public static void UpdateScore(ServerSockets.Packet stream, uint Score, Role.Player Player)
        {
            if (!EmperrosList.ContainsKey(Player.UID))
                EmperrosList.Add(Player.UID, new Info() { Name = Player.Name, UID = Player.UID, Score = Score });
            else
                EmperrosList[Player.UID].Score += Score;

            SortScores();

            if (Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints < 1)
            {
                FinishRound(stream);
                return;
            }
        }
        private static void SortScores(bool getwinner = false)
        {
            if (Proces != ProcesType.Dead)
            {
                var Array = EmperrosList.Values.ToArray();
                var DescendingList = Array.OrderByDescending(p => p.Score).ToArray();
                for (int x = 0; x < DescendingList.Length; x++)
                {
                    var element = DescendingList[x];
                    if (x == 0 && getwinner)
                    {
                        RoundOwner = element;
                    }
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + element.Name + " (" + element.Score + ")"
                           , MsgServer.MsgMessage.MsgColor.yellow, x == 0 ? MsgServer.MsgMessage.ChatMode.FirstRightCorner : MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                        SendMapPacket(msg.GetArray(rec.GetStream()));
                    }
                    if (x == 4)
                        break;
                }
            }
        }
        public static void SendMapPacket(ServerSockets.Packet packet)
        {
            foreach (var client in Pool.ServerMaps[MapID].Values)
                client.Send(packet);
        }
    }
}
