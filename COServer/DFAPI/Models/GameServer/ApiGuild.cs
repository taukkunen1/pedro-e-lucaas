using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("guilds")]
    public class ApiGuild : Guild
    {
        [Key]
        public uint Id { get; set; }
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
        public string Allies { get; set; }
        public string Enemies { get; set; }
    }
}
