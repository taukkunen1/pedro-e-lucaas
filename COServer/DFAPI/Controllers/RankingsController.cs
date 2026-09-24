using API.Models.GameServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/rankings")]
    [ApiController]
    public class RankingsController : ControllerBase
    {
        private readonly GameDbContext _db;

        public RankingsController(GameDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PublicRankingsResponse>> Get(int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);

            var players = await _db.Players.AsNoTracking().ToListAsync();
            var guilds = await _db.Guilds.AsNoTracking().ToListAsync();
            var arena = await _db.ArenaUsers.AsNoTracking().ToListAsync();

            var level = players
                .Where(IsPublicPlayer)
                .OrderByDescending(x => x.Reborn)
                .ThenByDescending(x => x.Level)
                .ThenByDescending(x => x.Experience)
                .ThenBy(x => x.Name)
                .Take(limit)
                .Select((x, index) => new PlayerRankingEntry
                {
                    Rank = index + 1,
                    Name = x.Name,
                    Level = x.Level,
                    Reborn = x.Reborn,
                    Class = ClassName(x.Class)
                }).ToList();

            var pk = players
                .Where(IsPublicPlayer)
                .OrderByDescending(x => x.PKPoints)
                .ThenByDescending(x => x.Level)
                .ThenBy(x => x.Name)
                .Take(limit)
                .Select((x, index) => new PlayerRankingEntry
                {
                    Rank = index + 1,
                    Name = x.Name,
                    Level = x.Level,
                    Reborn = x.Reborn,
                    PKPoints = x.PKPoints,
                    Class = ClassName(x.Class)
                }).ToList();

            var nobility = players
                .Where(x => IsPublicPlayer(x) && x.NobilityIsActive && x.NobilityDonation > 0)
                .OrderByDescending(x => x.NobilityDonation)
                .ThenBy(x => x.Name)
                .Take(limit)
                .Select((x, index) => new PlayerRankingEntry
                {
                    Rank = index + 1,
                    Name = x.Name,
                    Level = x.Level,
                    Reborn = x.Reborn,
                    NobilityDonation = x.NobilityDonation,
                    Class = ClassName(x.Class)
                }).ToList();

            var guildRanking = guilds
                .Where(x => x.GuildID > 0 && !string.IsNullOrWhiteSpace(x.GuildName))
                .OrderByDescending(x => x.Level)
                .ThenByDescending(x => x.MembersCount)
                .ThenByDescending(x => x.SilverFund)
                .ThenBy(x => x.GuildName)
                .Take(limit)
                .Select((x, index) => new GuildRankingEntry
                {
                    Rank = index + 1,
                    Name = x.GuildName,
                    Leader = x.LeaderName,
                    Level = x.Level,
                    Members = x.MembersCount
                }).ToList();

            var arenaRanking = arena
                .Where(x => x.UID > 0 && !string.IsNullOrWhiteSpace(x.Name))
                .OrderByDescending(x => x.ArenaPoints)
                .ThenByDescending(x => x.TotalWin)
                .ThenBy(x => x.Name)
                .Take(limit)
                .Select((x, index) => new ArenaRankingEntry
                {
                    Rank = index + 1,
                    Name = x.Name,
                    ArenaPoints = x.ArenaPoints,
                    Wins = x.TotalWin,
                    Losses = x.TotalLose
                }).ToList();

            return new PublicRankingsResponse
            {
                GeneratedAtUtc = DateTime.UtcNow,
                Level = level,
                PK = pk,
                Nobility = nobility,
                Guilds = guildRanking,
                Arena = arenaRanking
            };
        }

        private static bool IsPublicPlayer(ApiPlayer player)
        {
            return player.UID > 0
                && player.Level > 0
                && !string.IsNullOrWhiteSpace(player.Name)
                && !player.Name.Equals("None", StringComparison.OrdinalIgnoreCase);
        }

        private static string ClassName(byte value)
        {
            if (value >= 10 && value <= 15) return "Trojan";
            if (value >= 20 && value <= 25) return "Warrior";
            if (value >= 40 && value <= 45) return "Archer";
            if (value >= 100 && value <= 105) return "Taoist";
            if (value >= 130 && value <= 135) return "Water Taoist";
            if (value >= 140 && value <= 145) return "Fire Taoist";
            return "Class " + value;
        }
    }

    public class PublicRankingsResponse
    {
        public DateTime GeneratedAtUtc { get; set; }
        public List<PlayerRankingEntry> Level { get; set; } = new();
        public List<PlayerRankingEntry> PK { get; set; } = new();
        public List<PlayerRankingEntry> Nobility { get; set; } = new();
        public List<GuildRankingEntry> Guilds { get; set; } = new();
        public List<ArenaRankingEntry> Arena { get; set; } = new();
    }

    public class PlayerRankingEntry
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public string Class { get; set; } = "";
        public int Level { get; set; }
        public int Reborn { get; set; }
        public int PKPoints { get; set; }
        public ulong NobilityDonation { get; set; }
    }

    public class GuildRankingEntry
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public string Leader { get; set; } = "";
        public uint Level { get; set; }
        public uint Members { get; set; }
    }

    public class ArenaRankingEntry
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public uint ArenaPoints { get; set; }
        public uint Wins { get; set; }
        public uint Losses { get; set; }
    }
}
