using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class TeamElitePK : ITeamElitePK
    {
        public EliteTournamentType Type { get; set; }
        public byte Tournament { get; set; }
        public byte Rank { get; set; }
        public uint PlayerId { get; set; }
        public string PlayerName { get; set; }
        public uint PlayerMesh { get; set; }
        public byte ClaimReward { get; set; }
        public uint LeaderUID { get; set; }
    }
}
