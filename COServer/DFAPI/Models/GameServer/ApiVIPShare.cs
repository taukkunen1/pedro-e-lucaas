using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiVIPShare : VIPShare
    {
        [Key]
        public uint Id { get; set; }
    }
}
