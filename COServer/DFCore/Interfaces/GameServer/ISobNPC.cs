using static Core.Enums.SharedEnums;

namespace Core.Interfaces.GameServer
{
    public interface ISobNPC
    {
        public uint UID { get; set; }
        public string Name { get; set; }
        public uint HitPoints { get; set; }
        public uint MaxHitPoints { get; set; }
        public uint Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public MapObjectType ObjType { get; set; }
        public NpcType Type { get; set; }
        public StaticMesh Mesh { get; set; }
        public ushort Sort { get; set; }
    }
}
