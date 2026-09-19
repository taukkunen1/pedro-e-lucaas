using Core;
using Poker;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Database
{
    public class LotteryTable
    {

        public class LotteryItem
        {
            public int Rank, Chance;
            public string Name;
            public uint ID;
            public byte Color;
            public byte Sockets;
            public byte Plus;

            public override string ToString()
            {
                return Rank + " " + Chance + " " + Name + " " + ID + " " + Color + " " + Sockets + " " + Plus;
            }
        }
        private Dictionary<int, List<LotteryItem>> LotteryItems;
        public void LoadLotteryItems()
        {
            LotteryItems = new Dictionary<int, List<LotteryItem>>();

            using (DBActions.Read reader = new DBActions.Read("lottery.ini"))
            {
                if (reader.Reader())
                {
                    for (int x = 0; x < reader.Count; x++)
                    {
                        DBActions.ReadLine line = new DBActions.ReadLine(reader.ReadString(""), ' ');
                        LotteryItem item = new LotteryItem();
                        item.Rank = line.Read((int)0);
                        item.Chance = line.Read((int)0);
                        item.Name = line.Read("");
                        item.ID = line.Read((uint)0);
                        item.Color = line.Read((byte)0);
                        item.Sockets = line.Read((byte)0);
                        item.Plus = line.Read((byte)0);

                        if (item.Color < (byte)Role.Flags.Color.Black)
                        {
                            item.Color = 2;
                            Console.WriteLine("Detected invalid value for Color in lottery.ini, fixing...");
                        }

                        if (!LotteryItems.ContainsKey(item.Rank))
                            LotteryItems.Add(item.Rank, new List<LotteryItem>());
                        LotteryItems[item.Rank].Add(item);
                    }
                }
            }
        }
        public byte LotteryEntry(byte vipLevel)
        {
            byte chance = 10;
            switch (vipLevel)
            {
                case 6:
                    chance = 50;
                    break;
            }
            return chance;
        }
        public byte QuestEntry(byte vipLevel)
        {
            byte chance = 0;
            switch (vipLevel)
            {
                default:
                    chance = 3;
                    break;
                case 6:
                    chance = 6;
                    break;
            }
            return chance;
        }
        public byte Quest2Entry(byte vipLevel)
        {
            byte chance = 0;
            switch (vipLevel)
            {
                default:
                    chance = 3;
                    break;
                case 6:
                    chance = 6;
                    break;
            }
            return chance;
        }
        public byte MonsterEntry(byte vipLevel)
        {
            byte chance = 0;
            switch (vipLevel)
            {
                default:
                    chance = 5;
                    break;
                case 6:
                    chance = 15;
                    break;
            }
            return chance;
        }
        public LotteryItem GenerateLotteryItem()
        {
            List<LotteryItem> items = LotteryItems[GenerateRank()];
            if (items.Count == 1)
                return items[0];
            // Get a random item from the list of LotteryItem from the Rank specified but with a Utils.Rate with the item.Chan
            foreach(LotteryItem item in items)
            {
                if (Utils.Rate(item.Chance))
                {
                    return item;
                }
            }
            return items[(byte)BaseFunc.RandGet(items.Count, true)];
        }
        public Game.MsgServer.MsgGameItem CreateGameItem(LotteryItem Item)
        {
            Game.MsgServer.MsgGameItem GameItem = new Game.MsgServer.MsgGameItem();
            GameItem.ITEM_ID = Item.ID;
            GameItem.Color = (Role.Flags.Color)(byte)Pool.GetRandom.Next(4, 8);//Item.Color;
            GameItem.Plus = Item.Plus;
            if (Item.Sockets > 0)
                GameItem.SocketOne = Role.Flags.Gem.EmptySocket;
            if (Item.Sockets > 1)
                GameItem.SocketTwo = Role.Flags.Gem.EmptySocket;
            //GameItem.UID = Pool.ITEM_Counter.Next;
            var DBItem = Pool.ItemsBase[Item.ID];
            GameItem.Durability = GameItem.MaximDurability = DBItem.Durability;
            return GameItem;
        }
        public int GenerateRank()
        {
            int Rank = 6;
            if (Utils.Rate(15) && LotteryItems.Any(x => x.Value.Any(x => x.Rank == 9)))
            {
                Rank = 5;
            } else
            {
                if (Utils.Rate(10) && LotteryItems.Any(x => x.Value.Any(x => x.Rank == 8)))
                {
                    Rank = 4;
                } else
                {
                    if (Utils.Rate(5) && LotteryItems.Any(x => x.Value.Any(x => x.Rank == 3)))
                    {
                        Rank = 3;
                    } else
                    {
                        if (Utils.Rate(2) && LotteryItems.Any(x => x.Value.Any(x => x.Rank == 2)))
                        {
                            Rank = 2;
                        } else
                        {
                            if (Utils.Rate(1) && LotteryItems.Any(x => x.Value.Any(x => x.Rank == 1)))
                            {
                                Rank = 1;
                            }
                        }
                    }
                }
            }
            return Rank;
        }
    }
}
