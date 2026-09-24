using System;
using System.Collections.Generic;

namespace ConquerSite.Models.DTOs
{
    public class RankingsDTO
    {
        public bool Available { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public List<PlayerRankingDTO> Level { get; set; } = new();
        public List<PlayerRankingDTO> PK { get; set; } = new();
        public List<PlayerRankingDTO> Nobility { get; set; } = new();
        public List<GuildRankingDTO> Guilds { get; set; } = new();
        public List<ArenaRankingDTO> Arena { get; set; } = new();
    }

    public class PlayerRankingDTO
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public string Class { get; set; } = "";
        public int Level { get; set; }
        public int Reborn { get; set; }
        public int PKPoints { get; set; }
        public ulong NobilityDonation { get; set; }
    }

    public class GuildRankingDTO
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public string Leader { get; set; } = "";
        public uint Level { get; set; }
        public uint Members { get; set; }
    }

    public class ArenaRankingDTO
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public uint ArenaPoints { get; set; }
        public uint Wins { get; set; }
        public uint Losses { get; set; }
    }
}
