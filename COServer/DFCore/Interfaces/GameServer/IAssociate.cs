namespace Core.Interfaces.GameServer
{
    public interface IAssociate
    {
        public uint PlayerUID { get; set; }
        public uint MentorExpballs { get; set; }
        public uint MentorBlessing { get; set; }
        public uint MentorStones { get; set; }
    }

    public interface IAssociateMember
    {
        public uint UID { get; set; }
        public AssociateMemberType Type { get; set; }
        public ulong Timer { get; set; }
        public uint ExpBalls { get; set; }
        public uint Stone { get; set; }
        public uint Blessing { get; set; }
        public string MapName { get; set; }
        public string Name { get; set; }
        public ushort KillsCount { get; set; }
        public ushort BattlePower { get; set; }
    }

    public enum AssociateMemberType
    {
        Friends = 1,
        Enemy = 2,
        Partner = 3,
        Mentor = 4,
        Apprentice = 5,
        PKExplorer = 6
    }
}
