using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("static_statues")]
    public class ApiStaticStatue : StaticStatue
    {
        [Key]
        public uint Id {  get; set; }
    }
}
