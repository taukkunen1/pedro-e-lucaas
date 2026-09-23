using Core;
using GameServer.Client;
using GameServer.Game.MsgMonster;
using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using System;

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

            // A kill has only a handful of configured faucet rolls. Run them sequentially:
            // xx/yy are mutated by AddGroundItem(ref ...), so the old Parallel.ForEach introduced
            // a data race inside a single monster death and made drop placement/RNG auditing noisy.
            foreach (var Drop in ConfigurableDropSystem.Drops)
            {
                if (Utils.Rate(Drop.Percent) && Drop.Enabled)
                {
                    switch (Drop.Type)
                    {
                        case ConfigurableDropSystem.DropType.Money:
                            {
                                if (Drop.MoneyDrop.Type == ConfigurableDropSystem.MoneyType.Money)
                                {
                                    uint moneyItemId;
                                    uint moneyValue = GenerateGold(out moneyItemId, Mob, Drop.MoneyDrop);
                                    if (killer.Map.AddGroundItem(ref xx, ref yy) && moneyItemId > 0)
                                        Mob.DropItem(stream, killer.Player.UID, killer.Map, moneyItemId, xx, yy,
                                            Game.MsgFloorItem.MsgItem.ItemType.Money, moneyValue, false, 0);
                                }
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.MeteorScroll:
                            {
                                // Despite the legacy enum name this is one Meteor, not a scroll.
                                Mob.DropItemID(killer, Database.ItemType.Meteor, stream, 3, true);
                                returnBoolean = true;
                                break;
                            }
                        case ConfigurableDropSystem.DropType.Item:
                            {
                                byte quality;
                                bool special;
                                uint id = GenerateItemId(Mob, Mob.Map, out quality, out special, out Database.ItemType.DBItem dbItem);
                                if (id > 0 && killer.Map.AddGroundItem(ref xx, ref yy))
                                    DropItem(stream, Mob, killer.Player.UID, killer.Map, id, xx, yy,
                                        Game.MsgFloorItem.MsgItem.ItemType.Item, 0, special, quality, killer, dbItem);
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
                                // Era 1: Dragon Balls are physical world drops. VIP/autohunt
                                // auto-loot and automatic DB-scroll packing are later conveniences
                                // and bypass the intended hunting/market loop.
                                Mob.DropItemID(killer, Database.ItemType.DragonBall, stream, 3, true);
                                returnBoolean = true;
                                break;
                            }
                    }
                }
            }

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
            // Era 1: the monster database is the source of truth for Gold.
            // The old 5695/custom path used the same global 1,000-2,000 range for every
            // normal monster, which made low-level hunting an oversized Gold faucet.
            uint baseAmount = Mob.Family.DropMoney;
            if (baseAmount == 0)
            {
                ItemID = 0;
                return 0;
            }

            uint amount = baseAmount;
            if (Mob.Boss != 0)
            {
                int min = (int)baseAmount;
                int max = (int)System.Math.Min(int.MaxValue, (long)baseAmount * 10L);
                amount = max > min ? (uint)Pool.GetRandom.Next(min, max) : baseAmount;
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
            if (nRand >= 0 && nRand < 20) // 1.67%
            {
                dwItemSort = 160;
                dwItemLev = Mob.Family.DropBoots;
            }
            else if (nRand >= 20 && nRand < 50) // 2.50%
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
            else // 41.67%
            {
                int nRate = BaseFunc.RandGet(100, false);
                if (nRate >= 0 && nRate < 20) // 20% of weapon drops (= 8.33% overall) - Backswords
                {
                    dwItemSort = 421;
                    dwItemLev = Mob.Family.DropWeapon;
                }
                else if (nRate >= 20 && nRate < 80) // 60% of weapon drops (= 25.00% overall) - One handers
                {
                    dwItemSort = MobItemGenerator.OneHanderType[BaseFunc.RandGet(MobItemGenerator.OneHanderType.Length, false)];
                    dwItemLev = Mob.Family.DropWeapon;
                }
                else // 20% of weapon drops (= 8.33% overall) - Two handers (and shield)
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
