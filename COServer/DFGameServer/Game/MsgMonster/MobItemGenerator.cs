using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgMonster
{
    public class MobRateWatcher
    {
        private int tick;
        private int count;
        public static implicit operator bool(MobRateWatcher q)
        {
            bool result = false;
            q.count++;
            if (q.count == q.tick)
            {
                q.count = 0;
                result = true;
            }
            return result;
        }
        public MobRateWatcher(int Tick)
        {
            tick = Tick;
            count = 0;
        }
    }

    public struct SpecialItemWatcher
    {
        public uint ID;
        public MobRateWatcher Rate;
        public SpecialItemWatcher(uint ID, int Tick)
        {
            this.ID = ID;
            Rate = new MobRateWatcher(Tick);
        }
    }

    public class MobItemGenerator
    {
        public static ushort[] NecklaceType = new ushort[] { 120, 121 };
        public static ushort[] RingType = new ushort[] { 150, 151 };
        public static ushort[] ArmetType = new ushort[] { 111, 112, 113, 114, 117, 118 };
        public static ushort[] ArmorType = new ushort[] { 130, 131, 132, 133, 134 };
        public static ushort[] OneHanderType = new ushort[] { 410, 420, 421, 430, 440, 450, 460, 480, 481, 490, 500 };
        public static ushort[] TwoHanderType = new ushort[] { 510, 530, 560, 561, 580, 900, };
        public static uint[] SeaPotions = new uint[] { 3004230, 3004231, 3004232, 3004233, 3004234, 3004235, 3004236, 3004237, 3004238 };
        private MonsterFamily Family;

        private MobRateWatcher Refined;
        private MobRateWatcher Unique;
        private MobRateWatcher Elite;
        private MobRateWatcher Super;
        private MobRateWatcher PlusOne;
        private MobRateWatcher PlusTwo;

        private MobRateWatcher DropHp;
        private MobRateWatcher DropMp;
        private MobRateWatcher Chi100;
        private MobRateWatcher Study20;
        private MobRateWatcher Chi300;
        private MobRateWatcher Bomb;
        private MobRateWatcher LuckyAmulet;
        private MobRateWatcher MoonBox;

        //720665 CuteCPPack
        private MobRateWatcher DropSpecialPotions;
        private MobRateWatcher CuteCPPack;
        private MobRateWatcher DragonBalls;

        public MobItemGenerator(MonsterFamily family)
        {
            Family = family;
            // Era 1 hunting: rare quality/+ drops must remain scarce.
            // These deterministic watchers are intentionally conservative and replace
            // the 5695 test-like rates (1/10..1/30) that flooded the economy.
            Refined = new MobRateWatcher(Game.Era1.Era1Economy.RefinedDropEvery);
            Unique = new MobRateWatcher(Game.Era1.Era1Economy.UniqueDropEvery);
            Elite = new MobRateWatcher(Game.Era1.Era1Economy.EliteDropEvery);
            Super = new MobRateWatcher(Game.Era1.Era1Economy.SuperDropEvery);
            PlusOne = new MobRateWatcher(Game.Era1.Era1Economy.PlusOneDropEvery);
            PlusTwo = new MobRateWatcher(Game.Era1.Era1Economy.PlusTwoDropEvery);

            DropHp = new MobRateWatcher(50);
            DropMp = new MobRateWatcher(50);
            DropSpecialPotions = new MobRateWatcher(int.MaxValue);
            LuckyAmulet = new MobRateWatcher(300);

            Chi100 = new MobRateWatcher(int.MaxValue);
            Chi300 = new MobRateWatcher(int.MaxValue);
            MoonBox = new MobRateWatcher(int.MaxValue);
            Study20 = new MobRateWatcher(int.MaxValue);
            Bomb = new MobRateWatcher(int.MaxValue);
            CuteCPPack = new MobRateWatcher(int.MaxValue);
            DragonBalls = new MobRateWatcher(Game.Era1.Era1Economy.DragonBallDropEvery);
        }

        public uint GeneratePotionExtra(bool Special = false)
        {
            if (Special)
            {
                return SeaPotions[Pool.GetRandom.Next(0, SeaPotions.Length)];
            }

            if (DropSpecialPotions)
            {
                return SeaPotions[Pool.GetRandom.Next(0, SeaPotions.Length)];
            }
            return 0;
        }
        public uint GenerateGoldBoss(out uint ItemID, uint amount)
        {
            ItemID = Database.ItemType.MoneyItemID((uint)amount);
            return amount;
        }
        public List<uint> GenerateSoulsItems(ushort level, uint count = 1, ushort minlevel = 1, ushort maxlevel = 5)
        {
            if (level == 0)
                level = (ushort)Role.Core.Random.Next(minlevel, maxlevel);
            List<uint> items = new List<uint>();
            if (Database.ItemType.PurificationItems.ContainsKey(level))
            {
                var array = Database.ItemType.PurificationItems[level].Values.ToArray();
                for (int x = 0; x < (int)(count == 0 ? 1 : count); x++)
                {
                    int position = Pool.GetRandom.Next(0, array.Length);
                    items.Add(array[position].ID);
                }
            }
            return items;
        }
        public List<uint> GenerateRefineryItems(ushort level)
        {
            if (level == 0)
                level = (ushort)Role.Core.Random.Next(1, 5);
            if (Role.MyMath.Success(0.001))
                level = 5;
            List<uint> items = new List<uint>();
            byte count = 1;
            if (Database.ItemType.Refinary.ContainsKey(level))
            {
                var array = Database.ItemType.Refinary[level].Values.ToArray();
                for (int x = 0; x < (int)(count == 0 ? 1 : count); x++)
                {
                    int position = Pool.GetRandom.Next(0, array.Length);
                    items.Add(array[position].ItemID);
                }
            }

            //genereate accessory
            //var Accessorys = Database.ItemType.Accessorys.Values.ToArray();
            //var rand = (ushort)(Pool.GetRandom.Next() % 1000);
            //count = (byte)(rand % 3);
            //for (int x = 0; x < count; x++)
            //{
            //    int position = Pool.GetRandom.Next(0, Accessorys.Length);
            //    items.Add(Accessorys[position].ID);
            //}

            //--------------------------

            //if (level <= 3)
            //    items.Add(723341);//20 study points
            //else if (level > 3)
            //    items.Add(723342);//500 study points

            return items;
        }
        public List<uint> GenerateBossFamily()
        {
            List<uint> Items = new List<uint>();
            byte rand = (byte)Pool.GetRandom.Next(1, 7);
            for (int x = 0; x < 4; x++)
            {
                byte dwItemQuality = GenerateQuality();
                uint dwItemSort = 0;
                uint dwItemLev = 0;
                switch (rand)
                {
                    case 1:
                        {
                            dwItemSort = NecklaceType[Pool.GetRandom.Next(0, NecklaceType.Length)];
                            dwItemLev = Family.DropNecklace;
                            break;
                        }
                    case 2:
                        {
                            dwItemSort = RingType[Pool.GetRandom.Next(0, RingType.Length)];
                            dwItemLev = Family.DropRing;
                            break;
                        }
                    case 3:
                        {
                            dwItemSort = ArmorType[Pool.GetRandom.Next(0, ArmorType.Length)];
                            dwItemLev = Family.DropArmor;
                            break;
                        }
                    case 4:
                        {
                            dwItemSort = TwoHanderType[Pool.GetRandom.Next(0, TwoHanderType.Length)];
                            dwItemLev = ((dwItemSort == 900) ? Family.DropShield : Family.DropWeapon);
                            break;
                        }
                    default:
                        {
                            dwItemSort = OneHanderType[Pool.GetRandom.Next(0, OneHanderType.Length)];
                            dwItemLev = Family.DropWeapon;
                            break;
                        }
                }
                dwItemLev = AlterItemLevel(dwItemLev, dwItemSort);
                uint idItemType = (dwItemSort * 1000) + (dwItemLev * 10) + dwItemQuality;
                if (Pool.ItemsBase.ContainsKey(idItemType))
                    Items.Add(idItemType);
            }
            return Items;
        }
        public uint GenerateItemId(uint map, out byte dwItemQuality, out bool Special, out Database.ItemType.DBItem DbItem)
        {
            Special = false;
            foreach (SpecialItemWatcher sp in Family.DropSpecials)
            {
                if (sp.Rate)
                {
                    Special = true;
                    dwItemQuality = (byte)(sp.ID % 10);
                    if (Pool.ItemsBase.TryGetValue(sp.ID, out DbItem))
                        return sp.ID;
                }
            }
            if (DropHp)
            {
                dwItemQuality = 0;
                Special = true;
                if (Pool.ItemsBase.TryGetValue(Family.DropHPItem, out DbItem))
                    return Family.DropHPItem;
            }
            if (DropMp)
            {
                dwItemQuality = 0;
                Special = true; if (Pool.ItemsBase.TryGetValue(Family.DropMPItem, out DbItem))
                    return Family.DropMPItem;
            }
            dwItemQuality = 0;
            DbItem = null;
            return 0;
        }
        public byte GeneratePurity()
        {
            if (PlusOne)
                return 1;
            if (PlusTwo)
                return 2;
            return 0;
        }
        public byte GenerateBless()
        {
            // Blessed equipment is not part of normal Era 1 monster generation.
            return 0;
        }
        public byte GenerateSocketCount(uint ItemID)
        {
            // Random 1/2-socket equipment drops at the 5695 rates are an economy faucet.
            // Era 1 sockets are obtained through the classic upgrade/socket mechanics.
            return 0;
        }
        public byte GenerateQuality()
        {
            if (Super)
                return 9;
            if (Elite)
                return 8;
            if (Unique)
                return 7;
            if (Refined)
                return 6;
            return 3;
        }
        public uint AlterItemLevel(uint dwItemLev, uint dwItemSort)
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
