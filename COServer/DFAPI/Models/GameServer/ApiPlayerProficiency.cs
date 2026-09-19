using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiPlayerProficiency : PlayerProficiency
    {
        [Key]
        public uint Id { get; set; }
    }
}
