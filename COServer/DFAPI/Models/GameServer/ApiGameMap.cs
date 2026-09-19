using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("gamemaps")]
    public class ApiGameMap : GameMap
    {
        [Key]
        public uint Id {  get; set; }
    }
}
