using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("crimes")]
    public class ApiCrime : Crime
    {
        [Key]
        public uint Id { get; set; }
    }
}
