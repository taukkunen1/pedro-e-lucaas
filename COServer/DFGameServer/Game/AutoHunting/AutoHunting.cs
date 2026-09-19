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
            X = 0;
            Y = 0;
            AttackStamp = DateTime.Now;
            Angle = (Role.Flags.ConquerAngle)Pool.GetRandom.Next(0, 7);
        }
    }
}
