using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameServer.Database.Shops
{
    public static class HonorShop
    {
        public const uint UID = 6000;

        public static HonorShopBase Shop;
        public class HonorShopBase
        {
            public int Count { get { return Items.Count; } }
            public Dictionary<uint, uint> Items;
        }
        public static void Load()
        {
            string[] text = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "shops", "HonorShop.ini"));
            Shop = new HonorShopBase();
            for (int x = 0; x < text.Length; x++)
            {
                string line = text[x];
                if (line.StartsWith("Item"))
                {
                    line = line.Remove(0, 11);
                    Shop.Items = new Dictionary<uint, uint>(int.Parse(line));
                }
                else if (line.StartsWith("Recommend"))
                {
                    break;
                }
                else
                {
                    if (line.Length > 3)
                    {
                        string[] Parts = line.Split(',');
                        uint itemID = uint.Parse(Parts[0]);
                        uint cost = uint.Parse(Parts[1]);
                        if (!Shop.Items.ContainsKey(itemID))
                            Shop.Items.Add(itemID, cost);
                    }
                }
            }
        }
    }
}
