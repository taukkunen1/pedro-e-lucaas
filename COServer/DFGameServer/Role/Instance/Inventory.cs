using GameServer.Game.MsgServer;
using GameServer.Instance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Pool;

namespace GameServer.Role.Instance
{

    public class Inventory
    {
        private const byte File_Size = 40;

        public ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem> ClientItems = new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>();


        public bool AddAccess(ServerSockets.Packet stream, uint ID, byte count = 1, bool Activate = true, uint DaysRemain = uint.MaxValue, bool bound = false)
        {
            if (global::Core.Features.FeatureRegistry.IsBlockedItem(ID) || Game.Era1.Era1Items.IsBlockedEquipment(ID)) return false; // [feature-gate items]
            if (ID == 1088000)
            {

            }
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                byte x = 0;
                for (; x < count;)
                {
                    x++;
                    Database.ItemType.DBItem DbItem;
                    if (Pool.ItemsBase.TryGetValue(ID, out DbItem))
                    {

                        Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                        //ItemDat.UID = Pool.ITEM_Counter.Next;
                        ItemDat.ITEM_ID = ID;
                        ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                        if (bound) ItemDat.Bound = 1;
                        ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                        if (Activate)
                            ItemDat.Activate = 1;

                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }

                    }
                }
                if (x >= count)
                    return true;
            }

            return false;
        }
        public int GetCountItem(uint ItemID)
        {
            int count = 0;
            foreach (var DataItem in ClientItems.Values)
            {
                if (DataItem.ITEM_ID == ItemID)
                {
                    count += DataItem.StackSize > 1 ? DataItem.StackSize : 1;
                }
            }
            return count;
        }

        public bool VerifiedUpdateItem(List<uint> ItemsUIDS, uint ID, byte count, out Queue<Game.MsgServer.MsgGameItem> Items)
        {
            Queue<Game.MsgServer.MsgGameItem> ExistItems = new Queue<Game.MsgServer.MsgGameItem>();
            foreach (var DataItem in ClientItems.Values)
            {
                if (DataItem.ITEM_ID == ID)
                {
                    if (ItemsUIDS.Contains(DataItem.UID))
                    {
                        count--;
                        ItemsUIDS.Remove(DataItem.UID);
                        ExistItems.Enqueue(DataItem);
                    }
                }
            }
            Items = ExistItems;
            return ItemsUIDS.Count == 0 && count == 0;
        }

        private Client.GameClient Owner;
        public Inventory(Client.GameClient _own)
        {
            Owner = _own;
        }

        public void AddDBItem(Game.MsgServer.MsgGameItem item)
        {
            ClientItems.TryAdd(item.UID, item);
        }

        public void AddReturnedItem(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, byte bless = 0, byte Enchant = 0
            , Role.Flags.Gem sockone = Flags.Gem.NoSocket
             , Role.Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false, Role.Flags.ItemEffect Effect = Flags.ItemEffect.None, ushort StackSize = 0)
        {
            if (global::Core.Features.FeatureRegistry.IsBlockedItem(ID)) return; // [feature-gate items]

            byte x = 0;
            for (; x < count;)
            {
                x++;
                Database.ItemType.DBItem DbItem;
                if (ItemsBase.TryGetValue(ID, out DbItem))
                {

                    Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                    //ItemDat.UID = ITEM_Counter.Next;
                    ItemDat.ITEM_ID = ID;
                    ItemDat.Effect = Effect;
                    ItemDat.StackSize = StackSize;
                    ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                    ItemDat.Plus = plus;
                    ItemDat.Bless = bless;
                    ItemDat.Enchant = Enchant;
                    ItemDat.SocketOne = sockone;
                    ItemDat.SocketTwo = socktwo;
                    ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                    ItemDat.Bound = (byte)(bound ? 1 : 0);
                    ItemDat.Mode = Flags.ItemMode.AddItemReturned;
                    ItemDat.WH_ID = ushort.MaxValue;
                    if (DbItem.TimeItems != 0 && DbItem.StackSize == 1)
                    {
                        ItemDat.Activate = 1;
                        ItemDat.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                    }
                    Owner.Warehouse.AddItem(ItemDat, ushort.MaxValue);

                    ItemDat.Send(Owner, stream);
                }
            }
        }

        public bool CollectedMoonBoxTokens(byte bound = 0)
        {
            return Contain(721010, 1, bound) && Contain(721011, 1, bound) && Contain(721012, 1, bound) && Contain(721013, 1, bound) && Contain(721014, 1, bound) && Contain(721015, 1, bound);
        }
        public bool CollectedAnyMoonBoxTokens()
        {
            return Contain(721010, 1) || Contain(721011, 1) || Contain(721012, 1) || Contain(721013, 1) || Contain(721014, 1) || Contain(721015, 1);
        }
        public bool CollectedTokens(uint ID)
        {
            if (ID == 1044)
            {
                return Contain(721011, 1);
            }
            if (ID == 1046)
            {
                return Contain(721013, 1);
            }
            if (ID == 1048)
            {
                return Contain(721015, 1);
            }
            if (ID == 1045)
            {
                return Contain(721012, 1);
            }
            if (ID == 1043)
            {
                return Contain(721010, 1);
            }
            if (ID == 1047)
            {
                return Contain(721014, 1);
            }
            return false;
        }

        public bool HaveSpace(byte count)
        {
            return (ClientItems.Count + count) <= File_Size;
        }

        public bool TryGetItem(uint UID, out Game.MsgServer.MsgGameItem item)
        {
            return ClientItems.TryGetValue(UID, out item);
        }
        public bool SearchItemByID(uint ID, out Game.MsgServer.MsgGameItem item)
        {
            foreach (var msg_item in ClientItems.Values)
            {
                if (msg_item.ITEM_ID == ID)
                {
                    item = msg_item;
                    return true;
                }
            }
            item = null;
            return false;
        }

        public bool SearchItemByID(uint ID, byte count, out List<Game.MsgServer.MsgGameItem> Items)
        {
            byte increase = 0;
            Items = new List<Game.MsgServer.MsgGameItem>();
            foreach (var msg_item in ClientItems.Values)
            {
                if (msg_item.ITEM_ID == ID)
                {
                    Items.Add(msg_item);
                    increase++;
                    if (increase == count)
                    {
                        return true;
                    }
                }
            }
            Items = null;
            return false;
        }
        public bool Contain(uint ID, uint Amount, byte bound = 0)
        {
            if (ID == 711214 && ID == 711215 && ID == 711216 && ID == 711217 && ID == 711218 && ID == 711219 && ID == 711220)
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == 711214
                        && item.ITEM_ID == 711215 && item.ITEM_ID == 711216 && item.ITEM_ID == 711217 && item.ITEM_ID == 711218 && item.ITEM_ID == 711219 && item.ITEM_ID == 711220)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            else if (ID == 711301 && ID == 711302 && ID == 711303 && ID == 711304 && ID == 711305)
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == 711301 && item.ITEM_ID == 711302 && item.ITEM_ID == 7113013 && item.ITEM_ID == 711304 && item.ITEM_ID == 711305)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            else if (ID == 723468 && ID == 723469)
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == 727999 && item.ITEM_ID == 723469)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            else if (ID == 720364 && ID == 720362 && ID == 720365 && ID == 710968 && ID == 720157)
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == 720364 && item.ITEM_ID == 720362 && item.ITEM_ID == 720365 && item.ITEM_ID == 710968 && item.ITEM_ID == 720157)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            else if (ID == Database.ItemType.MoonBox || ID == 723087)//execept for bound
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == ID)
                    {
                        count += item.StackSize;
                        if (count >= Amount)
                            return true;
                    }
                }
            }
            else
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == ID)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public uint HasMoonBox(uint Amount = 1)
        {
            uint count = 0;
            foreach (var item in ClientItems.Values)
            {
                if (item.ITEM_ID >= Database.ItemType.MoonBox && item.ITEM_ID <= Database.ItemType.MoonBoxLast || (item.ITEM_ID >= 721030 && item.ITEM_ID <= 721035 || (item.ITEM_ID >= 721040 && item.ITEM_ID <= 721045) || (item.ITEM_ID >= 721050 && item.ITEM_ID <= 721055)) || (item.ITEM_ID >= 721060 && item.ITEM_ID <= 721065) || (item.ITEM_ID >= 721080 && item.ITEM_ID <= 721084) || item.ITEM_ID == 721090)
                {
                    count += item.StackSize;
                    if (count >= Amount)
                        return item.ITEM_ID;
                }
            }
            return count;
        }

        public bool Remove(uint ID, uint count, ServerSockets.Packet stream)
        {
            if (Contain(ID, count) || Contain(ID, count, 1))
            {
                if (ID == Database.ItemType.Meteor || ID == Database.ItemType.MeteorTear)
                {
                    byte removed = 0;
                    for (byte x = 0; x < count; x++)
                    {
                        foreach (var item in ClientItems.Values)
                        {
                            if (item.ITEM_ID == Database.ItemType.Meteor
                         || item.ITEM_ID == Database.ItemType.MeteorTear)
                            {
                                try
                                {
                                    Update(item, AddMode.REMOVE, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                                removed++;
                                if (removed == count)
                                    break;
                            }
                        }
                        if (removed == count)
                            break;
                    }
                }
                else
                {
                    byte removed = 0;
                    for (byte x = 0; x < count; x++)
                    {
                        foreach (var item in ClientItems.Values)
                        {
                            if (item.ITEM_ID == ID)
                            {
                                try
                                {
                                    Update(item, AddMode.REMOVE, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                                removed++;
                                if (removed == count)
                                    break;
                            }
                        }
                        if (removed == count)
                            break;
                    }
                }
                return true;
            }
            return false;
        }
        public bool AddSteed(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, bool bound = false, byte ProgresGreen = 0, byte ProgresBlue = 0, byte ProgresRed = 0)
        {
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                for (byte x = 0; x < count; x++)
                {
                    Database.ItemType.DBItem DbItem;
                    if (ItemsBase.TryGetValue(ID, out DbItem))
                    {
                        Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                        //ItemDat.UID = ITEM_Counter.Next;
                        ItemDat.ITEM_ID = ID;

                        ItemDat.ProgresGreen = ProgresGreen;
                        ItemDat.Enchant = ProgresBlue;
                        ItemDat.Bless = ProgresRed;
                        ItemDat.SocketProgress = (uint)(ProgresGreen | (ProgresBlue << 8) | (ProgresRed << 16));
                        ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                        ItemDat.Plus = plus;
                        ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                        ItemDat.Bound = (byte)(bound ? 1 : 0);
                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }
                        if (x >= count)
                            return true;
                    }
                }
            }
            return false;
        }
        public bool AddSoulItem_GM(ServerSockets.Packet stream, uint ID, uint SoulID, uint soullevel = 6, uint souldays = 14, byte plus = 12, byte gem1 = 12, byte gem2 = 12, byte hp = 255
         , byte daamge = 7, byte times = 1, bool bound = false)
        {
            if (times == 0)
            {
                times = 1;
            }
            if (this.HaveSpace(times))
            {
                byte num = 0;
                while (num < times)
                {
                    Database.ItemType.DBItem item;
                    num = (byte)(num + 1);
                    if (ItemsBase.TryGetValue(ID, out item))
                    {
                        MsgGameItem item2;
                        if (SoulID != 0)
                        {
                            item2 = new MsgGameItem
                            {
                                ITEM_ID = ID,
                                Effect = Flags.ItemEffect.None,
                                Durability = item.Durability,
                                Plus = plus,
                                Bless = daamge,
                                Enchant = 0xff,
                                SocketOne = Role.Flags.Gem.SuperDragonGem,
                                SocketTwo = Role.Flags.Gem.SuperDragonGem,
                                Color = (Role.Flags.Color)GetRandom.Next(3, 9),
                                Bound = bound ? ((byte)1) : ((byte)0),
                                Purification = new MsgItemExtra.Purification()
                            };
                            item2.Purification.AddedOn = DateTime.Now;
                            item2.Purification.ItemUID = item2.UID;
                            item2.Purification.PurificationLevel = soullevel;
                            item2.Purification.PurificationDuration = 0;
                            item2.Purification.PurificationItemID = SoulID;
                            item2.Purification.Typ = MsgItemExtra.Typing.PurificationEffect;
                            MsgItemExtra extra = new MsgItemExtra
                            {
                                Purifications = { item2.Purification }
                            };
                            Owner.Send(extra.CreateArray(stream, false));
                        }
                        else
                        {
                            item2 = new MsgGameItem
                            {
                                ITEM_ID = ID,
                                Effect = Flags.ItemEffect.None,
                                Durability = item.Durability,
                                Plus = plus,
                                Bless = daamge,
                                Enchant = 0xff,
                                SocketOne = Role.Flags.Gem.SuperDragonGem,
                                SocketTwo = Role.Flags.Gem.SuperDragonGem,
                                Color = (Role.Flags.Color)GetRandom.Next(3, 9),
                                Bound = bound ? ((byte)1) : ((byte)0),
                            };
                        }
                        item2.Mode = Role.Flags.ItemMode.AddItem | Role.Flags.ItemMode.Trade;
                        item2.Send(Owner, stream);
                        try
                        {
                            if (!this.Update(item2, AddMode.ADD, stream, false))
                            {
                                return false;
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.SaveException(exception);
                        }
                    }
                }
                if (num >= times)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Add(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, byte bless = 0, byte Enchant = 0
            , Role.Flags.Gem sockone = Flags.Gem.NoSocket
             , Role.Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false, Role.Flags.ItemEffect Effect = Flags.ItemEffect.None, bool SendMessage = false
            , string another_text = "")
        {
            if (global::Core.Features.FeatureRegistry.IsBlockedItem(ID) || Game.Era1.Era1Items.IsBlockedEquipment(ID)) return false; // [feature-gate items]
            if (ID == 1088000)
            {

            }
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                byte x = 0;
                for (; x < count;)
                {
                    x++;
                    Database.ItemType.DBItem DbItem;
                    if (ItemsBase.TryGetValue(ID, out DbItem))
                    {

                        Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                        ItemDat.ITEM_ID = ID;
                        ItemDat.Effect = Effect;
                        ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                        ItemDat.Plus = plus;
                        ItemDat.Bless = bless;
                        ItemDat.Enchant = Enchant;
                        ItemDat.SocketOne = sockone;
                        ItemDat.SocketTwo = socktwo;
                        ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                        ItemDat.Bound = (byte)(bound ? 1 : 0);
                        if (DbItem.TimeItems != 0 && ItemDat.RemainingTime > 0)
                        {
                            ItemDat.Activate = 1;
                            ItemDat.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                        }
                        if (SendMessage)
                        {
                            switch(another_text)
                            {
                                case "~from~mining!":
                                    {
                                        Owner.SendWhisper("You~received~a~" + DbItem.Name + "" + another_text, "MiningSystem", Owner.Player.Name);
                                        break;
                                    }
                                default:
                                    Owner.CreateBoxDialog("You~received~a~" + DbItem.Name + "" + another_text);
                                    break;
                            }
                            
                        }

                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }

                    }
                }
                if (x >= count)
                    return true;
            }

            return false;
        }
        public bool AddMinute(ServerSockets.Packet stream, uint ID, int minute = 0, byte count = 1, byte plus = 0, bool bound = false)
        {
            if (global::Core.Features.FeatureRegistry.IsBlockedItem(ID) || Game.Era1.Era1Items.IsBlockedEquipment(ID)) return false; // [feature-gate items]
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                byte x = 0;
                for (; x < count;)
                {
                    x++;
                    Database.ItemType.DBItem DbItem;
                    if (ItemsBase.TryGetValue(ID, out DbItem))
                    {

                        MsgGameItem ItemDat = new MsgGameItem();
                        ItemDat.ITEM_ID = ID;
                        ItemDat.Plus = plus;
                        ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                        ItemDat.Bound = (byte)(bound ? 1 : 0);
                        if (minute == 0)
                        {
                            if (DbItem.TimeItems != 0 && DbItem.StackSize == 0)
                            {
                                ItemDat.Activate = 1;
                                ItemDat.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                            }
                        }
                        else if (minute != 0)
                        {
                            ItemDat.Activate = 1;
                            ItemDat.EndDate = DateTime.Now.AddMinutes(minute);
                        }
                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }

                    }
                }
                if (x >= count)
                    return true;
            }

            return false;
        }
        public bool AddRefinaryItem(uint ID, bool Bound, ServerSockets.Packet stream)
        {
            ID = ID + Database.ItemType.GetNextRefineryItem();
            if (ID == 724348 || ID == 724349)
                ID += 150;
            if (ID == 724449)
                ID = 724445;

            return Add(stream, ID, 1, 0, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, Bound);
        }
        public bool AddItemWitchStack(uint ID, byte Plus, ushort amount, ServerSockets.Packet stream, bool bound = false, int IDEvent = 0)
        {
            if (global::Core.Features.FeatureRegistry.IsBlockedItem(ID) || Game.Era1.Era1Items.IsBlockedEquipment(ID)) return false; // [feature-gate items]
            //return Add(stream, ID, (byte)amount, Plus, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, bound);
            Database.ItemType.DBItem DbItem;
            bool AllOk = false;
            if (ItemsBase.TryGetValue(ID, out DbItem))
            {
                if (DbItem.StackSize > 0)
                {
                    byte _bound = 0;
                    if (bound)
                        _bound = 1;
                    foreach (var item in ClientItems.Values)
                    {

                        if (item.ITEM_ID == ID && item.Bound == _bound)
                        {
                            if (item.StackSize + amount <= DbItem.StackSize)
                            {
                                item.Mode = Flags.ItemMode.Update;
                                item.StackSize += amount;
                                if (bound)
                                    item.Bound = 1;
                                if (DbItem.TimeItems != 0 && DbItem.StackSize == 1)
                                {
                                    item.Activate = 1;
                                    item.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                                }
                                item.Send(Owner, stream);
                                return true;
                            }
                        }
                    }

                    if (amount > DbItem.StackSize)
                    {
                        if (HaveSpace((byte)((amount / DbItem.StackSize) + (byte)(Owner.OnInterServer ? 1 : 0))))
                        {
                            while (amount >= DbItem.StackSize)
                            {
                                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                                ItemDat.ITEM_ID = ID;
                                ItemDat.IDEvent = IDEvent;
                                ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                                ItemDat.Plus = Plus;
                                ItemDat.StackSize += DbItem.StackSize;
                                ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                                if (DbItem.TimeItems != 0 && DbItem.StackSize == 1)
                                {
                                    ItemDat.Activate = 1;
                                    ItemDat.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                                }
                                if (bound)
                                    ItemDat.Bound = 1;
                                try
                                {
                                    Update(ItemDat, AddMode.ADD, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                                amount -= DbItem.StackSize;

                            }
                            if (amount > 0 && amount < DbItem.StackSize)
                            {
                                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                                ItemDat.ITEM_ID = ID;
                                ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                                ItemDat.Plus = Plus;
                                ItemDat.IDEvent = IDEvent;
                                ItemDat.StackSize += amount;
                                if (DbItem.TimeItems != 0 && DbItem.StackSize == 1)
                                {
                                    ItemDat.Activate = 1;
                                    ItemDat.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                                }
                                ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                                if (bound)
                                    ItemDat.Bound = 1;
                                try
                                {
                                    Update(ItemDat, AddMode.ADD, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                            }
                            return true;
                        }
                        else
                        {
                            while (amount >= DbItem.StackSize)
                            {
                                AddReturnedItem(stream, ID, 1, Plus, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, bound, Flags.ItemEffect.None, DbItem.StackSize);
                                amount -= DbItem.StackSize;
                            }
                            if (amount > 0 && amount < DbItem.StackSize)
                            {
                                AddReturnedItem(stream, ID, 1, Plus, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, bound, Flags.ItemEffect.None, amount);
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (HaveSpace(1))
                        {
                            Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                            ItemDat.ITEM_ID = ID;
                            ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                            ItemDat.Plus = Plus;
                            ItemDat.IDEvent = IDEvent;
                            ItemDat.StackSize = amount;
                            ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                            if (DbItem.TimeItems != 0 && DbItem.StackSize == 1)
                            {
                                ItemDat.Activate = 1;
                                ItemDat.EndDate = DateTime.Now.AddMinutes(DbItem.TimeItems);
                            }
                            if (bound)
                                ItemDat.Bound = 1;
                            try
                            {
                                Update(ItemDat, AddMode.ADD, stream);
                            }
                            catch (Exception e)
                            {
                                Console.SaveException(e);
                            }
                            return true;
                        }
                    }
                }
                for (int count = 0; count < amount; count++)
                    AllOk = Add(ID, Plus, DbItem, stream, bound, IDEvent);
                return AllOk;
            }
            return AllOk;
        }
        public bool ContainItemWithStack(uint UID, ushort Count)
        {
            Game.MsgServer.MsgGameItem ItemDat;
            if (ClientItems.TryGetValue(UID, out ItemDat))
            {
                return ItemDat.StackSize >= Count || Count == 1 && ItemDat.StackSize == 0;
            }
            return false;
        }

        public bool RemoveStackItem(uint UID, ushort Count, ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgGameItem ItemDat;
            if (ClientItems.TryGetValue(UID, out ItemDat))
            {
                if (ItemDat.StackSize > Count)
                {
                    ItemDat.StackSize -= Count;
                    ItemDat.Mode = Flags.ItemMode.Update;
                    ItemDat.Send(Owner, stream);
                }
                else
                {
                    ItemDat.StackSize = 1;
                    Update(ItemDat, AddMode.REMOVE, stream);
                    return true;
                }
            }
            else
            {

                foreach (var item in ClientItems.Values)
                {
                    if (0 == Count)
                        break;
                    if (item.ITEM_ID == UID)
                    {
                        if (item.StackSize > Count)
                        {
                            item.StackSize -= Count;
                            item.Mode = Flags.ItemMode.Update;
                            item.Send(Owner, stream);
                            Count = 0;
                        }
                        else
                        {
                            Count -= item.StackSize;
                            item.StackSize = 1;
                            Update(item, AddMode.REMOVE, stream);
                        }
                    }
                }
            }
            return false;
        }
        public bool AddSoul(uint ID, uint SoulID, byte plus, byte gem1, byte gem2, byte hp, byte daamge, byte times, ServerSockets.Packet stream, bool bound)
        {
            Database.ItemType.DBItem Soul = null;
            Database.ItemType.DBItem ITEMDB = null;
            if (SoulID != 0)
            {
                if (!ItemsBase.TryGetValue((uint)SoulID, out Soul))
                    return false;

            }
            if (!ItemsBase.TryGetValue((uint)ID, out ITEMDB))
                return false;
            if (HaveSpace(1))
            {
                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                //ItemDat.UID = ITEM_Counter.Next;
                ItemDat.ITEM_ID = ID;
                ItemDat.Durability = ItemDat.MaximDurability = ITEMDB.Durability;
                ItemDat.Plus = plus;
                ItemDat.SocketOne = (Flags.Gem)gem1;
                ItemDat.SocketTwo = (Flags.Gem)gem2;
                ItemDat.Bless = daamge;
                if (hp > 0)
                {
                    ItemDat.Enchant = (byte)(new System.Random().Next(200, 255));
                }
                ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                if (ITEMDB.TimeItems != 0 && ITEMDB.StackSize == 1)
                {
                    ItemDat.Activate = 1;
                    ItemDat.EndDate = DateTime.Now.AddMinutes(ITEMDB.TimeItems);
                }
                MsgItemExtra.Purification purify = new MsgItemExtra.Purification();
                purify.AddedOn = DateTime.Now;
                purify.ItemUID = ItemDat.UID;
                purify.PurificationLevel = 6;
                purify.PurificationItemID = SoulID;
                ItemDat.Purification.Typ = MsgItemExtra.Typing.PurificationEffect;
                ItemDat.Purification = purify;
                try
                {
                    Update(ItemDat, AddMode.ADD, stream);
                }
                catch (Exception e)
                {
                    Console.SaveException(e);
                }
                return true;
            }
            return false;

        }
        public bool Add(uint ID, byte Plus, Database.ItemType.DBItem ITEMDB, ServerSockets.Packet stream, bool bound = false, int IDEvent = 0)
        {
            if (ITEMDB.StackSize > 0)
            {
                byte _bound = 0;
                if (bound)
                    _bound = 1;
                foreach (var item in ClientItems.Values)
                {

                    if (item.ITEM_ID == ID && item.Bound == _bound)
                    {
                        if (item.StackSize < ITEMDB.StackSize)
                        {
                            item.Mode = Flags.ItemMode.Update;
                            item.StackSize++;
                            if (ITEMDB.TimeItems != 0)
                            {
                                item.Activate = 1;
                                item.EndDate = DateTime.Now.AddMinutes(ITEMDB.TimeItems);
                            }
                            if (bound)
                                item.Bound = 1;
                            item.Send(Owner, stream);

                            return true;
                        }
                    }
                }
            }
            if (HaveSpace(1))
            {
                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                //ItemDat.UID = ITEM_Counter.Next;
                ItemDat.ITEM_ID = ID;
                ItemDat.Durability = ItemDat.MaximDurability = ITEMDB.Durability;
                ItemDat.Plus = Plus;
                ItemDat.IDEvent = IDEvent;
                ItemDat.Color = (Role.Flags.Color)Pool.GetRandom.Next(3, 9);
                if (ITEMDB.TimeItems != 0)
                {
                    ItemDat.Activate = 1;
                    ItemDat.EndDate = DateTime.Now.AddMinutes(ITEMDB.TimeItems);
                }
                if (bound)
                    ItemDat.Bound = 1;
                try
                {
                    Update(ItemDat, AddMode.ADD, stream);
                }
                catch (Exception e)
                {
                    Console.SaveException(e);
                }
                return true;
            }
            return false;

        }
        public bool Add(Game.MsgServer.MsgGameItem ItemDat, Database.ItemType.DBItem ITEMDB, ServerSockets.Packet stream)
        {
            if (global::Core.Features.FeatureRegistry.IsBlockedItem(ItemDat.ITEM_ID) || Game.Era1.Era1Items.IsBlockedEquipment(ItemDat.ITEM_ID))
                return false;

            if (ITEMDB.StackSize > 0)
            {
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == ItemDat.ITEM_ID)
                    {
                        if (item.StackSize < ITEMDB.StackSize)
                        {
                            item.Mode = Flags.ItemMode.Update;
                            item.StackSize++;
                            item.Send(Owner, stream);
                            return true;
                        }
                    }
                }
            }
            if (HaveSpace(1))
            {
                Update(ItemDat, AddMode.ADD, stream);
                return true;
            }
            return false;

        }
        public bool AddItemWitchStack(Game.MsgServer.MsgGameItem ItemDat, byte amount, ServerSockets.Packet stream)
        {
            Database.ItemType.DBItem DbItem;
            if (ItemsBase.TryGetValue(ItemDat.ITEM_ID, out DbItem))
            {
                for (int count = 0; count < amount; count++)
                    Add(ItemDat, DbItem, stream);
                return true;
            }
            return false;
        }
        public unsafe bool Update(Game.MsgServer.MsgGameItem ItemDat, AddMode mode, ServerSockets.Packet stream, bool Removefull = false)
        {
            if (HaveSpace(1) || mode == AddMode.REMOVE)
            {
                string logs = "[Item]" + Owner.Player.Name + " [" + mode + "] [" + ItemDat.UID + "]" + ItemDat.ITEM_ID + " plus [" + ItemDat.Plus + "] at : " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
                Database.ServerDatabase.LoginQueue.Enqueue(logs);
                switch (mode)
                {
                    case AddMode.ADD:
                        {
                            CheakUp(ItemDat);
                            if (ItemDat.StackSize == 0)
                                ItemDat.StackSize = 1;
                            ItemDat.Position = 0;
                            ItemDat.Mode = Flags.ItemMode.AddItem;
                            ItemDat.Send(Owner, stream);
                            if (Owner.IsConnectedInterServer())
                            {
                                ItemDat.Send(Owner.PipeClient, stream);
                            }
                            break;
                        }
                    case AddMode.MOVE:
                        {
                            CheakUp(ItemDat);
                            if (ItemDat.StackSize == 0)
                                ItemDat.StackSize = 1;
                            ItemDat.Position = 0;
                            ItemDat.Mode = Flags.ItemMode.AddItem;
                            ItemDat.Send(Owner, stream);
                            break;
                        }
                    case AddMode.REMOVE:
                        {
                            if (ItemDat.StackSize > 1 && ItemDat.Position < 40 && !Removefull)
                            {
                                ItemDat.StackSize -= 1;
                                ItemDat.Mode = Flags.ItemMode.Update;
                                ItemDat.Send(Owner, stream);
                                break;
                            }
                            Game.MsgServer.MsgGameItem item;
                            if (ClientItems.TryRemove(ItemDat.UID, out item))
                            {
                                Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.RemoveInventory, ItemDat.UID, 0, 0, 0, 0, 0));
                            }
                            else Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.RemoveInventory, ItemDat.UID, 0, 0, 0, 0, 0));
                            break;
                        }
                }
                if (mode == AddMode.ADD || mode == AddMode.REMOVE)
                {
                    var era1Resources = Game.Era1.Era1Economy.TrackedResources(ItemDat.ITEM_ID, ItemDat.Plus);
                    if (era1Resources.Length != 0)
                    {
                        long units = mode == AddMode.REMOVE && !Removefull ? 1 : (ItemDat.StackSize > 1 ? ItemDat.StackSize : 1);
                        units *= Game.Era1.Era1Economy.TrackedResourceUnitMultiplier(ItemDat.ITEM_ID);
                        for (int resourceIndex = 0; resourceIndex < era1Resources.Length; resourceIndex++)
                            Telemetry.Economy.RecordResource(Owner.Player.UID, Owner.Player.Name, Owner.Player.Map,
                                era1Resources[resourceIndex], mode == AddMode.ADD ? units : -units);
                    }
                }

                if (ItemDat.ITEM_ID == 750000)
                {
                    Owner.DemonExterminator.ItemUID = ItemDat.UID;
                    if (mode == AddMode.REMOVE)
                        Owner.DemonExterminator.ItemUID = 0;
                }
                /* try
                 {
                     StackTrace stackTrace = new StackTrace();           // get call stack
                     StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                     string data = "[CallStack]" + Owner.Player.Name + " " + ItemDat.ITEM_ID + " "+ItemDat.UID+" \n";
                     // write call stack method names
                     foreach (StackFrame stackFrame in stackFrames)
                     {
                         data += stackFrame.GetMethod().Name + " " + stackFrame.GetMethod().DeclaringType.Name + " ";   
                  
                     }

                     data += Environment.StackTrace;

                     Database.ServerDatabase.LoginQueue.Enqueue(data);
                  
                 }
                 catch (Exception e)
                 {
                     MyConsole.SaveException(e);
                 }*/

                return true;

            }
            return false;
        }
        private void CheakUp(Game.MsgServer.MsgGameItem ItemDat)
        {
            if (ItemDat.UID == 0) return;
                //ItemDat.UID = ITEM_Counter.Next;
            if (!ClientItems.TryAdd(ItemDat.UID, ItemDat))
            {
                //do
                //    ItemDat.UID = ITEM_Counter.Next;
                //while
                //  (ClientItems.TryAdd(ItemDat.UID, ItemDat) == false);
            }
        }

        public bool CheckMeteors(byte count, bool Removethat, ServerSockets.Packet stream)
        {

            if (Contain(1088001, count))
            {
                if (Removethat)
                    Remove(1088001, count, stream);
                return true;
            }
            else
            {
                byte Counter = 0;
                var RemoveThis = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
                var MyMetscrolls = GetMyMetscrolls();
                var MyMeteors = GetMyMeteors();
                foreach (var GameItem in MyMetscrolls.Values)
                {
                    Counter += 10;
                    RemoveThis.Add(GameItem.UID, GameItem);
                    if (Counter >= count)
                        break;
                }
                if (Counter >= count)
                {
                    byte needSpace = (byte)(Counter - count);
                    if (HaveSpace(needSpace))
                    {
                        if (Removethat)
                        {
                            Add(stream, 1088001, needSpace);
                        }
                    }
                    else
                    {
                        Counter -= 10;
                        RemoveThis.Remove(RemoveThis.Values.First().UID);
                        byte needmetsss = (byte)(count - Counter);
                        if (needmetsss <= MyMeteors.Count)
                        {
                            foreach (var GameItem in MyMeteors.Values)
                            {
                                Counter += 1;
                                RemoveThis.Add(GameItem.UID, GameItem);
                                if (Counter >= count)
                                    break;
                            }
                            if (Removethat)
                            {
                                foreach (var GameItem in RemoveThis.Values)
                                    Update(GameItem, AddMode.REMOVE, stream);
                            }
                        }
                        else
                            return false;
                    }
                    if (Removethat)
                    {
                        foreach (var GameItem in RemoveThis.Values)
                            Update(GameItem, AddMode.REMOVE, stream);
                    }
                    return true;
                }
                foreach (var GameItem in MyMeteors.Values)
                {
                    Counter += 1;
                    RemoveThis.Add(GameItem.UID, GameItem);
                    if (Counter >= count)
                        break;
                }
                if (Counter >= count)
                {
                    if (Removethat)
                    {
                        foreach (var GameItem in RemoveThis.Values)
                            Update(GameItem, AddMode.REMOVE, stream);
                    }
                    return true;
                }
            }

            return false;
        }
        private Dictionary<uint, Game.MsgServer.MsgGameItem> GetMyMetscrolls()
        {
            var array = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
            foreach (var GameItem in ClientItems.Values)
            {
                if (GameItem.ITEM_ID == 720027)
                {
                    if (!array.ContainsKey(GameItem.UID))
                        array.Add(GameItem.UID, GameItem);
                }
            }
            return array;
        }
        private Dictionary<uint, Game.MsgServer.MsgGameItem> GetMyMeteors()
        {
            var array = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
            foreach (var GameItem in ClientItems.Values)
            {
                if (GameItem.ITEM_ID == Database.ItemType.Meteor || GameItem.ITEM_ID == Database.ItemType.MeteorTear)
                {
                    if (!array.ContainsKey(GameItem.UID))
                        array.Add(GameItem.UID, GameItem);
                }
            }
            return array;
        }


        public void ShowALL(ServerSockets.Packet stream)
        {
            foreach (var msg_item in ClientItems.Values)
            {
                msg_item.Mode = Flags.ItemMode.AddItem;
                msg_item.Send(Owner, stream);
            }
        }
        public void Clear(ServerSockets.Packet stream)
        {
            var dictionary = ClientItems.Values.ToArray();
            foreach (var msg_item in dictionary)
                Update(msg_item, AddMode.REMOVE, stream, true);
        }
    }
}
