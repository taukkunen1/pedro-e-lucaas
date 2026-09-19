using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("clans")]
    public class ApiClan:Clan
    {
        [Key]
        public uint Id { get; set; }
    }
}
