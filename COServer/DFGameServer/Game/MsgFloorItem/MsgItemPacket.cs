using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameServer.Game.MsgFloorItem
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetItemPacket(this ServerSockets.Packet stream, out uint uid)
        {
            //   uint stamp = stream.ReadUInt32();
            uid = stream.ReadUInt32();
        }
        public static unsafe ServerSockets.Packet ItemPacketCreate(this ServerSockets.Packet stream, MsgItemPacket Item)
        {
            stream.InitWriter();//stream
            stream.Write(Item.m_UID);//4
            stream.Write(Item.m_ID);//8
            stream.Write(Item.m_X);//12
            stream.Write(Item.m_Y);//14
            stream.Write((ushort)Item.m_Color);//16
            stream.Write((byte)Item.DropType);//18
            stream.Finalize(GamePackets.FloorMap);
            return stream;
        }
    }

    public unsafe class MsgItemPacket
    {
        public enum EffectMonsters : uint
        {
            None = 0,
            EarthquakeLeftRight = 1,
            EarthquakeUpDown = 2,
            Night = 4,
            EarthquakeAndNight = 5
        }

        public const uint
            DBShowerEffect = 17;


        public uint m_UID;
        public uint m_ID;
        public ushort m_X;
        public ushort m_Y;
        public ushort MaxLife;
        public MsgDropID DropType;
        public uint Life;
        public byte m_Color;
        public byte m_Color2;
        public uint ItemOwnerUID;
        public byte DontShow;
        public uint GuildID;
        public byte FlowerType;
        public ulong Timer;
        public string Name;
        public uint UnKnow;
        public byte Plus;



        public ushort OwnerX;
        public ushort OwnerY;

        public static MsgItemPacket Create()
        {
            MsgItemPacket item = new MsgItemPacket();
            return item;
        }

        public static bool TryAutoPickup(Client.GameClient client, MsgFloorItem.MsgItem mapItem, ServerSockets.Packet packet)
        {
            if (client == null || mapItem == null || client.InTrade || !client.Player.OnMyOwnServer)
                return false;
            if (!client.AutoHunting.Enable || !AutoHunting.CanAutoPickUp(client.Player.VipLevel))
                return false;
            if (!client.AutoHunting.ShouldAutoPickUp(mapItem))
                return false;
            if (Role.Core.GetDistance(client.Player.X, client.Player.Y, mapItem.X, mapItem.Y) > 5)
                return false;

            // Respect the same temporary owner/team protection used by manual pickup.
            if (mapItem.ToMySelf && !mapItem.ExpireMySelf && mapItem.ItemOwner != client.Player.UID)
            {
                if (client.Team == null || !client.Team.IsTeamMember(mapItem.ItemOwner))
                    return false;
                if (mapItem.Typ == MsgItem.ItemType.Money && !client.Team.PickupMoney)
                    return false;
                if (mapItem.Typ == MsgItem.ItemType.Item && !client.Team.PickupItems)
                    return false;
            }

            switch (mapItem.Typ)
            {
                case MsgItem.ItemType.Money:
                    if (!mapItem.TryClaimPickup())
                        return false;
                    client.Player.Money += mapItem.Gold;
                    client.Player.SendUpdate(packet, client.Player.Money, MsgServer.MsgUpdate.DataType.Money);
                    mapItem.SendAll(packet, MsgDropID.Remove);
                    client.Map.cells[mapItem.X, mapItem.Y] &= ~Role.MapFlagType.Item;
                    client.Map.View.LeaveMap<Role.IMapObj>(mapItem);
                    return true;

                case MsgItem.ItemType.Item:
                    if (mapItem.ItemBase == null || !client.Inventory.HaveSpace(1))
                        return false;

                    Database.ItemType.DBItem dbItem;
                    if (!Pool.ItemsBase.TryGetValue(mapItem.MsgFloor.m_ID, out dbItem))
                        return false;
                    if (!mapItem.TryClaimPickup())
                        return false;

                    bool awarded = false;
                    try
                    {
                        bool added;
                        if (mapItem.ItemBase.StackSize > 1)
                            added = client.Inventory.Update(mapItem.ItemBase, Instance.AddMode.ADD, packet);
                        else
                            added = client.Inventory.Add(mapItem.ItemBase, dbItem, packet);

                        if (!added)
                        {
                            mapItem.ReleasePickupClaim();
                            return false;
                        }

                        // From this point onward the item belongs to the player.
                        // Never release the floor claim after a successful inventory award,
                        // even if map cleanup or a quest side effect throws.
                        awarded = true;
                        client.Map.cells[mapItem.X, mapItem.Y] &= ~Role.MapFlagType.Item;
                        client.Map.View.LeaveMap<Role.IMapObj>(mapItem);
                        mapItem.SendAll(packet, MsgDropID.Remove);
                        if (dbItem.ID == 711352)
                            client.Player.QuestGUI.IncreaseQuestObjectives(packet, 1311, 1);
                        return true;
                    }
                    catch
                    {
                        if (!awarded)
                            mapItem.ReleasePickupClaim();
                        throw;
                    }
            }

            return false;
        }

        [PacketAttribute(GamePackets.FloorMap)]
        public unsafe static void FloorMap(Client.GameClient client, ServerSockets.Packet packet)
        {
            if (client.InTrade)
                return;
            if (!client.Player.OnMyOwnServer)
                return;

            uint m_UID;

            packet.GetItemPacket(out m_UID);

            MsgFloorItem.MsgItem MapItem;
            if (client.Map.View.TryGetObject<MsgFloorItem.MsgItem>(m_UID, Role.MapObjectType.Item, client.Player.X, client.Player.Y, out MapItem))
            {
                if (MapItem.ToMySelf)
                {
                    if (!MapItem.ExpireMySelf)
                    {
                        if (MapItem.ItemOwner != client.Player.UID)
                        {
                            if (client.Team != null)
                            {
                                if (client.Team.IsTeamMember(MapItem.ItemOwner))
                                {
                                    if (MapItem.Typ == MsgItem.ItemType.Money && !client.Team.PickupMoney)
                                    {
                                        client.SendSysMesage("You cannot get the money because the Owner of team not allowed this.");
                                        return;
                                    }
                                    if (MapItem.Typ == MsgItem.ItemType.Item && !client.Team.PickupItems)
                                    {
                                        client.SendSysMesage("You cannot get the item because the Owner of team not allowed this.");
                                        return;
                                    }
                                } else
                                {
                                    // The item is not from any member of the team
                                    client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players or other teams.");
                                    return;
                                }
                            }
                            else if (client.Team == null)
                            {
                                if (MapItem.Typ == MsgItem.ItemType.Money)
                                {
                                    client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players.");
                                    return;
                                }
                                else
                                {
                                    client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players.");
                                    return;
                                }
                            }
                        }
                    }
                }
                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y) <= 5)
                {
                    switch (MapItem.Typ)
                    {

                        case MsgItem.ItemType.Money:
                            {
                                if (!MapItem.TryClaimPickup())
                                    return;

                                client.Player.Money += MapItem.Gold;
                                client.Player.SendUpdate(packet, client.Player.Money, MsgServer.MsgUpdate.DataType.Money);
                                MapItem.SendAll(packet, MsgDropID.Remove);
                                client.Map.cells[MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y] &= ~Role.MapFlagType.Item;
                                client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                client.SendSysMesage("You have picked up a " + MapItem.Gold + " silvers.");
                                break;
                            }
                        case MsgItem.ItemType.Item:
                            {
                                Database.ItemType.DBItem DBItem;
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (Pool.ItemsBase.TryGetValue(MapItem.MsgFloor.m_ID, out DBItem))
                                    {
                                        if (!MapItem.TryClaimPickup())
                                            return;

                                        bool added;
                                        if (MapItem.ItemBase.StackSize > 1)
                                            added = client.Inventory.Update(MapItem.ItemBase, Instance.AddMode.ADD, packet);
                                        else
                                            added = client.Inventory.Add(MapItem.ItemBase, DBItem, packet);

                                        if (!added)
                                        {
                                            MapItem.ReleasePickupClaim();
                                            return;
                                        }

                                        client.Map.cells[MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y] &= ~Role.MapFlagType.Item;
                                        client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                        MapItem.SendAll(packet, MsgDropID.Remove);
                                        client.SendSysMesage("You have picked up a " + DBItem.Name + ".");
                                        if (DBItem.ID == 711352)
                                        {
                                            client.Player.QuestGUI.IncreaseQuestObjectives(packet, 1311, 1);
                                        }
                                    }
                                }
                                break;
                            }
                        case MsgItem.ItemType.Cps:
                            {
                                Database.ItemType.DBItem DBItem;
                                if (Pool.ItemsBase.TryGetValue(MapItem.MsgFloor.m_ID, out DBItem))
                                {
                                    if (!MapItem.TryClaimPickup())
                                        return;

                                    if (MapItem.ItemBase.ITEM_ID == 3001133)
                                    {
                                        client.Player.ConquerPoints += 5;
                                        client.SendSysMesage("You have picked up a 5 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720657)
                                    {
                                        client.Player.ConquerPoints += 5;
                                        client.SendSysMesage("You have picked up a 5 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720656)
                                    {
                                        client.Player.ConquerPoints += 10;
                                        client.SendSysMesage("You have picked up a 10 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 3001134)
                                    {
                                        client.Player.ConquerPoints += 10;
                                        client.SendSysMesage("You have picked up a 10 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 3001135)
                                    {
                                        client.Player.ConquerPoints += 20;
                                        client.SendSysMesage("You have picked up a 20 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720655)
                                    {
                                        client.Player.ConquerPoints += 20;
                                        client.SendSysMesage("You have picked up a 20 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720658)
                                    {
                                        client.Player.ConquerPoints += 25;
                                        client.SendSysMesage("You have picked up a 25 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720663)
                                    {
                                        client.Player.ConquerPoints += 50;
                                        client.SendSysMesage("You have picked up a 50 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720659)
                                    {
                                        client.Player.ConquerPoints += 50;
                                        client.SendSysMesage("You have picked up a 50 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720660)
                                    {
                                        client.Player.ConquerPoints += 100;
                                        client.SendSysMesage("You have picked up a 100 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720664)
                                    {
                                        client.Player.ConquerPoints += 100;
                                        client.SendSysMesage("You have picked up a 100 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720665)
                                    {
                                        client.Player.ConquerPoints += 200;
                                        client.SendSysMesage("You have picked up a 200 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720675)
                                    {
                                        client.Player.ConquerPoints += 200;
                                        client.SendSysMesage("You have picked up a 250 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720653)
                                    {
                                        client.Player.ConquerPoints += 270;
                                        client.SendSysMesage("You have picked up a 270 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720681)
                                    {
                                        client.Player.ConquerPoints += 500;
                                        client.SendSysMesage("You have picked up a 500 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 720676)
                                    {
                                        client.Player.ConquerPoints += 500;
                                        client.SendSysMesage("You have picked up a 500 ConquerPoints.");
                                    }
                                    else if (MapItem.ItemBase.ITEM_ID == 3001136)
                                    {
                                        client.Player.ConquerPoints += 500;
                                        client.SendSysMesage("You have picked up a 500 ConquerPoints.");
                                    }
                                    else
                                        client.Inventory.Add(MapItem.ItemBase, DBItem, packet);
                                    MapItem.SendAll(packet, MsgDropID.Remove);
                                    client.Map.cells[MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y] &= ~Role.MapFlagType.Item;
                                    client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                    break;
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
