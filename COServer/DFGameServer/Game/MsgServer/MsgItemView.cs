namespace GameServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet ItemViewCreate(this ServerSockets.Packet stream, uint PlayerUID, uint Cost, MsgGameItem DateItem, MsgItemView.ActionMode Mode)
        {
            stream.InitWriter();
            stream.Write(DateItem.UID);
            stream.Write(PlayerUID);
            stream.Write(Cost);
            stream.Write(DateItem.ITEM_ID);
            stream.Write(DateItem.Durability);
            stream.Write(DateItem.MaximDurability);
            stream.Write((ushort)Mode);
            stream.Write(DateItem.Position);
            stream.Write(DateItem.SocketProgress);
            stream.Write((byte)DateItem.SocketOne);
            stream.Write((byte)DateItem.SocketTwo);
            stream.Write((ushort)0);
            stream.Write((uint)DateItem.Effect);
            stream.Write((byte)0);
            stream.Write(DateItem.Plus);
            stream.Write(DateItem.Bless);
            stream.Write(DateItem.Bound);
            stream.Write(DateItem.Enchant);
            stream.Write((uint)DateItem.ProgresGreen);//46
            stream.Write((uint)0);
            stream.Write(DateItem.Suspicious);//50
            stream.Write((ushort)DateItem.Locked);
            stream.Write((uint)DateItem.Color);
            stream.Write(DateItem.PlusProgress);
            stream.Write(DateItem.Inscribed);
            Database.ItemType.DBItem DBItem;
            if (Pool.ItemsBase.TryGetValue(DateItem.ITEM_ID, out DBItem))
            {
                if (DBItem.TimeItems != 0 && DateItem.Activate == 0)
                {
                    stream.Write(DateItem.Activate);//53 //active 
                }
                else
                {
                    if (DateItem.Activate == 1 && DateItem.RemainingTime > 0 && DateItem.RemainingTime < uint.MaxValue)
                    {

                        stream.Write((uint)(DateItem.RemainingTime));//active 
                    }
                    else
                        stream.Write(DateItem.TimeLeftInMinutes);//active 
                    stream.Write(0);
                }
            }
            stream.Write(DateItem.StackSize);
            stream.Write(DateItem.Purification.PurificationItemID);
            stream.Write(DateItem.Purification.PurificationItemID);
            stream.Finalize(GamePackets.ItemView);
            return stream;
        }

    }

    public class MsgItemView
    {
        public enum ActionMode : ushort
        {
            Gold = 1,
            CPs = 3,
            ViewEquip = 4
        }
    }
}
