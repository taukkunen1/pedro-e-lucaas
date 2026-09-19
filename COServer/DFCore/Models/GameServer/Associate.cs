using Core.Interfaces.GameServer;
using System.Collections.Generic;

namespace Core.Models.GameServer
{
    public class Associate : IAssociate
    {
        public uint PlayerUID { get; set; }
        public uint MentorExpballs { get; set; }
        public uint MentorBlessing { get; set; }
        public uint MentorStones { get; set; }
        public List<AssociateMember> Members { get; set; }
    }
    public class AssociateMember
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
}
