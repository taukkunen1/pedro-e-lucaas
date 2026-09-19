using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiBanIP : BanIP
    {
        [Key]
        public uint Id { get; set; }
    }
}
