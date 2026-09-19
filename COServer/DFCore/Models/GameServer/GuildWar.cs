using Core.Interfaces.GameServer;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Core.Models.GameServer
{
    public class GuildWar : IGuildWar
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
        public string RewardLeadersJson { get; set; } = "[]";
        public string RewardDeputiesJson { get; set; } = "[]";
        [NotMapped]
        public List<uint> RewardLeaders
        {
            get => JsonSerializer.Deserialize<List<uint>>(RewardLeadersJson) ?? new();
            set => RewardLeadersJson = JsonSerializer.Serialize(value);
        }
        [NotMapped]
        public List<uint> RewardDeputies
        {
            get => JsonSerializer.Deserialize<List<uint>>(RewardDeputiesJson) ?? new();
            set => RewardDeputiesJson = JsonSerializer.Serialize(value);
        }
    }
}
