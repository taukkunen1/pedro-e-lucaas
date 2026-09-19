namespace Core.Interfaces.GameServer
{
    public interface IArenaUser
    {
        public uint UID { get; set; }
        public string Name { get; set; }
        public ushort Level { get; set; }
        public byte Class { get; set; }
        public uint Mesh { get; set; }
        public uint ArenaPoints { get; set; }
        public uint CurrentHonor { get; set; }
        public uint HistoryHonor { get; set; }
        public uint TodayBattles { get; set; }
        public uint TotalWin { get; set; }
        public uint TotalLose { get; set; }
        public uint TodayWin { get; set; }
        public uint LastSeasonArenaPoints { get; set; }
        public uint LastSeasonWin { get; set; }
        public uint LastSeasonLose { get; set; }
        public uint LastSeasonRank { get; set; }
    }
}
