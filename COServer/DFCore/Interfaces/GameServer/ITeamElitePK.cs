namespace Core.Interfaces.GameServer
{
    public interface ITeamElitePK
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
    public enum EliteTournamentType
    {
        TeamElitePK = 0,
        SkillTeamElitePK = 1,
    }
}
