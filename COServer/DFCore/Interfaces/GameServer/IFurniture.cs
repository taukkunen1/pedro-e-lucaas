using static Core.Enums.SharedEnums;

namespace Core.Interfaces.GameServer
{
    public interface IFurniture
    {
        public uint UID { get; set; }
        public string Name { get; set; }
        public uint ItemID { get; set; }
        public uint MoneyCost { get; set; }
        public ushort Mesh { get; set; }
        public uint Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public NpcType Type { get; set; }
    }
}
