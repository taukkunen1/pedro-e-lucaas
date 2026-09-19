using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("teamarenas")]
    public class ApiTeamArenaUser : ArenaUser
    {
        [Key]
        public uint Id { get; set; }
    }
}
