using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.AccountServer
{
    [Table("configurations")]
    public class Configuration
    {
        [Key]
        public uint Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public Configuration()
        {
        }
    }
}
