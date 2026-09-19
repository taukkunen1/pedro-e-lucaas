namespace Core.Interfaces.GameServer
{
    public interface ICityWar
    {
        public CityWarType CityWarType { get; set; }
        public uint GuildId { get; set; }
        public string GuildName { get; set; }
        public uint PoleHitPoints { get; set; }
    }
    public enum CityWarType
    {
        TC,
        PC,
        AC,
        DC,
        BI
    }
}
