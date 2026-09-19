using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiClassPKWar : ClassPKWar
    {
        [Key]
        public uint Id { get; set; }
    }
}
