using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class ClanWar
    {
        public CityType Type { get; set; }
        public uint ClanId { get; set; }
        public string PoleName { get; set; }
        public uint WinnerClaimReward { get; set; }
        public uint WinnerNextReward { get; set; }
        public uint WinnerOccupationDays { get; set; }
        public uint WinnerReward { get; set; }
        public uint BestWinnerClanId { get; set; }
        public string BestWinnerName { get; set; }
        public uint BestWinnerClaimReward { get; set; }
        public uint BestWinnerNextReward { get; set; }
        public uint BestWinnerOccupationDays { get; set; }
        public uint BestWinnerReward { get; set; }
    }
}
