using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.MsgServer
{
    public static class AgateEx
    {
        public enum AgateActions
        {
            Record = 1,
            Recall = 3,
            Repair = 4
        }
        public static unsafe void GetAgate(this ServerSockets.Packet stream, out AgateActions action, out uint UID, out sbyte Map)
        {
            action = (AgateActions)stream.ReadUInt32();
            UID = stream.ReadUInt32();
            Map = stream.ReadInt8();
        }
    }
    public class MsgMemoryAgate
    {

        public static readonly System.Collections.Generic.List<uint> revnomap = new System.Collections.Generic.List<uint> {
            1, 2, 3, 0x80c, 0x1b61, 0x80c, 0x79e, 0x3ed, 0x1b5d, 0x1b5e, 0x1b60, 0x1770, 0x1774, 0x1771, 0x1772, 0x1773,
            0x734, 0x1b59, 0x709, 0x5e4, 0x5ee, 0x1e61, 0x22ad, 0xd05, 0x442, 0x4c9, 1860, 700, 3073, 2064, 1038
         };
        public static List<uint> BlockedMaps = new List<uint>() { 2071, 1005, 1036, 2056 };
        [PacketAttribute(GamePackets.MemoryAgate)]
        public static unsafe void MemoryAgateHandler(Client.GameClient client, ServerSockets.Packet stream)
        {
            AgateEx.AgateActions action;
            uint ItemUID;
            sbyte MapKey;

            stream.GetAgate(out action, out ItemUID, out MapKey);
            switch (action)
            {
                case AgateEx.AgateActions.Record:
                    {
                        if (!client.Player.Alive) return;
                        if (client.Player.DeadState) return;
                        if (client.Player.DynamicID != 0) return;
                        MsgGameItem Item = null;
                        if (revnomap.Contains(client.Player.Map))
                            return;
                        if (client.Player.Map == 1210 || client.Player.Map == 1700)
                            return;
                        if (client.Inventory.TryGetItem(ItemUID, out Item))
                        {
                            if (Item.Agate_map.ContainsKey((uint)MapKey))
                            {
                                if (BlockedMaps.Contains(client.Player.Map))
                                    return;
                                Item.Agate_map[(uint)MapKey] = client.Player.Map
                                   + "~" + client.Player.X
                                   + "~" + client.Player.Y;
                                //Database.ConquerItemTable.UpdateItemAgate(Item);
                                Item.SendAgate(client);
                                break;
                            }
                            if (MapKey > Item.Agate_map.Count)
                            {
                                Item.Agate_map.Add((byte)(Item.Agate_map.Count), client.Player.Map
                                   + "~" + client.Player.X
                                   + "~" + client.Player.Y);
                                //  Database.ConquerItemTable.UpdateItemAgate(Item);
                                Item.SendAgate(client);
                                break;
                            }
                            else
                            {
                                if (!Item.Agate_map.ContainsKey((uint)MapKey))
                                {

                                    if (BlockedMaps.Contains(client.Player.Map))
                                        return;
                                    Item.Agate_map.Add((uint)MapKey, client.Player.Map
                                        + "~" + client.Player.X

                                       + "~" + client.Player.Y);
                                    //Database.ConquerItemTable.UpdateItemAgate(Item);
                                    Item.SendAgate(client);
                                }
                                break;
                            }
                        }
                        break;
                    }
                case AgateEx.AgateActions.Recall:
                    {
                        if (!client.Player.Alive) return;
                        if (client.Player.DeadState) return;
                        if (client.Player.DynamicID != 0) return;
                        MsgGameItem Item = null;
                        if (revnomap.Contains(client.Player.Map))
                            return;
                        if (client.Player.Map == 1210 || client.Player.Map == 1700) return;
                        if (client.Inventory.TryGetItem(ItemUID, out Item))
                        {
                            if (Item.Agate_map.ContainsKey((uint)MapKey))
                            {
                                if (ushort.Parse(Item.Agate_map[(uint)MapKey].Split('~')[0].ToString()) == 1038)
                                    return;
                                if (ushort.Parse(Item.Agate_map[(uint)MapKey].Split('~')[0].ToString()) == 6001)
                                    return;
                                var map = ushort.Parse(Item.Agate_map[(uint)MapKey].Split('~')[0].ToString());
                                if (BlockedMaps.Contains(map))
                                    return;
                                client.Teleport(ushort.Parse(Item.Agate_map[(uint)MapKey].Split('~')[1].ToString())
                                  , ushort.Parse(Item.Agate_map[(uint)MapKey].Split('~')[2].ToString()), ushort.Parse(Item.Agate_map[(uint)MapKey].Split('~')[0].ToString()));
                                Item.Durability--;
                                Item.SendAgate(client);
                                //Database.ConquerItemTable.UpdateItemAgate(Item);
                            }
                        }
                        break;
                    }
                case AgateEx.AgateActions.Repair:
                    {
                        if (client.Player.DeadState) return;
                        if (!client.Player.Alive) return;
                        MsgGameItem Item = null;
                        if (client.Inventory.TryGetItem(ItemUID, out Item))
                        {
                            uint cost = (uint)(Item.MaximDurability - Item.Durability) / 2;
                            if (cost == 0)
                                cost = 1;
                            if (client.Player.ConquerPoints > cost)
                            {
                                client.Player.ConquerPoints -= (uint)cost;
                                Item.Durability = Item.MaximDurability;
                                Item.SendAgate(client);
                                //Database.ConquerItemTable.UpdateItemAgate(Item);
                            }
                        }
                        break;
                    }

            }
        }
    }
}
