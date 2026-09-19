using System;

namespace GameServer.Database
{
    public class ClientItems
    {
        public struct DBItem
        {
            public uint UID;
            public uint ITEM_ID;
            public ushort Durability;
            public ushort MaximDurability;
            public ushort Position;
            public uint SocketProgress;
            public Role.Flags.Gem SocketOne;
            public Role.Flags.Gem SocketTwo;
            public Role.Flags.ItemEffect Effect;
            public byte Plus;
            public byte Bless;
            public byte Bound;
            public byte Enchant;
            public byte Suspicious;
            public byte Locked;
            
            public uint PlusProgress;
            public uint Inscribed;
            public uint Activate;
            public uint TimeLeftInMinutes;
            public ushort StackSize;
            public uint WH_ID;
            public Role.Flags.Color Color;
            public int IDEvent;
            public bool Fake;
            public int UnLockTimer;
            public uint RemainingTime;

            //Artefacts 
            public uint PurificationItemID;
            public uint PurificationLevel;
            public uint PurificationDuration;
            public long PurificationAddedOn;

            //Refinary
            public uint EffectID;
            public uint EffectLevel;
            public uint EffectPercent;
            public uint EffectPercent2;
            public uint EffectDuration;
            public long EffectAddedOn;

            // Expiration
            public long Expiration;

            public uint DepositeCount;

            public DBItem GetDBItem(Game.MsgServer.MsgGameItem DataItem)
            {
                UID = DataItem.UID;
                ITEM_ID = DataItem.ITEM_ID;
                Durability = DataItem.Durability;
                MaximDurability = DataItem.MaximDurability;
                Position = DataItem.Position;
                SocketProgress = DataItem.SocketProgress;
                SocketOne = DataItem.SocketOne;
                SocketTwo = DataItem.SocketTwo;
                Effect = DataItem.Effect;
                Plus = DataItem.Plus;
                Bless = DataItem.Bless;
                Bound = DataItem.Bound;
                Enchant = DataItem.Enchant;
                Suspicious = DataItem.Suspicious;
                Locked = DataItem.Locked;
                PlusProgress = DataItem.PlusProgress;
                Inscribed = DataItem.Inscribed;
                Activate = DataItem.Activate;
                TimeLeftInMinutes = DataItem.TimeLeftInMinutes;
                StackSize = DataItem.StackSize;
                WH_ID = DataItem.WH_ID;
                Color = DataItem.Color;
                IDEvent = DataItem.IDEvent;
                PurificationItemID = DataItem.Purification.PurificationItemID;
                PurificationAddedOn = DataItem.Purification.AddedOn.Ticks;
                PurificationDuration = DataItem.Purification.PurificationDuration;
                PurificationLevel = DataItem.Purification.PurificationLevel;
                EffectAddedOn = DataItem.Refinary.AddedOn.Ticks;
                EffectDuration = DataItem.Refinary.EffectDuration;
                EffectID = DataItem.Refinary.EffectID;
                EffectLevel = DataItem.Refinary.EffectLevel;
                EffectPercent = DataItem.Refinary.EffectPercent;
                EffectPercent2 = DataItem.Refinary.EffectPercent2;
                Fake = DataItem.Fake;
                UnLockTimer = DataItem.UnLockTimer;
                RemainingTime = DataItem.RemainingTime;
                Expiration = DataItem.EndDate == DateTime.FromBinary(0) ? 0 : DataItem.EndDate.ToBinary();
                return this;
            }

            public Game.MsgServer.MsgGameItem GetDataItem()
            {
                Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
                DataItem.UID = UID;
                DataItem.ITEM_ID = ITEM_ID;
                DataItem.Durability = Durability;
                DataItem.MaximDurability = MaximDurability;
                DataItem.Position = Position;
                DataItem.SocketProgress = SocketProgress;
                DataItem.SocketOne = SocketOne;
                DataItem.SocketTwo = SocketTwo;
                DataItem.Effect = Effect;
                DataItem.Plus = Plus;
                DataItem.Bless = Bless;
                DataItem.Bound = Bound;
                DataItem.Enchant = Enchant;
                DataItem.Suspicious = Suspicious;
                DataItem.Locked = Locked;
                DataItem.PlusProgress = PlusProgress;
                DataItem.Inscribed = Inscribed;
                DataItem.Activate = Activate;
                DataItem.TimeLeftInMinutes = TimeLeftInMinutes;
                DataItem.StackSize = StackSize;
                DataItem.IDEvent = IDEvent;
                DataItem.Fake = Fake;
                DataItem.UnLockTimer = UnLockTimer;
                DataItem.EndDate = DateTime.FromBinary(Expiration);
                DataItem.WH_ID = WH_ID;
                DataItem.Color = Color;
                DataItem.Purification.ItemUID = UID;
                DataItem.Purification.PurificationItemID = PurificationItemID;
                DataItem.Purification.PurificationLevel = PurificationLevel;
                DataItem.Purification.PurificationDuration = PurificationDuration;
                try
                {
                    DataItem.Purification.AddedOn = DateTime.FromBinary(PurificationAddedOn);
                }
                catch
                {
                    DataItem.Purification.AddedOn = DateTime.FromBinary(0);
                }
                if (!DataItem.Purification.InLife)
                    DataItem.Purification = new Game.MsgServer.MsgItemExtra.Purification();

                DataItem.Refinary.ItemUID = UID;
                bool failed = false;
                if (EffectID != 0)
                {
                    Database.Refinery.Item BaseAddingItem;
                    if (Pool.RefineryItems.TryGetValue(EffectID, out BaseAddingItem))
                    {
                        if (Database.ItemType.ItemPosition(ITEM_ID) != BaseAddingItem.ForItemPosition && BaseAddingItem.Name != "(Heavy)Ring")
                            failed = true;
                        if (BaseAddingItem.Name == "(Heavy)Ring")
                        {
                            if (ITEM_ID / 1000 != 151)
                                failed = true;
                        }

                        if (BaseAddingItem.Name == "2-Handed" || BaseAddingItem.Name == "2-Handed")
                        {
                            if (!Database.ItemType.IsTwoHand(ITEM_ID))
                                failed = true;
                        }
                        if (BaseAddingItem.Name == "Bow" || BaseAddingItem.Name == "Bow")
                        {
                            if (!Database.ItemType.IsBow(ITEM_ID))
                                failed = true;
                        }
                        if (BaseAddingItem.Name == "Bracelet" || BaseAddingItem.Name == "Bracelet")
                        {
                            if (!Database.ItemType.IsBraclet(ITEM_ID))
                                failed = true;
                        }
                        if (BaseAddingItem.Name == "Ring" || BaseAddingItem.Name == "Ring")
                        {
                            if (!Database.ItemType.IsRing(ITEM_ID) && !Database.ItemType.IsHeavyRing(ITEM_ID))
                                failed = true;
                        }
                    }
                    if (!failed)
                    {
                        DataItem.Refinary.EffectID = EffectID;
                        DataItem.Refinary.EffectLevel = EffectLevel;
                        DataItem.Refinary.EffectPercent = EffectPercent;
                        DataItem.Refinary.EffectPercent2 = EffectPercent2;
                        DataItem.Refinary.EffectDuration = EffectDuration;
                    }
                }
                try
                {
                    DataItem.Refinary.AddedOn = DateTime.FromBinary(EffectAddedOn);
                }
                catch
                {
                    DataItem.Refinary.AddedOn = DateTime.FromBinary(0);
                }
                if (!DataItem.Refinary.InLife)
                    DataItem.Refinary = new Game.MsgServer.MsgItemExtra.Refinery();
                return DataItem;
            }
        }
    }
}