using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Interfaces.GameServer
{
    public interface IQuest
    {
        public uint UID { get; set; }
        public QuestStatus Status { get; set; }
        public uint Time { get; set; }
        public string IntentionsJson { get; set; }
        public uint[] Intentions
        {
            get => string.IsNullOrEmpty(IntentionsJson) ? new uint[0] : System.Text.Json.JsonSerializer.Deserialize<uint[]>(IntentionsJson);
            set => IntentionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        public uint PlayerUID { get; set; }
    }
    public enum QuestStatus : uint
    {
        Accepted = 0,
        Finished = 1,
        Available = 2
    }
}