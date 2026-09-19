using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class CityWar : ICityWar
    {
        public CityWarType CityWarType { get; set; }
        public uint GuildId { get; set; }
        public string GuildName { get; set; }
        public uint PoleHitPoints { get; set; }
    }
}
