using Core.Interfaces.GameServer;
using System.Collections.Generic;

namespace Core.Models.GameServer
{
    public class PlayerItem : IPlayerItem
    {
        public uint Uid { get; set; }
        public uint EntityID { get; set; }
        public uint ItemId { get; set; }
        public ushort Durability { get; set; }
        public ushort MaxDurability { get; set; }
        public ushort Position { get; set; }
        public uint SocketProgress { get; set; }
        public ushort SocketOne { get; set; }
        public ushort SocketTwo { get; set; }
        public ushort Effect { get; set; }
        public byte Plus { get; set; }
        public byte Bless { get; set; }
        public byte Bound { get; set; }
        public byte Enchant { get; set; }
        public byte Suspicious { get; set; }
        public byte Locked { get; set; }
        public uint PlusProgress { get; set; }
        public uint Inscribed { get; set; }
        public uint Activate { get; set; }
        public uint TimeLeftInMinutes { get; set; }
        public ushort StackSize { get; set; }
        public uint WarehouseId { get; set; }
        public ushort Color { get; set; }
        public int IDEvent { get; set; }
        public bool Fake { get; set; }
        public int UnlockTimer { get; set; }
        public uint ItemPoints { get; set; }
        public uint RemainingTime { get; set; }
        public uint PurificationItemID { get; set; }
        public uint PurificationLevel { get; set; }
        public uint PurificationDuration { get; set; }
        public long PurificationAddedOn { get; set; }
        public uint EffectID { get; set; }
        public uint EffectLevel { get; set; }
        public uint EffectPercent { get; set; }
        public uint EffectPercent2 { get; set; }
        public uint EffectDuration { get; set; }
        public long EffectAddedOn { get; set; }
        public long Expiration { get; set; }
        public uint DepositeCount { get; set; }
        public PlayerItem()
        {

        }
        public PlayerItem(dynamic DataItem, uint EntityID)
        {
            Uid = DataItem.UID;
            this.EntityID = EntityID;
            ItemId = DataItem.ITEM_ID;
            Durability = DataItem.Durability;
            MaxDurability = DataItem.MaximDurability;
            Position = DataItem.Position;
            SocketProgress = DataItem.SocketProgress;
            SocketOne = (ushort)DataItem.SocketOne;
            SocketTwo = (ushort)DataItem.SocketTwo;
            Effect = (ushort)DataItem.Effect;
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
            WarehouseId = DataItem.WH_ID;
            Color = (ushort)DataItem.Color;
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
            UnlockTimer = DataItem.UnLockTimer;
            RemainingTime = DataItem.RemainingTime;
        }
    }

    public class UpdatePlayerItem
    {
        public List<PlayerItem> PlayerItems { get; set; }
        public uint EntityUid { get; set; }
    }
}
