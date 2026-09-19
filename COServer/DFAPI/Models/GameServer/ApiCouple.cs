using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiCouple : Couple
    {
        [Key]
        public uint Id { get; set; }
    }
}
