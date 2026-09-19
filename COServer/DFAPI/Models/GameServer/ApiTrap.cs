using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("traps")]
    public class ApiTrap : Trap
    {
        [Key]
        public uint Id {  get; set; }
    }
}
