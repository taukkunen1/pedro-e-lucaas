using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{

    [Table("player_items")]
    public class ApiPlayerItem : PlayerItem
    {
        [Key]
        public uint Id { get; set; }

        public ApiPlayerItem()
        {

        }
    }
}
