using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConquerSite.Models
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
}
