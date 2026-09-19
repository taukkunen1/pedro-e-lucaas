using Core;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database.Shops
{
    public static class ShopFile
    {
        public static Dictionary<uint, Shop> Shops;
        public class Shop
        {
         
            public uint UID;
            public MoneyType MoneyType;
            public int Count { get { return Items.Count; } }
            public List<uint> Items;

            public List<uint> BoundItems = new List<uint>();
          
        }
        public static void Load()
        {
            Shops = new Dictionary<uint, Shop>();
            string ShopFilePath = Path.Combine(ServerConfig.DbLocation, "shops", "Shop.dat");
            IniFileHelper iniHelper = new IniFileHelper(ShopFilePath);
            int Count = iniHelper.ReadInt32("Header", "Amount", 0);
            for (int x = 0; x < Count; x++)
            {
                Shop shop = new Shop();
                shop.UID = iniHelper.ReadUInt32("Shop" + x.ToString(), "ID", 0);
                shop.MoneyType = (MoneyType)iniHelper.ReadUInt32("Shop" + x.ToString(), "MoneyType", 0);
                int Items = iniHelper.ReadInt32("Shop" + x.ToString(), "ItemAmount", 0);
                shop.Items = new List<uint>();
                for (int i = 0; i < Items; i++)
                {
                    shop.Items.Add(iniHelper.ReadUInt32("Shop" + x.ToString(), "Item" + i.ToString(), 0));
                }
                if (!Shops.ContainsKey(shop.UID))
                    Shops.Add(shop.UID, shop);
            }
        }
        public enum MoneyType
        {
            Gold = 0,
            ConquerPoints = 1,
            HonorPoints = 2, 
            BoundConquerPoints = 3
        }
    }
}
