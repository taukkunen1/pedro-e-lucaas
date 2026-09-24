using Core;
using GameServer.Game.MsgServer;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Role.Instance
{
    public class Vendor
    {
        public static Counter VendorCounter = new Counter(100000);

        public const byte MaxItems = 20;
        
        public class VendorItem
        {
            public Game.MsgServer.MsgItemView.ActionMode CostType;
            public Game.MsgServer.MsgGameItem DataItem;
            public uint AmountCost;
            public void Regenerate(VendorItem item, Vendor booth)
            {
                booth.Items.TryRemove(item.DataItem.UID, out item);
                this.AmountCost = item.AmountCost;
                this.DataItem = new MsgGameItem();
                this.DataItem.ITEM_ID = item.DataItem.ITEM_ID;
                //this.DataItem.UID = Pool.ITEM_Counter.Next;
                this.DataItem.Plus = item.DataItem.Plus;
                this.DataItem.Enchant = item.DataItem.Enchant;
                this.DataItem.Bless = item.DataItem.Bless;
                this.DataItem.SocketOne = item.DataItem.SocketOne;
                this.DataItem.SocketTwo = item.DataItem.SocketTwo;
                this.DataItem.StackSize = item.DataItem.StackSize;
                Database.ItemType.DBItem CIBI;
                if (Pool.ItemsBase.TryGetValue(this.DataItem.ITEM_ID, out CIBI))
                {
                    this.DataItem.Durability = CIBI.Durability;
                    this.DataItem.MaximDurability = CIBI.Durability;
                    this.CostType = item.CostType;
                    booth.Items.TryAdd(this.DataItem.UID, this);
                }
            }
        }
        public static Dictionary<uint, Vendor> Booths = new Dictionary<uint, Vendor>();
        public Client.GameClient Owner;
        public ConcurrentDictionary<uint, VendorItem> Items;
        public Game.MsgServer.MsgMessage HalkMeesaje = null;
        public SobNpc VendorNpc;
        public uint VendorUID;

        public bool InVending;
        public Vendor()
        {
            Items = new ConcurrentDictionary<uint, VendorItem>();
        }
        public static bool TryGetValue(uint uid, out Vendor booth)
        {
            return Booths.TryGetValue(uid, out booth);
        }
        public Vendor(Client.GameClient client)
        {
            Items = new ConcurrentDictionary<uint, VendorItem>();
            Owner = client;
        }
        public unsafe void CreateVendor(ServerSockets.Packet stream)
        {
            if (InVending) return;
            if (!Game.Era1.Era1Services.CanCreatePlayerBooth(Owner))
            {
                Owner?.SendSysMesage("Vending is only available in the classic Market.");
                return;
            }

            VendorUID = VendorCounter.Next;

            VendorNpc = new SobNpc();
            VendorNpc.ObjType = MapObjectType.SobNpc;
            VendorNpc.OwnerVendor = Owner;
            VendorNpc.Name = Owner.Player.Name;
            VendorNpc.UID = VendorUID;
            VendorNpc.Mesh = SobNpc.StaticMesh.Vendor;
            VendorNpc.Type = Flags.NpcType.Booth;
            VendorNpc.Map = Owner.Player.Map;
            VendorNpc.X = (ushort)(Owner.Player.X + 1);
            VendorNpc.Y = Owner.Player.Y;

            Owner.Map.View.EnterMap<Role.IMapObj>(VendorNpc);

            foreach (var IObj in Owner.Player.View.Roles(MapObjectType.Player))
            {
                Role.Player screenObj = IObj as Role.Player;
                screenObj.View.CanAdd(VendorNpc, true, stream);
            }
            Owner.Player.Send(VendorNpc.GetArray(stream, false));
            InVending = true;
        }
        public unsafe void StopVending(ServerSockets.Packet stream)
        {
            if (InVending)
            {

                ActionQuery actione = new ActionQuery()
                {
                    ObjId = VendorUID,
                    Type = ActionType.RemoveEntity
                };
                Owner.Player.View.SendView(stream.ActionCreate(&actione), true);

                Items.Clear();
                Owner.Map.View.LeaveMap<Role.IMapObj>(VendorNpc);
                InVending = false;
                Owner.MyVendor = null;
            }
        }
        public bool AddItem(Game.MsgServer.MsgGameItem DataItem, Game.MsgServer.MsgItemView.ActionMode CostType, uint Amout)
        {
            if (DataItem == null
                || !Game.Era1.Era1Services.IsAllowedPlayerVendingItem(DataItem.ITEM_ID)
                || !Game.Era1.Era1Services.IsValidVendingPrice(Amout)
                || DataItem.Bound == 1 || DataItem.Inscribed == 1 || DataItem.Locked != 0)
                return false;

            if (Items.Count == MaxItems)
                return false;
            if (!Items.ContainsKey(DataItem.UID))
            {
                VendorItem VItem = new VendorItem();
                VItem.DataItem = DataItem;
                VItem.CostType = CostType;
                VItem.AmountCost = Amout;
                Items.TryAdd(DataItem.UID, VItem);
                return true;
            }
            return false;
        }
    }
}
