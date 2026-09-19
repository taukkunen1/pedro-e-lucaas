using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    [Table("votes")]
    public class Vote
    {
        [Key]
        public uint ID { get; set; }
        public uint EntityID { get; set; }
        public uint Votes { get; set; }
        public DateTime LastVoteDate { get; set; }
    }
    public class AddVote
    {
        public uint UID { get; set; }
    }
}
