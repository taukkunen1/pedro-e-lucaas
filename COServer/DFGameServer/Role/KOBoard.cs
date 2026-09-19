using Core;
using GameServer.MadeByDaRkFox;
using System.Linq;

namespace GameServer.Role
{
    public class KOBoard
    {
        public class Entry
        {
            public uint UID = 0;
            public string Name = "";
            public uint Points = 0;
            public uint Rank = 0;
        }
        public static class KOBoardRanking
        {
            public const int MaxItems = 100;
            public static SafeDictionaryAlt<uint, Entry> Items = new SafeDictionaryAlt<uint, Entry>();
            public static object SynRoot = new object();
            private static Entry[] Top50 = new Entry[0];
            private static IDataManager DataManager = new KOBoardRanksManager();

            public static bool GetMyRank(uint UID, out Entry item)
            {
                
                if (Items.TryGetValue(UID, out item))
                {
                    return true;
                }

                return false;
            }

            public static void OnLoggin(Client.GameClient user)
            {
                Entry Item;
                if (GetMyRank(user.Player.UID, out Item))
                {
                    user.SendSysMesage("You have set a new personal! KO record with " + Item.Points + " KOs! You currently rank# " + Item.Rank+ "", Game.MsgServer.MsgMessage.ChatMode.System, Game.MsgServer.MsgMessage.MsgColor.white);
                }
            }
            public static void SendMessage(string Message)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(Message, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }               
            }
            public static void AddItem(Entry item, bool SendMessages = false)
            {
                if (ServerConfig.IsInterServer)
                    return;
                if (Items.Count < MaxItems)
                {
                    if (!Items.ContainsKey(item.UID))
                    {
                        Items.Add(item.UID, item);
                        lock (SynRoot)
                        {
                            Top50 = Items.Values.OrderByDescending(p => p.Points).ToArray();
                            for (int x = 0; x < Top50.Length; x++)
                                Top50[x].Rank = (ushort)(x + 1);
                        }
                        if (SendMessages)
                            SendMessage("" + item.Name + " has killed " + item.Points + " monsters with XP skills and ranks " + item.Rank + " on the KO board.");
                    }
                    else if (Items.ContainsKey(item.UID))
                    {
                        uint points = Items[item.UID].Points;
                        uint rank = Items[item.UID].Rank;
                        Items[item.UID] = item;
                        item.Rank = rank;
                        if (points < item.Points)
                        {
                            lock (SynRoot)
                            {
                                Top50 = Items.Values.OrderByDescending(p => p.Points).ToArray();
                                for (int x = 0; x < Top50.Length; x++)
                                    Top50[x].Rank = (ushort)(x + 1);
                            }
                            if (SendMessages)
                                SendMessage("" + item.Name + " has killed " + item.Points + " monsters with XP skills and ranks " + item.Rank + " on the KO board.");
                        }
                    }
                }
                else
                {
                    if (Items.ContainsKey(item.UID))
                    {
                        uint points = Items[item.UID].Points;
                        uint rank = Items[item.UID].Rank;
                        Items[item.UID] = item;
                        item.Rank = rank;

                        if (points < item.Points)
                        {
                            lock (SynRoot)
                            {
                                Top50 = Items.Values.OrderByDescending(p => p.Points).ToArray();
                                for (int x = 0; x < Top50.Length; x++)
                                    Top50[x].Rank = (ushort)(x + 1);
                            }
                            if (SendMessages)
                                SendMessage("" + item.Name + " has killed " + item.Points + " monsters with XP skills and ranks " + item.Rank + " on the KO board.");
                        }
                    }
                    else
                    {
                        var last = Top50[Top50.Length - 1];
                        if (item.Points > last.Points)
                        {
                            Items.Remove(last.UID);
                            Items.Add(item.UID, item);

                            lock (SynRoot)
                            {
                                Top50 = Items.Values.OrderByDescending(p => p.Points).ToArray();
                                for (int x = 0; x < Top50.Length; x++)
                                    Top50[x].Rank = (ushort)(x + 1);
                            }
                            if (SendMessages)
                                SendMessage("" + item.Name + " has killed " + item.Points + " monsters with XP skills and ranks " + item.Rank + " on the KO board.");
                        }
                    }
                }
            }
            public static void Load()
            {
                DataManager.Load();
            }
            public static void Save()
            {
                DataManager.Save();
            }
        }
    }
}
