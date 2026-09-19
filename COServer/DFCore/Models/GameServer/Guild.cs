using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class Guild : IGuild
    {
        public string GuildName { get; set; }
        public uint GuildID { get; set; }
        public long SilverFund { get; set; }
        public uint ConquerPointFund { get; set; }
        public uint MembersCount { get; set; }
        public uint MyRank { get; set; }
        public uint Level { get; set; }
        public uint CreateTime { get; set; }
        public string LeaderName { get; set; }
        public string Recruit { get; set; }
        public string AdvertiseRecruit { get; set; }
        public string Bulletin { get; set; }
        public bool UseAdvertise { get; set; }
        public uint BuletinEnrole { get; set; }
        public uint CTFExploits { get; set; }
        public uint CTFNextConquerPoints { get; set; }
        public uint CTFNextMoney { get; set; }
        public uint CTFRank { get; set; }
        public uint ClaimCtfReward { get; set; }
        public string Allies { get; set; }
        public string Enemies { get; set; }
        public string Arsenal { get; set; }
    }
}
