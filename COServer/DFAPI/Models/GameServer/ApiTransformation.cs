using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("transformations")]
    public class ApiTransformation: Transformation
    {
        [Key]
        public uint Id {  get; set; }
    }
}
