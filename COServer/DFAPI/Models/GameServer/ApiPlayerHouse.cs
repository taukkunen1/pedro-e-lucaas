using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("player_houses")]
    public class ApiPlayerHouse : PlayerHouse
    {
        [Key]
        public uint Id { get; set; }
        public List<ApiPlayerHouseFurniture> Furnitures { get; set; }
    }
}
