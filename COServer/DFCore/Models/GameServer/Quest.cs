using Core.Interfaces.GameServer;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.GameServer
{
    public class Quest: IQuest
    {
        public uint UID { get; set; }
        public QuestStatus Status { get; set; }
        public uint Time { get; set; }
        [Column(TypeName = "json")]
        public string IntentionsJson { get; set; }
        public uint[] Intentions
        {
            get => string.IsNullOrEmpty(IntentionsJson) ? new uint[0] : System.Text.Json.JsonSerializer.Deserialize<uint[]>(IntentionsJson);
            set => IntentionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
        public uint PlayerUID { get; set; }
    }
}
