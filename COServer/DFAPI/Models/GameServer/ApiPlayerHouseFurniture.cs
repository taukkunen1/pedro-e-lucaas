using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("player_house_furnitures")]
    public class ApiPlayerHouseFurniture : PlayerHouseFurniture
    {
        [Key]
        public uint Id { get; set; }
    }
}
