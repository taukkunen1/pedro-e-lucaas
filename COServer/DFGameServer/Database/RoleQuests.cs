using Core;
using Core.Interfaces.GameServer;
using Core.Models.GameServer;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class RoleQuests
    {
        public static void Remove(Client.GameClient user, Quest questToRemove)
        {
            if (!ServerConfig.DbFromFiles)
            {
                List<Quest> questsToRemove = new List<Quest>() { questToRemove };
                RestApiHelper.PostRequestSuccessful<List<Quest>>("Quests/Remove", questsToRemove);
            }
        }
        public static void Save(Client.GameClient user)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "Quests", user.Player.UID + ".bin"), FileMode.Create))
                {
                    int AmountQuests = user.Player.QuestGUI.src.Count;
                    binary.WriteInt(AmountQuests);
                    foreach (var quest in user.Player.QuestGUI.src.Values)
                    {
                        uint UID = quest.UID;
                        uint Status = (uint)quest.Status;
                        uint Time = quest.Time;


                        int size = quest.Intentions.Length;
                        binary.WriteUInt(UID);
                        binary.WriteUInt(Status);
                        binary.WriteUInt(Time);
                        binary.WriteInt(size);
                        for (int x = 0; x < size; x++)
                        {
                            uint Intention = quest.Intentions[x];
                            binary.WriteUInt(Intention);
                        }

                    }
                    binary.Close();
                }
            } else
            {
                List<Quest> questsToUpdate = new List<Quest>();
                foreach (var quest in user.Player.QuestGUI.src.Values)
                {
                    questsToUpdate.Add(new Quest
                    {
                        UID = quest.UID,
                        PlayerUID = user.Player.UID,
                        IntentionsJson = System.Text.Json.JsonSerializer.Serialize(quest.Intentions),
                        Status = (QuestStatus)quest.Status,
                        Time = quest.Time
                    });
                }
                RestApiHelper.PostRequestSuccessful<List<Quest>>("Quests/Update", questsToUpdate);
            }
        }
        public static void Load(Client.GameClient user)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "Quests", user.Player.UID + ".bin"), FileMode.Open))
                {
                    int AmountQuests = user.Player.QuestGUI.src.Count;
                    AmountQuests = binary.ReadInt();
                    for (int x = 0; x < AmountQuests; x++)
                    {

                        uint _UID;
                        uint _Status;
                        uint _Time;
                        int Intentions;
                        uint[] QIntentions;
                        _UID = binary.ReadUint();
                        _Status = binary.ReadUint();
                        _Time = binary.ReadUint();
                        Intentions = binary.ReadInt();
                        QIntentions = new uint[Intentions];
                        for (int i = 0; i < Intentions; i++)
                        {
                            uint QIntention;
                            Intentions = binary.ReadInt();
                            QIntention = binary.ReadUint();
                            QIntentions[i] = QIntention;
                        }

                        var quest = new Game.MsgServer.MsgQuestList.QuestListItem()
                        {
                            UID = _UID,
                            Status = (Game.MsgServer.MsgQuestList.QuestListItem.QuestStatus)_Status,
                            Time = _Time,
                            Intentions = QIntentions
                        };

                        if (!user.Player.QuestGUI.src.ContainsKey(quest.UID))
                            user.Player.QuestGUI.src.Add(quest.UID, quest);
                        if (quest.Status == Game.MsgServer.MsgQuestList.QuestListItem.QuestStatus.Accepted && !user.Player.QuestGUI.AcceptedQuests.ContainsKey(quest.UID))
                            user.Player.QuestGUI.AcceptedQuests.Add(quest.UID, quest);
                    }
                }
            } else
            {
                List<Quest> quests = RestApiHelper.GetRequest<List<Quest>>("Quests/Get");
                foreach (var quest in quests)
                {
                    var questItem = new Game.MsgServer.MsgQuestList.QuestListItem()
                    {
                        UID = quest.UID,
                        Status = (Game.MsgServer.MsgQuestList.QuestListItem.QuestStatus)quest.Status,
                        Time = quest.Time,
                        Intentions = quest.Intentions
                    };

                    if (!user.Player.QuestGUI.src.ContainsKey(questItem.UID))
                        user.Player.QuestGUI.src.Add(questItem.UID, questItem);
                    if (questItem.Status == Game.MsgServer.MsgQuestList.QuestListItem.QuestStatus.Accepted
                        && !user.Player.QuestGUI.AcceptedQuests.ContainsKey(questItem.UID))
                    {
                        user.Player.QuestGUI.AcceptedQuests.Add(questItem.UID, questItem);
                    }
                }
            }
        }
    }
}
