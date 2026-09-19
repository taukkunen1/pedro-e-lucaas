using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiBanUID : BanUID
    {
        [Key]
        public uint Id { get; set; }
    }
}
