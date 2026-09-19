using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiTeamElitePK : TeamElitePK
    {
        [Key]
        public uint Id { get; set; }
    }
}
