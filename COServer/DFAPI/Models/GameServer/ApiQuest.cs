using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("quests")]
    public class ApiQuest: Quest
    {
        [Key]
        public uint Id { get; set; }

        [Column(TypeName = "json")]
        public string IntentionsJson { get; set; }

        [NotMapped]
        public uint[] Intentions
        {
            get => string.IsNullOrEmpty(IntentionsJson) ? new uint[0] : System.Text.Json.JsonSerializer.Deserialize<uint[]>(IntentionsJson);
            set => IntentionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}
