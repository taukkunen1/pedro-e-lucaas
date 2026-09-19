using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("players")]
    public class ApiPlayer : Player
    {
        [Key]
        public uint Id { get; set; }
    }
}
