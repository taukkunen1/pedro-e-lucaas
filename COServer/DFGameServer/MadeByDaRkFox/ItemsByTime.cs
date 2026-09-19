using GameServer.Game.MsgServer;
using System;

namespace GameServer.MadeByDaRkFox
{
    class ItemExpireSystem
    {
        public static void CheckItemsTime(Client.GameClient client, int time)
        {
            try
            {
                if (client == null || !client.FullLoading || client.Player == null)
                    return;
                foreach (var itemWH in client.AllItemsTimeWarehouse())
                {
                    if (DateTime.Now < itemWH.EndDate)
                        continue;

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        foreach (var Wh in client.Warehouse.ClientItems)
                        {
                            foreach (var item2 in Wh.Value.Values)
                            {
                                if (item2.UID == itemWH.UID)
                                {
                                    client.Warehouse.RemoveItem(item2.UID, Wh.Key, stream);
                                    client.Send(stream.FinalizeWarehouse());
                                    client.Inventory.Update(itemWH, Instance.AddMode.REMOVE, stream);
                                    client.SendSysMesage($"{Pool.ItemsBase.GetItemName(itemWH.ITEM_ID)} has expired!", MsgMessage.ChatMode.SystemWhisper);

                                }
                            }
                        }
                    }
                }
                foreach (var item in client.AllMyItemsInvEquip())
                {
                    if (DateTime.Now < item.EndDate)
                        continue;

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        if (client.Inventory.ClientItems.ContainsKey(item.UID))
                        {
                            client.Inventory.Update(item, Instance.AddMode.REMOVE, stream);
                            client.SendSysMesage($"{Pool.ItemsBase.GetItemName(item.ITEM_ID)} has expired!", MsgMessage.ChatMode.System);
                            continue;
                        }
                        if (client.Equipment.ClientItems.ContainsKey(item.UID))
                        {
                            Role.Flags.ConquerItem position = (Role.Flags.ConquerItem)Database.ItemType.ItemPosition(item.ITEM_ID);
                            client.Equipment.Remove(position, stream);
                            client.Inventory.Update(item, Instance.AddMode.REMOVE, stream);
                            client.Equipment.QueryEquipment(client.Equipment.Alternante);
                            client.SendSysMesage($"{Pool.ItemsBase.GetItemName(item.ITEM_ID)} has expired!", MsgMessage.ChatMode.System);

                        }

                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
