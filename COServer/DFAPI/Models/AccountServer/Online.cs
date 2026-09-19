using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.AccountServer
{
    [Table("online")]
    public class Online
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        [Column("OnlineCount")]
        public uint OnlineCount { get; set; }

        public Online ()
        {
        }
    }
}
