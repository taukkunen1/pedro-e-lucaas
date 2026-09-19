using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("portals")]
    public class ApiPortal : Portal
    {
        [Key]
        public uint Id {  get; set; }
    }
}
