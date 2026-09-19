using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("clan_wars")]
    public class ApiClanWar : Core.Models.GameServer.ClanWar
    {
        [Key]
        public uint Id { get; set; }
    }
}
