namespace Core.Interfaces.GameServer
{
    public interface IGuildWar
    {
        public GuildWarType Type { get; set; }
        public uint WinnerGuildID { get; set; }
        public string WinnerName { get; set; }
        public int LeaderReward { get; set; }
        public int DeputiLeaderReward { get; set; }
        public int PoleHitPoints { get; set; }
        public string GuildConductor1 { get; set; }
        public string GuildConductor2 { get; set; }
        public string GuildConductor3 { get; set; }
        public string GuildConductor4 { get; set; }
    }
    public enum GuildWarType
    {
        GuildWar = 0,
        EliteGuildWar = 1,
    }
}