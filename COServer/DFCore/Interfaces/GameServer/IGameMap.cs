namespace Core.Interfaces.GameServer
{
    public interface IGameMap
    {
        public uint Uid { get; set; }
        public string Name { get; set; }
        public uint MapDoc { get; set; }
        public ulong TypeStatus { get; set; }
        public ushort RebornMap { get; set; }
        public ushort RebornX { get; set; }
        public ushort RebornY { get; set; }
        public uint RecordSteedRace { get; set; }
        public uint MapColor { get; set; }
    }
}
