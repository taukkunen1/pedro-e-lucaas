using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiElitePK : ElitePK
    {
        [Key]
        public uint Id { get; set; }
    }
}
