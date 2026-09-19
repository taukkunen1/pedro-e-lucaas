using System;
using System.Linq;

namespace GameServer.Bot
{
    public enum EquipmentType
    {
        Necklace,
        Ring,
        Armet,
        Armor,
        OneHander,
        TwoHander
    }
    public class Equipment
    {
        public static ushort[] NecklaceType = new ushort[] { 120, 121 };
        public static ushort[] RingType = new ushort[] { 150, 151 };
        public static ushort[] ArmetType = new ushort[] { 111, 112, 113, 114, 117, 118 };
        public static ushort[] ArmorType = new ushort[] { 130, 131, 132, 133, 134 };
        public static ushort[] OneHanderType = new ushort[] { 410, 420, 421, 430, 440, 450, 460, 480, 481, 490, 500, 601 };
        public static ushort[] TwoHanderType = new ushort[] { 510, 530, 560, 561, 580, 900, };
        public static ushort[] DevBow = new ushort[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320, 330, 340, 350, 360, 370, 380, 390, 400, 410, 420 };
        public static uint[] Garments = new uint[]
        {
            181375, 182305, 181365, 181345, 182315,
            181335, 191305, 181305, 181405, 181505,
            181605, 181705, 181805, 181905, 181315,
            181415, 181515, 181615, 181715, 181815,
            181915, 181325, 181425, 181525, 181625,
            181725, 181825, 181925, 181355, 191405
        };
        public AI Bot;
        public ServerSockets.Packet stream;
        public Equipment(AI _bot)
        {
            Bot = _bot;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                stream = rec.GetStream();
            }
        }
        public Equipment GetRandomEquipment(byte c)
        {
            var all_items = Pool.ItemsBase.Values.Where(e => Database.ItemType.EquipPassJobReq(e, Bot.BEntity) == true && (e.ID % 10) == 9).ToArray();
            var armor = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.Armor && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            Armor(armor[Role.Core.Random.Next(0, armor.Length)].ID);
            var head = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.Head && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            Head(head[Role.Core.Random.Next(0, head.Length)].ID);
            var ring = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.Ring && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            Ring(ring[Role.Core.Random.Next(0, ring.Length)].ID);
            var boots = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.Boots && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            Boots(boots[Role.Core.Random.Next(0, boots.Length)].ID);
            var necklace = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.Necklace && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            Necklace(necklace[Role.Core.Random.Next(0, necklace.Length)].ID);
            var r_wep = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.RightWeapon && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            var l_wep = all_items.Where(e => Database.ItemType.ItemPosition(e.ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon && (e.Level >= 10 && e.Level < Bot.BEntity.Player.Level)).ToArray();
            if (c >= 40 && c <= 45)
            {
                var wep = r_wep.Where(e => Database.ItemType.IsBow(e.ID)).ToArray();
                RightWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                wep = null;
            }
            if (c >= 50 && c <= 55)
            {
                var wep = r_wep.Where(e => Database.ItemType.IsKatana(e.ID)).ToArray();
                RightWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                LeftWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                wep = null;
            }
            if (c >= 10 && c <= 15)
            {
                var wep = r_wep.Where(e => (e.ID / 1000) == 480 || (e.ID / 1000) == 410 || (e.ID / 1000) == 420).ToArray();
                RightWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                LeftWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                wep = null;
            }
            if (c >= 100)
            {
                var wep = r_wep.Where(e => Database.ItemType.IsBacksword(e.ID)).ToArray();
                RightWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                wep = null;
            }
            if (c >= 20 && c <= 25)
            {
                var wep = r_wep.Where(e => (e.ID / 1000) == 561 || (e.ID / 1000) == 560).ToArray();
                RightWeapon(wep[Role.Core.Random.Next(0, wep.Length)].ID);
                wep = null;
            }
            if (Bot.BEntity.Player.Level >= 100)
            {
                Tower(202009);
                Fan(201009);
            }
            necklace = boots = ring = head = l_wep = r_wep = armor = all_items = null;
            GC.Collect();
            return this;
        }
        public Equipment LeftWeapon(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.LeftWeapon, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment RightWeapon(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.RightWeapon, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment RightWeaponAccessory(uint ID)
        {

            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.RightWeaponAccessory);
            return this;
        }
        public Equipment LeftWeaponAccessory(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.LeftWeaponAccessory);
            return this;
        }
        public Equipment Garment(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Garment);
            return this;
        }
        public Equipment Armor(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Armor, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Boots(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Boots, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Ring(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Ring, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Head(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Head, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Necklace(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Necklace, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Tower(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Tower, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Fan(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.Fan, (byte)Pool.GetRandom.Next(3, 5));
            return this;
        }
        public Equipment Crop(uint ID)
        {
            Bot.BEntity.Equipment.Add(stream, ID, Role.Flags.ConquerItem.RidingCrop, (byte)Pool.GetRandom.Next(1, 6));
            return this;
        }
        public Equipment Send()
        {
            Bot.BEntity.Equipment.QueryEquipment(false);
            Bot.BEntity.Player.View.SendView(Bot.BEntity.Player.GetArray(stream, false), false);
            return this;
        }
        private uint GenerateBowItem()
        {
            uint BaseID = 500000;
            uint Super = 9;
            uint SecPartID = (uint)DevBow[Pool.GetRandom.Next(0, DevBow.Length)] + Super;
            return BaseID + SecPartID;
        }
        private uint GenerateItem(EquipmentType equipmentType)
        {
            uint ItemId = 0;
            byte dwItemQuality = 9;
            uint dwItemSort = 0;
            uint dwItemLev = 0;
            switch (equipmentType)
            {
                case EquipmentType.Necklace:
                    {
                        dwItemSort = NecklaceType[Pool.GetRandom.Next(0, NecklaceType.Length)];
                        dwItemLev = 7;
                        break;
                    }
                case EquipmentType.Ring:
                    {
                        dwItemSort = RingType[Pool.GetRandom.Next(0, RingType.Length)];
                        dwItemLev = 10;
                        break;
                    }
                case EquipmentType.Armor:
                    {
                        dwItemSort = ArmorType[Pool.GetRandom.Next(0, ArmorType.Length)];
                        dwItemLev = 15;
                        break;
                    }
                case EquipmentType.OneHander:
                    {
                        dwItemSort = OneHanderType[Pool.GetRandom.Next(0, OneHanderType.Length)];
                        dwItemLev = 15;
                        break;
                    }
            }
            //dwItemLev = AlterItemLevel(dwItemLev, dwItemSort);
            ItemId = (dwItemSort * 1000) + (dwItemLev * 4) + dwItemQuality;
            if (equipmentType == EquipmentType.Armor)
            {
                if (Pool.ItemsBase.ContainsKey(ItemId))
                {
                    return ItemId;
                }
                else Garment(Garments[Pool.GetRandom.Next(0, Garments.Length)]);
            }
            else
            {
                if (Pool.ItemsBase.ContainsKey(ItemId))
                    return ItemId;
            }
            return 0;
        }
        private uint AlterItemLevel(uint dwItemLev, uint dwItemSort)
        {
            int nRand = BaseFunc.RandGet(100, true);

            if (nRand < 50) // 50% down one level
            {
                uint dwLev = dwItemLev;
                dwItemLev = (uint)(BaseFunc.RandGet((int)(dwLev / 2 + dwLev / 3), false));

                if (dwItemLev > 1)
                    dwItemLev--;
            }
            else if (nRand > 80) // 20% up one level
            {
                if ((dwItemSort >= 110 && dwItemSort <= 114) ||
                    (dwItemSort >= 130 && dwItemSort <= 134) ||
                    (dwItemSort >= 900 && dwItemSort <= 999))
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 9);
                }
                else
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 23);
                }
            }

            return dwItemLev;
        }
    }
}
