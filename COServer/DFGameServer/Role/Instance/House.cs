using System.Collections.Concurrent;
using System.IO;
using Core;
using GameServer.MadeByDaRkFox;

namespace GameServer.Role.Instance
{
    public class House
    {
        public struct DBNpc
        {
            public uint UID;
            public uint UnKnow;
            public ushort X;
            public ushort Y;
            public ushort Mesh;
            public Role.Flags.NpcType NpcType;
            public Role.MapObjectType ObjType;
            public ushort Sort;
            public uint DynamicID;
            public uint Map;

            public static DBNpc Create(Game.MsgNpc.Npc npc)
            {
                DBNpc Dbnpc = new DBNpc();
                Dbnpc.UID = npc.UID;
                Dbnpc.UnKnow = npc.UnKnow;
                Dbnpc.X = npc.X;
                Dbnpc.Y = npc.Y;
                Dbnpc.Mesh = npc.Mesh;
                Dbnpc.NpcType = npc.NpcType;
                Dbnpc.ObjType = npc.ObjType;
                Dbnpc.Sort = npc.Sort;
                Dbnpc.DynamicID = npc.DynamicID;
                Dbnpc.Map = npc.Map;
                return Dbnpc;
            }
            public static Game.MsgNpc.Npc GetServerNpc(DBNpc Dbnpc)
            {
                Game.MsgNpc.Npc npc = new Game.MsgNpc.Npc();
                npc.UID = Dbnpc.UID;
                npc.UnKnow = Dbnpc.UnKnow;
                npc.X = Dbnpc.X;
                npc.Y = Dbnpc.Y;
                npc.Mesh = Dbnpc.Mesh;
                npc.NpcType = Dbnpc.NpcType;
                npc.ObjType = Dbnpc.ObjType;
                npc.Sort = Dbnpc.Sort;
                npc.DynamicID = Dbnpc.DynamicID;
                npc.Map = Dbnpc.Map;
                return npc;
            }
        }
        public House(uint UID)
        {
            if (!HousePoll.ContainsKey(UID))
                HousePoll.TryAdd(UID, this);

            if (!Pool.BlockAttackMap.Contains(UID))
                Pool.BlockAttackMap.Add(UID);
        }
        public static ConcurrentDictionary<uint, House> HousePoll = new ConcurrentDictionary<uint, House>();

        public byte Level = 1;
        public ConcurrentDictionary<uint, Game.MsgNpc.Npc> Furnitures = new ConcurrentDictionary<uint, Game.MsgNpc.Npc>();
        public static PlayerHousesManager PlayerHousesManager = new PlayerHousesManager();

        internal unsafe static void Load(Client.GameClient client)
        {
            PlayerHousesManager.Load(client);
        }
        internal unsafe static void Save(Client.GameClient client)
        {
            PlayerHousesManager.Save(client);
        }
    }
}
