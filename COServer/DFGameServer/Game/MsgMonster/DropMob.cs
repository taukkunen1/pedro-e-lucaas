using Core;
using GameServer.Client;
using GameServer.Game.MsgMonster;
using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using System;
using System.Threading.Tasks;

namespace GameServer
{
    public class DropMob
    {
        public static bool Checkup(ServerSockets.Packet stream, GameClient killer, MonsterRole Mob)
        {
            bool returnBoolean = false;
            if (killer == null)
                return false;

            ushort xx = Mob.X;
            ushort yy = Mob.Y;

            Parallel.ForEach(ConfigurableDropSystem.Drops, (Drop) => {
                if (killer == null) return;
                if (Utils.Rate(Drop.Percent) && Drop.Enabled)
                {
                    switch (Drop.Type)
                    {
                        case ConfigurableDropSystem.DropType.Money:
                            {
                                if (Drop.MoneyDrop.Type == ConfigurableDropSystem.MoneyType.Money)
                                {
                                    uint Money_Itemid;
                                    uint money_value = GenerateGold(out Money_Itemid, Mob, Drop.MoneyDrop);
                                    if (killer.Player.VipLevel >= 3)
                                    {
                                        if (killer.AutoHunting.Enable)
                                        {
                                            if (killer.AutoHunting.LootMoney)
                                            {
                                                killer.Player.Money += money_value;
                                                killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + $"] You get {money_value} of Money in Inventory.", MsgMessage.ChatMode.TopLeft);
                                            }
                                            else
                                            {
                                                if (killer.Map.AddGroundItem(ref xx, ref yy) && Money_Itemid > 0)
                                                {
                                                    Mob.DropItem(stream, killer.Player.UID, killer.Map, Money_Itemid, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Money, money_value, false, 0);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            killer.Player.Money += money_value;
                                            killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + $"] You get {money_value} of Money in Inventory.", MsgMessage.ChatMode.TopLeft);
                                        }
                                    }
                                    else
                                    {
                                        if (killer.Map.AddGroundItem(ref xx, ref yy) && Money_Itemid > 0)
                                        {
                                            Mob.DropItem(stream, killer.Player.UID, killer.Map, Money_Itemid, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Money, money_value, false, 0);
                                        }
                                    }
                                }
                                // Era 1 hunting economy uses silver drops, not direct CP/Bound-CP monster rewards.
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.MeteorScroll:
                            {
                                uint ID = 1088001;
                                if (killer.Player.VipLevel >= 3)
                                {
                                    if (killer.AutoHunting.Enable)
                                    {
                                        if (killer.AutoHunting.Meteors)
                                        {
                                            if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                            {
                                                killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + $"] You got a Meteor in your inventory.", MsgMessage.ChatMode.TopLeft);
                                            }
                                        }
                                        else
                                        {
                                            Mob.DropItemID(killer, ID, stream, 3, true);
                                        }
                                    }
                                    else
                                    {
                                        if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                        {
                                            killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + $"] You got a Meteor in your inventory.", MsgMessage.ChatMode.TopLeft);
                                        }
                                    }
                                }
                                else
                                {
                                    Mob.DropItemID(killer, ID, stream, 3, true);
                                }
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.Item:
                            {
                                byte ID_Quality;
                                bool ID_Special;
                                uint ID = GenerateItemId(Mob, Mob.Map, out ID_Quality, out ID_Special, out Database.ItemType.DBItem DbItem);
                                if (killer.Player.VipLevel >= 3 && ID > 0)
                                {
                                    if (killer.AutoHunting.Enable)
                                    {
                                        if (killer.AutoHunting.MaterialItems)
                                        {
                                            if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                            {
                                                killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + $"] You got a RandomItem in your inventory.", MsgMessage.ChatMode.TopLeft);
                                            }
                                        }
                                        else
                                        {
                                            if (killer.Map.AddGroundItem(ref xx, ref yy))
                                            {
                                                DropItem(stream, Mob, killer.Player.UID, killer.Map, ID, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, ID_Special, ID_Quality, killer, DbItem);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                        {
                                            killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + $"] You got a RandomItem in your inventory.", MsgMessage.ChatMode.TopLeft);
                                        }
                                    }
                                }
                                else
                                {
                                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                                    {
                                        DropItem(stream, Mob, killer.Player.UID, killer.Map, ID, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, ID_Special, ID_Quality, killer, DbItem);
                                    }
                                }
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.Stone:
                        case ConfigurableDropSystem.DropType.ExpBall:
                        case ConfigurableDropSystem.DropType.Letter:
                        case ConfigurableDropSystem.DropType.PowerEXPBall:
                            {
                                // Later/custom economy drops are not part of the 5017 Era 1 hunting loop.
                                break;
                            }
#if false
                        case ConfigurableDropSystem.DropType.Stone_Legacy:
                            {
                                ushort randomStonePlus = (ushort)Role.Core.Random.Next(1, 3);
                                uint ID = StoneId(randomStonePlus);
                                if (killer.Player.VipLevel >= 3)
                                {
                                    if (killer.AutoHunting.Enable)
                                    {
                                        if (killer.AutoHunting.PlusItems)
                                        {
                                            if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                            {
                                                killer.SendSysMesage($"[VIP-" + killer.Player.VipLevel + $"] You got a Stone(+{randomStonePlus}) in your inventory.", MsgMessage.ChatMode.TopLeft);
                                            }
                                        }
                                        else
                                        {
                                            if (killer.Map.AddGroundItem(ref xx, ref yy))
                                            {
                                                Mob.DropItem(stream, killer.Player.UID, killer.Map, ID, xx, yy,
                                                    Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0);
                                                killer.SendSysMesage($"A Stone(+{randomStonePlus}) dropped at (" + xx + "," + yy + ")!", MsgMessage.ChatMode.Talk);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                        {
                                            killer.SendSysMesage($"[VIP-" + killer.Player.VipLevel + $"] You got a Stone(+{randomStonePlus}) in your inventory.", MsgMessage.ChatMode.TopLeft);
                                        }
                                    }
                                }
                                else
                                {
                                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                                    {
                                        Mob.DropItem(stream, killer.Player.UID, killer.Map, ID, xx, yy,
                                            Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0);
                                        killer.SendSysMesage($"A Stone(+{randomStonePlus}) dropped at (" + xx + "," + yy + ")!", MsgMessage.ChatMode.Talk);
                                    }
                                }
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.ExpBall:
                            {
                                uint ID = 722136;
                                if (killer.Player.VipLevel >= 3)
                                {
                                    if (killer.AutoHunting.Enable)
                                    {
                                        if (killer.AutoHunting.ExpBallEventItems)
                                        {
                                            killer.Inventory.AddItemWitchStack(ID, 0, 1, stream);
                                            killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + "] You got a ExpBall in your inventory.", MsgMessage.ChatMode.TopLeft);
                                        }
                                        else
                                        {
                                            Mob.DropItemID(killer, ID, stream);
                                        }
                                    }
                                    else
                                    {
                                        killer.Inventory.AddItemWitchStack(ID, 0, 1, stream);
                                        killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + "] You got a ExpBall in your inventory.", MsgMessage.ChatMode.TopLeft);
                                    }
                                }
                                else
                                {
                                    killer.SendSysMesage("A ExpBall dropped at (" + xx + "," + yy + ")!", MsgMessage.ChatMode.TopLeft);
                                    Mob.DropItemID(killer, ID, stream);
                                }
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.Letter:
                            {
                                byte rand2 = (byte)Pool.GetRandom.Next(0, 7);
                                uint ID = 711214;
                                switch (rand2)
                                {
                                    case 0:
                                        {
                                            ID = 711214;
                                            killer.SendSysMesage("LetterC Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                    case 1:
                                        {
                                            ID = 711215;
                                            killer.SendSysMesage("LetterO Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                    case 2:
                                        {
                                            ID = 711216;
                                            killer.SendSysMesage("LetterN Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                    case 3:
                                        {
                                            ID = 711217;
                                            killer.SendSysMesage("LetterQ Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                    case 4:
                                        {
                                            ID = 711218;
                                            killer.SendSysMesage("LetterU Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                    case 5:
                                        {
                                            ID = 711219;
                                            killer.SendSysMesage("LetterE Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                    case 6:
                                        {
                                            ID = 711220;
                                            killer.SendSysMesage("LetterR Dropped.", MsgMessage.ChatMode.TopLeft);
                                            break;
                                        }
                                }
                                Mob.DropItemID(killer, ID, stream, 3, true);
                                killer.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "dispel7");
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.PowerEXPBall:
                            {
                                uint ID = 722057;
                                if (killer.Player.VipLevel >= 3)
                                {
                                    if (killer.AutoHunting.Enable)
                                    {
                                        if (killer.AutoHunting.ExpBallEventItems)
                                        {
                                            if (killer.Inventory.HaveSpace(1))
                                            {
                                                if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                                {
                                                    killer.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "dispel7");
                                                    killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + "] You got a PowerEXPBall in your inventory.", MsgMessage.ChatMode.System);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Mob.DropItemID(killer, ID, stream, 3, true);
                                        }
                                    }
                                    else
                                    {
                                        if (killer.Inventory.HaveSpace(1))
                                        {
                                            if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                            {
                                                killer.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "dispel7");
                                                killer.SendSysMesage("[VIP-" + killer.Player.VipLevel + "] You got a PowerEXPBall in your inventory.", MsgMessage.ChatMode.System);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Mob.DropItemID(killer, ID, stream, 3, true);
                                }
                                returnBoolean = true;
                                break;
                            }
#endif
                        case ConfigurableDropSystem.DropType.DragonBall:
                            {
                                uint ID = 1088000;
                                if (killer.Player.VipLevel >= 3)
                                {
                                    if (killer.AutoHunting.Enable)
                                    {
                                        if (killer.AutoHunting.DBalls)
                                        {
                                            if (killer.Inventory.HaveSpace(1))
                                            {
                                                if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                                {
                                                    killer.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "dispel7");
                                                    if (killer.Inventory.Contain(1088000, 10) && killer.Player.VipLevel == 6)
                                                    {
                                                        killer.Inventory.Remove(1088000, 10, stream);
                                                        killer.Inventory.Add(stream, 720028, 1);
                                                        killer.SendSysMesage("[VIP-6] DBScroll got autopacked.", MsgMessage.ChatMode.TopLeft);
                                                        returnBoolean = true;
                                                    }
                                                    killer.SendSysMesage("[VIP-6] You got a DragonBall in your inventory.", MsgMessage.ChatMode.TopLeft);
                                                    returnBoolean = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Mob.DropItemID(killer, ID, stream, 3, true);
                                        }
                                    }
                                    else
                                    {
                                        if (killer.Inventory.HaveSpace(1))
                                        {
                                            if (killer.Inventory.AddItemWitchStack(ID, 0, 1, stream))
                                            {
                                                killer.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "dispel7");
                                                if (killer.Inventory.Contain(1088000, 10) && killer.Player.VipLevel == 6)
                                                {
                                                    killer.Inventory.Remove(1088000, 10, stream);
                                                    killer.Inventory.Add(stream, 720028, 1);
                                                    killer.SendSysMesage("[VIP-6] DBScroll got autopacked.", MsgMessage.ChatMode.TopLeft);
                                                    returnBoolean = true;
                                                }
                                                killer.SendSysMesage("[VIP-6] You got a DragonBall in your inventory.", MsgMessage.ChatMode.TopLeft);
                                                returnBoolean = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Mob.DropItemID(killer, ID, stream, 3, true);
                                }
                                returnBoolean = true;
                                break;
                            }
                    }
                }
            });

            return returnBoolean;
        }
        public static uint StoneId(UInt16 Plus)
        {
            switch (Plus)
            {
                case 1: return 730001;
                case 2: return 730002;
                case 3: return 730003;
                case 4: return 730004;
                case 5: return 730005;
                case 6: return 730006;
                case 7: return 730007;
                case 8: return 730008;
            }
            return 0;
        }
        public static uint GenerateGold(out uint ItemID, MonsterRole Mob, MoneyDrop MoneyDrop)
        {
            uint amount;
            if (Mob.Boss != 0)
                amount = (uint)Pool.GetRandom.Next(Mob.Family.DropMoney, Mob.Family.DropMoney * 10);
            else
            {
                amount = (uint)Pool.GetRandom.Next((int)MoneyDrop.Min, (int)MoneyDrop.Max);
            }
            ItemID = Database.ItemType.MoneyItemID(amount);
            return amount;
        }
        public static void DropItem(ServerSockets.Packet stream, MonsterRole Mob, uint OwnerItem, Role.GameMap map, uint ItemID, ushort XX, ushort YY, Game.MsgFloorItem.MsgItem.ItemType typ
            , uint amount, bool special, byte ID_Quality, Client.GameClient user = null, Database.ItemType.DBItem DBItem = null)
        {
            Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
            if (!Game.Era1.Era1Items.IsAllowedGeneratedDrop(ItemID)) return;
            DataItem.ITEM_ID = ItemID;
            if (DataItem.Durability > 100)
            {
                DataItem.Durability = (ushort)Pool.GetRandom.Next(100, DataItem.Durability / 10);
                DataItem.MaximDurability = DataItem.Durability;
            }

            else
            {
                DataItem.Durability = (ushort)Pool.GetRandom.Next(1, 10);
                DataItem.MaximDurability = 10;
            }
            DataItem.Color = Role.Flags.Color.Red;
            if (typ == Game.MsgFloorItem.MsgItem.ItemType.Item)
            {
                if (DataItem.IsEquip)
                {
                    if (!special)
                    {
                        if (ID_Quality == 3)
                        {
                            // Hunting may MINT a scarce +1. +2 and above are composition-only BURN progression.
                            // Blessing and free double-socket rolls from the 5695/custom drop path are disabled.
                            DataItem.Plus = Game.Era1.Era1Economy.RollHuntingPlus();
                        }
                        if (DBItem != null)
                        {
                            DataItem.Durability = (ushort)Pool.GetRandom.Next(1, DBItem.Durability / 10 + 10);
                            DataItem.MaximDurability = (ushort)Pool.GetRandom.Next(DataItem.Durability, DBItem.Durability);
                        }
                    }
                }
                else
                {
                    if (DBItem != null)
                        DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                }
                if (DataItem.Bless > 0)
                {
                    if (user.Inventory.HaveSpace(1))
                    {
                        if (DBItem != null)
                        {
                            if (user.AutoHunting.Enable)
                            {
                                if (user.AutoHunting.BlessedItems)
                                {
                                    user.Inventory.Add(DataItem, DBItem, stream);
                                    user.SendSysMesage("A " + DBItem.Name + "[-" + DataItem.Bless + " Blessed] got autopacked.", MsgMessage.ChatMode.Talk);
                                } else
                                {
                                    Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                                }
                            } else
                            {
                                Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                            }
                            return;
                        }
                    }
                    else
                    {
                        Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                    }
                }
                if (DataItem.Plus > 0)
                {
                    if (user.Inventory.HaveSpace(1))
                    {
                        if (DBItem != null)
                        {
                            if (user.AutoHunting.Enable)
                            {
                                if (user.AutoHunting.PlusItems)
                                {
                                    user.Inventory.Add(DataItem, DBItem, stream);
                                    user.SendSysMesage("A " + DBItem.Name + "[+" + DataItem.Plus + "] got autopacked.", MsgMessage.ChatMode.Talk);
                                }
                                else
                                {
                                    Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                                }
                            }
                            else
                            {
                                Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                            }
                            return;
                        }
                    }
                    else
                    {
                        Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                    }
                }
                if (DataItem.SocketTwo > 0)
                {
                    if (user.Inventory.HaveSpace(1))
                    {
                        if (DBItem != null)
                        {
                            if (user.AutoHunting.Enable)
                            {
                                if (user.AutoHunting.SocketedItems)
                                {
                                    user.Inventory.Add(DataItem, DBItem, stream);
                                    user.SendSysMesage("A " + DBItem.Name + "[2soc] got autopacked.", MsgMessage.ChatMode.Talk);
                                }
                                else
                                {
                                    Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                                }
                            }
                            else
                            {
                                Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                            }
                            return;
                        }
                    }
                    else
                    {
                        Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                    }
                }
                if (ID_Quality == 9)
                {
                    if (user.Inventory.HaveSpace(1))
                    {
                        if (DBItem != null)
                        {
                            if (user.AutoHunting.Enable)
                            {
                                if (user.AutoHunting.QualityItems)
                                {
                                    user.Inventory.Add(DataItem, DBItem, stream);
                                    user.SendSysMesage("A " + DBItem.Name + "[Super] got autopacked.", MsgMessage.ChatMode.Talk);
                                }
                                else
                                {
                                    Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                                }
                            }
                            else
                            {
                                Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                            }
                            return;
                        }
                    }
                    else
                    {
                        Mob.DropItemID(user, DBItem.ID, stream, 3, true);
                    }
                }
            }
            Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(DataItem, XX, YY, typ, amount, Mob.DynamicID, Mob.Map, OwnerItem, true, map);

            if (map.EnqueueItem(DropItem))
            {
                DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);
            }
        }

        public static uint GenerateItemId(MonsterRole Mob, uint map, out byte dwItemQuality, out bool Special, out Database.ItemType.DBItem DbItem)
        {
            Special = false;
            dwItemQuality = Game.Era1.Era1Economy.RollEquipmentQuality();

            uint dwItemSort = 0;
            uint dwItemLev = 0;
            int nRand = BaseFunc.RandGet(1200, false);
            if (nRand >= 0 && nRand < 20) // 0.17%
            {
                dwItemSort = 160;
                dwItemLev = Mob.Family.DropBoots;
            }
            else if (nRand >= 20 && nRand < 50) // 0.25%
            {
                dwItemSort = MobItemGenerator.NecklaceType[BaseFunc.RandGet(MobItemGenerator.NecklaceType.Length, false)];
                dwItemLev = Mob.Family.DropNecklace;
            }
            else if (nRand >= 50 && nRand < 100) // 4.17%
            {
                dwItemSort = MobItemGenerator.RingType[BaseFunc.RandGet(MobItemGenerator.RingType.Length, false)];
                dwItemLev = Mob.Family.DropRing;
            }
            else if (nRand >= 100 && nRand < 400) // 25%
            {
                dwItemSort = MobItemGenerator.ArmetType[BaseFunc.RandGet(MobItemGenerator.ArmetType.Length, false)];
                dwItemLev = Mob.Family.DropArmet;
            }
            else if (nRand >= 400 && nRand < 700) // 25%
            {
                dwItemSort = MobItemGenerator.ArmorType[BaseFunc.RandGet(MobItemGenerator.ArmorType.Length, false)];
                dwItemLev = Mob.Family.DropArmor;
            }
            else // 45%
            {
                int nRate = BaseFunc.RandGet(100, false);
                if (nRate >= 0 && nRate < 20) // 20% of 45% (= 9%) - Backswords
                {
                    dwItemSort = 421;
                }
                else if (nRate >= 40 && nRate < 80)	// 40% of 45% (= 18%) - One handers
                {
                    dwItemSort = MobItemGenerator.OneHanderType[BaseFunc.RandGet(MobItemGenerator.OneHanderType.Length, false)];
                    dwItemLev = Mob.Family.DropWeapon;
                }
                else if (nRate >= 80 && nRate < 100)// 20% of 45% (= 9%) - Two handers (and shield)
                {
                    dwItemSort = MobItemGenerator.TwoHanderType[BaseFunc.RandGet(MobItemGenerator.TwoHanderType.Length, false)];
                    dwItemLev = ((dwItemSort == 900) ? Mob.Family.DropShield : Mob.Family.DropWeapon);
                }
            }
            if (dwItemLev != 99)
            {
                dwItemLev = Mob.Family.ItemGenerator.AlterItemLevel(dwItemLev, dwItemSort);
                uint idItemType = (dwItemSort * 1000) + (dwItemLev * 10) + dwItemQuality;
                if (Pool.ItemsBase.TryGetValue(idItemType, out DbItem))
                {
                    ushort position = Database.ItemType.ItemPosition(idItemType);
                    byte level = Database.ItemType.ItemMaxLevel((Role.Flags.ConquerItem)position);
                    if (DbItem.Level > level)
                        return 0;
                    return idItemType;
                }
            }
            DbItem = null;
            return 0;
        }

    }
}
