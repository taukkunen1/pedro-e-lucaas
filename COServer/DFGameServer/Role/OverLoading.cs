using GameServer.Game.MsgServer;
namespace GameServer
{
    public class OverLoading
    {
        public static void InGame(Client.GameClient client)
        {
            //client.GeneratorItemDrop(DropStatus.All);

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)770004, client.Player.Class, 2);

                if (!client.Player.QuestGUI.CheckQuest(ActiveQuest.MissionId, MsgQuestList.QuestListItem.QuestStatus.Finished))
                {
                    client.Player.QuestGUI.Accept(ActiveQuest, 0);
                    client.Player.QuestGUI.SetQuestObjectives(stream, 2, client.TotalMobsKilled);
                }
                else
                {
                    if (client.Player.MonsterEntries < Pool.NewLottery.MonsterEntry(client.Player.VipLevel))
                    {
                        client.Player.QuestGUI.RemoveQuest(ActiveQuest.MissionId);
                        client.Player.QuestGUI.Accept(ActiveQuest, 0);
                    }
                }
                ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)103009, client.Player.Class, 3);
                if (!client.Player.QuestGUI.CheckQuest(ActiveQuest.MissionId, MsgQuestList.QuestListItem.QuestStatus.Finished))
                {
                    client.Player.QuestGUI.Accept(ActiveQuest, 0);
                    client.Player.QuestGUI.SetQuestObjectives(stream, 3, client.TotalMobsKilled2);
                }
            }
            client.FullLoading = true;
        }
    }
}
