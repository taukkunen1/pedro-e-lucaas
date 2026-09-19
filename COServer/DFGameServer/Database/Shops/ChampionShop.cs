using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameServer.Database.Shops
{
    public static class ChampionShop
    {
        public const uint UID = 6003;

        public static ChampionShopBase Shop;
        public class ChampionItem
        {
            public uint ID, Price;
            public bool Bound;
        }
        public class ChampionShopBase
        {
            public int Count { get { return Items.Count; } }
            public Dictionary<uint, ChampionItem> Items;
        }
        public static void Load()
        {
            string[] text = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "shops", "GoldenLeagueShop.ini"));
            Shop = new ChampionShopBase();
            Shop.Items = new Dictionary<uint, ChampionItem>();
            for (int x = 0; x < text.Length; x++)
            {
                String line = text[x];
                String[] split = line.Split(' ');
                Shop.Items.Add(uint.Parse(split[0]),
                    new ChampionItem()
                    {
                        ID = uint.Parse(split[0]),
                        Price = uint.Parse(split[1]),
                        Bound = split[2].Contains("Bound")
                    });
            }
        }
    }
}
