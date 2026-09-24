using System;

namespace GameServer
{
    public class AutoHunting
    {
        public string SocketedItemsStatus => this.SocketedItems ? "[Enabled]" : "[Disabled]";
        public string MaterialItemsStatus => this.MaterialItems ? "[Enabled]" : "[Disabled]";
        public string QualityItemsStatus => this.QualityItems ? "[Enabled]" : "[Disabled]";
        public string BlessedItemsStatus => this.BlessedItems ? "[Enabled]" : "[Disabled]";
        public string DBallsStatus => this.DBalls ? "[Enabled]" : "[Disabled]";
        public string MeteorsStatus => this.Meteors ? "[Enabled]" : "[Disabled]";
        public string PlusItemsStatus => this.PlusItems ? "[Enabled]" : "[Disabled]";
        public string SoulItemsStatus => this.SoulItems ? "[Enabled]" : "[Disabled]";
        public string ExpBallEventItemsStatus => this.ExpBallEventItems ? "[Enabled]" : "[Disabled]";
        public string LootMoneyStatus => this.LootMoney ? "[Enabled]" : "[Disabled]";
        public string Status => this.Enable ? "[Enabled]" : "[Disabled]";

        // Hunting zone is anchored when Auto Hunt starts. Radius 0 means unlimited.
        public ushort OriginX;
        public ushort OriginY;
        public ushort HuntRadius = 0;
        public bool UseSkills = true;
        public byte HpPotionPercent = 40;
        public byte MpPotionPercent = 30;
        public AutoHuntExpDelivery ExpDeliveryMode = AutoHuntExpDelivery.OnStop;
        public ulong PendingExperience = 0;

        public enum AutoHuntExpDelivery : byte
        {
            OnStop = 0,
            Instant = 1
        }

        public static ushort NormalizeRadius(ushort radius)
        {
            return radius == 10 || radius == 20 || radius == 30 ? radius : (ushort)0;
        }

        public void AddPendingExperience(ulong experience)
        {
            if (experience == 0)
                return;
            checked { PendingExperience += experience; }
        }

        public ulong TakePendingExperience()
        {
            ulong value = PendingExperience;
            PendingExperience = 0;
            return value;
        }

        public bool IsInsideHuntRadius(ushort x, ushort y)
        {
            return HuntRadius == 0 || Role.Core.GetDistance(OriginX, OriginY, x, y) <= HuntRadius;
        }

        // Prefer explicitly selected valuable loot, then selected equipment/materials,
        // then Silver. Distance is the tie-breaker inside the same priority class.
        public int GetAutoPickUpPriority(Game.MsgFloorItem.MsgItem floorItem)
        {
            if (floorItem == null)
                return int.MaxValue;
            if (floorItem.Typ == Game.MsgFloorItem.MsgItem.ItemType.Money)
                return 300;
            if (floorItem.Typ != Game.MsgFloorItem.MsgItem.ItemType.Item || floorItem.ItemBase == null)
                return int.MaxValue;

            uint id = floorItem.ItemBase.ITEM_ID;
            if (id == Database.ItemType.DragonBall || id == Database.ItemType.DragonBallScroll)
                return 0;
            if (id == Database.ItemType.Meteor || id == Database.ItemType.MeteorTear || id == Database.ItemType.MeteorScroll)
                return 10;
            if (floorItem.ItemBase.Plus > 0 || floorItem.ItemBase.SocketOne != Role.Flags.Gem.NoSocket ||
                floorItem.ItemBase.SocketTwo != Role.Flags.Gem.NoSocket || floorItem.ItemBase.Bless > 0)
                return 20;
            if (Database.ItemType.ItemPosition(id) != 0 && id % 10 >= 7)
                return 30;
            if (id == Database.ItemType.ExpBall || id == Database.ItemType.ExpBall2 || id == Database.ItemType.PowerExpBall)
                return 40;
            return 100;
        }

        // Official Auto Hunt privilege thresholds.
        // Basic Auto Hunt: everyone; Auto Jump: VIP 3+; Auto Pick Up: VIP 4+.
        public const byte AutoJumpVipLevel = 3;
        public const byte AutoPickUpVipLevel = 4;

        public static bool CanAutoHunt(byte vipLevel) => true;
        public static bool CanAutoJump(byte vipLevel) => vipLevel >= AutoJumpVipLevel;
        public static bool CanAutoPickUp(byte vipLevel) => vipLevel >= AutoPickUpVipLevel;

        public string RadiusLabel => HuntRadius == 0 ? "Unlimited" : HuntRadius.ToString();
        public string ExpDeliveryLabel => ExpDeliveryMode == AutoHuntExpDelivery.Instant ? "Instant" : "On Stop";
        public string SkillsStatus => UseSkills ? "[Enabled]" : "[Disabled]";
        public string FastModeStatus => FastMode ? "[Enabled]" : "[Disabled]";

        public void CycleRadius()
        {
            switch (HuntRadius)
            {
                case 0: HuntRadius = 10; break;
                case 10: HuntRadius = 20; break;
                case 20: HuntRadius = 30; break;
                default: HuntRadius = 0; break;
            }
        }

        public void ToggleExpDelivery()
        {
            ExpDeliveryMode = ExpDeliveryMode == AutoHuntExpDelivery.OnStop
                ? AutoHuntExpDelivery.Instant
                : AutoHuntExpDelivery.OnStop;
        }

        public bool ShouldAutoPickUp(Game.MsgFloorItem.MsgItem floorItem)
        {
            if (floorItem == null)
                return false;

            if (floorItem.Typ == Game.MsgFloorItem.MsgItem.ItemType.Money)
                return LootMoney;

            if (floorItem.Typ != Game.MsgFloorItem.MsgItem.ItemType.Item || floorItem.ItemBase == null)
                return false;

            var item = floorItem.ItemBase;
            uint id = item.ITEM_ID;

            if (DBalls && (id == Database.ItemType.DragonBall || id == Database.ItemType.DragonBallScroll))
                return true;
            if (Meteors && (id == Database.ItemType.Meteor || id == Database.ItemType.MeteorTear || id == Database.ItemType.MeteorScroll))
                return true;
            if (PlusItems && item.Plus > 0)
                return true;
            if (QualityItems && Database.ItemType.ItemPosition(id) != 0 && id % 10 >= 7)
                return true;
            if (SocketedItems && (item.SocketOne != Role.Flags.Gem.NoSocket || item.SocketTwo != Role.Flags.Gem.NoSocket))
                return true;
            if (BlessedItems && item.Bless > 0)
                return true;
            if (MaterialItems && (id == Database.ItemType.EuxeniteOre || id == Database.ItemType.Emerald || (id >= 1072000 && id < 1073000)))
                return true;
            if (SoulItems && Database.ItemType.GetSoulPosition(id) != Role.Flags.SoulTyp.None)
                return true;
            if (ExpBallEventItems && (id == Database.ItemType.ExpBall || id == Database.ItemType.ExpBall2 || id == Database.ItemType.PowerExpBall))
                return true;

            return false;
        }

        public bool FastMode = false;
        public bool Enable = false;
        public bool DBalls = false;
        public bool Meteors = false;
        public bool PlusItems = false;
        public bool QualityItems = false;
        public bool ExpBallEventItems = false;
        public bool SocketedItems = false;
        public bool BlessedItems = false;
        public bool MaterialItems = false;
        public bool SoulItems = false;
        public bool LootMoney = false;
        public byte Mytitle = 0;
        public ushort DirectionChange;
        public ushort X;
        public ushort Y;
        public DateTime AttackStamp = DateTime.Now;
        public Role.Flags.ConquerAngle Angle;
        public AutoHunting()
        {
            Mytitle = 0;
            Enable = false;
            DBalls = false;
            Meteors = false;
            PlusItems = false;
            QualityItems = false;
            ExpBallEventItems = false;
            SocketedItems = false;
            BlessedItems = false;
            MaterialItems = false;
            SoulItems = false;
            LootMoney = false;
            DirectionChange = 0;
            OriginX = 0;
            OriginY = 0;
            HuntRadius = 0;
            UseSkills = true;
            HpPotionPercent = 40;
            MpPotionPercent = 30;
            ExpDeliveryMode = AutoHuntExpDelivery.OnStop;
            PendingExperience = 0;
            X = 0;
            Y = 0;
            AttackStamp = DateTime.Now;
            Angle = (Role.Flags.ConquerAngle)Pool.GetRandom.Next(0, 7);
        }
    }
}
