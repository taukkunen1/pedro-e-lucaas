using Core.Interfaces.GameServer;
using static Core.Enums.SharedEnums;

namespace Core.Models.GameServer
{
    public class PlayerHouseFurniture : IPlayerHouseFurniture
    {
        public uint UID { get; set; }
        public uint Data { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort Mesh { get; set; }
        public NpcType NpcType { get; set; }
        public MapObjectType ObjType { get; set; }
        public ushort Sort { get; set; }
        public uint DynamicID { get; set; }
        public uint Map { get; set; }
    }
}
