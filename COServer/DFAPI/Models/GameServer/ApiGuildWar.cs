using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiGuildWar : GuildWar
    {
        [Key]
        public int Id { get; set; }
    }
}
