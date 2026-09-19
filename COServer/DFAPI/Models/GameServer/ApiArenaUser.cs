using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("arenas")]
    public class ApiArenaUser : ArenaUser
    {
        [Key]
        public uint Id { get; set; }
    }
}
