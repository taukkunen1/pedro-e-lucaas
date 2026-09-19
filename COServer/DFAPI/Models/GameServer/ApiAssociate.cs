using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("associates")]
    public class ApiAssociate : Associate
    {
        [Key]
        public uint Id { get; set; }
        public List<ApiAssociateMember> Members { get; set; }
    }
    [Table("associate_members")]
    public class ApiAssociateMember : AssociateMember
    {
        [Key]
        public uint Id { get; set; }
        public uint UID { get; set; }
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
