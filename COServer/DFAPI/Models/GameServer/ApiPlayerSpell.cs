using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiPlayerSpell:PlayerSpell
    {
        [Key]
        public uint Id { get; set; }
    }
}
