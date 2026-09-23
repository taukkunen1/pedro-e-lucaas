using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetUpdateItem(this ServerSockets.Packet stream, out MsgUpdateItem.ActionType Action, out uint ItemUID, out List<uint> items)
        {
            Action =(MsgUpdateItem.ActionType)stream.ReadUInt8();
            byte FullCount = stream.ReadUInt8();
            ushort padding = stream.ReadUInt16();
            ItemUID = stream.ReadUInt32();
            uint Count = stream.ReadUInt32();

            items = new List<uint>();
            if (Action != MsgUpdateItem.ActionType.UpdateLevel && Action != MsgUpdateItem.ActionType.UpdateQuality)
            {

                items.Add(Count);
                for (byte x = 0; x < FullCount - 2; x++)
                {
                    items.Add(stream.ReadUInt32());
                }
            }
            else
            {
                for (byte x = 0; x < Count; x++)
                    items.Add(stream.ReadUInt32());
            }
        }
    }


    public struct MsgUpdateItem
    {
        public enum ActionType : byte
        {
            Plus = 0,
            CurrentSteed = 2,
            NewSteed = 3,
            ChanceUpgrade = 4,
            UpdateLevel = 6,
            UpdateQuality = 7
        }
        [PacketAttribute(GamePackets.Compose)]
        public unsafe static void Compose(Client.GameClient client, ServerSockets.Packet stream)
        {

            MsgUpdateItem.ActionType Action; uint ItemUID;
            List<uint> ItemsUIDS;

           
            //MsgUpdateItem* PacketCompose = (MsgUpdateItem*)packet.Pointer;

            uint dwParam1 = 2;

            stream.GetUpdateItem(out Action, out ItemUID, out ItemsUIDS);
            using var _econScope = GameServer.Telemetry.Economy.Scope(GameServer.Telemetry.SourceKind.Compose, (uint)Action);

            if (Action == ActionType.CurrentSteed || Action == ActionType.NewSteed)
            {
                client.SendSysMesage("Steed composition is not available in Era 1.");
                return;
            }

            switch (Action)
            {
                case ActionType.UpdateLevel:
                    {
                     
                

                       
                        if (ItemsUIDS.Count == 0)
                            break;

                        MsgGameItem DataItem;
                        if (client.TryGetItem(ItemUID, out DataItem))
                        {
                            if (!Game.Era1.Era1Economy.IsClassicForgeTarget(DataItem.ITEM_ID))
                                return;
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            //anti proxy --------------------
                            if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                            {
                                client.SendSysMesage("This item's level cannot be upgraded anymore.");
                                return;
                            }
                            //------------------------
                            MsgGameItem itemuse;
                            if (client.Inventory.ClientItems.TryGetValue(ItemsUIDS[0], out itemuse))
                            {
                                if (itemuse.ITEM_ID == Database.ItemType.DragonBall)
                                {
                                    Database.ItemType.DBItem DBItem;
                                    if (Pool.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                    {
                                         bool succesed = false;
                                         uint nextItemId = Pool.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);

                                         if ((DBItem.Level >= 70 && Database.ItemType.Equipable(nextItemId, client) == false)
                                               && (Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.RightWeapon
                                               || Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon))
                                         {
                                             client.CreateBoxDialog("You can`t update this item.");
                                         }
                                         else
                                         {
                                             dwParam1 = 1;
                                             uint oldid = DataItem.ITEM_ID;
                                             DataItem.ITEM_ID = Pool.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);
                                             DataItem.Mode = Role.Flags.ItemMode.Update;
                                             DataItem.Send(client, stream);//.Update(itemuse, Instance.AddMode.REMOVE,stream);
                                             if (succesed && oldid != DataItem.ITEM_ID)
                                             {
                                                 Game.Era1.Era1Economy.RecordEquipmentTransformation(client, oldid, DataItem.Plus, DataItem.ITEM_ID, DataItem.Plus);
                                                 client.Inventory.Update(itemuse, Instance.AddMode.REMOVE, stream);
                                             }
                                             else
                                             {
                                                 client.SendSysMesage("This item's level cannot be upgraded anymore.");
                                             }
                                         }
                                    }
                                }
                                else if (itemuse.ITEM_ID == Database.ItemType.Meteor || itemuse.ITEM_ID == Database.ItemType.MeteorScroll)
                                {
                                    if (client.Inventory.CheckMeteors((byte)ItemsUIDS.Count, false,stream))
                                    {
                                        Database.ItemType.DBItem DBItem;
                                        if (Pool.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                        {
                                            bool succesed = false;

                                            uint nextItemId = Pool.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);

                                            if ((DBItem.Level >= 70 && Database.ItemType.Equipable(nextItemId, client) == false)
                                             && (Database.ItemType.ItemPosition(DataItem.ITEM_ID) ==(ushort)Role.Flags.ConquerItem.RightWeapon
                                             ||Database.ItemType.ItemPosition(DataItem.ITEM_ID) ==(ushort)Role.Flags.ConquerItem.LeftWeapon))
                                            {
                                                client.CreateBoxDialog("You can`t update this item.");
                                            }
                                            else
                                            {
                                                if (Database.ItemType.UpItemMeteors(DataItem.ITEM_ID, (uint)ItemsUIDS.Count))
                                                {
                                                    dwParam1 = 1;
                                                    uint oldid = DataItem.ITEM_ID;
                                                    DataItem.ITEM_ID = Pool.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    if (succesed && oldid != DataItem.ITEM_ID)
                                                        Game.Era1.Era1Economy.RecordEquipmentTransformation(client, oldid, DataItem.Plus, DataItem.ITEM_ID, DataItem.Plus);

                                                }
#if TEST
                                            Console.WriteLine("Update Item " + DBItem.Name + " id = " + Database.ItemType.GetLevel(DataItem.ITEM_ID));
#endif

                                                client.Inventory.CheckMeteors((byte)ItemsUIDS.Count, true, stream);
                                           }
                                        }
                                    }
                                }
                                if (DataItem.Position != 0)
                                    client.Equipment.QueryEquipment(client.Equipment.Alternante);
                            }
                            client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.UpgradeMeteor, ItemUID, dwParam1, 0, 0, 0, 0));

                        }
                        break;
                    }
                case ActionType.UpdateQuality:
                    {
                      
            
                        if (ItemsUIDS.Count == 0)
                            break;
                        MsgGameItem DataItem;
                        if (client.TryGetItem(ItemUID, out DataItem))
                        {
                            if (!Game.Era1.Era1Economy.IsClassicForgeTarget(DataItem.ITEM_ID)
                                || DataItem.ITEM_ID % 10 >= 9)
                                return;
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            //anti proxy --------------------
                            if (Position != (ushort)Role.Flags.ConquerItem.Fan
                                && Position != (ushort)Role.Flags.ConquerItem.Tower && Position != (ushort)Role.Flags.ConquerItem.RidingCrop)
                            {
                                if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                {
                                    client.SendSysMesage("This item's Quality cannot be upgraded anymore.");
                                    return;
                                }
                            }
                            //------------------------
                            Queue<MsgGameItem> UseItems = new Queue<MsgGameItem>();
                            HashSet<uint> UniqueItems = new HashSet<uint>();
                            bool EmbedUpdate = false;
                            for (int x = 0; x < ItemsUIDS.Count; x++)
                            {
                                MsgGameItem itemuse;
                                if (UniqueItems.Add(ItemsUIDS[x])
                                    && client.Inventory.ClientItems.TryGetValue(ItemsUIDS[x], out itemuse)
                                    && itemuse.ITEM_ID == Database.ItemType.DragonBall)
                                {
                                    UseItems.Enqueue(itemuse);
                                    EmbedUpdate = true;
                                }
                                else { EmbedUpdate = false; break; }
                            }
                            if (EmbedUpdate && UseItems.Count > 0)
                            {
                                var CheckItem = UseItems.Dequeue();
                                if (CheckItem.ITEM_ID == Database.ItemType.DragonBall)
                                {
                                    Database.ItemType.DBItem DBItem;
                                    if (Pool.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                    {
                                        if (Database.ItemType.UpQualityDB(DataItem.ITEM_ID, (uint)(UseItems.Count + 1)))
                                        {
                                            dwParam1 = 1;
                                            uint oldid = DataItem.ITEM_ID;
                                            if (DataItem.ITEM_ID % 10 < 5)
                                                DataItem.ITEM_ID += 5 - DataItem.ITEM_ID % 10;
                                            DataItem.ITEM_ID++;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(client,stream);
                                            Game.Era1.Era1Economy.RecordEquipmentTransformation(client, oldid, DataItem.Plus, DataItem.ITEM_ID, DataItem.Plus);
                                        }


                                        client.Inventory.Update(CheckItem, Instance.AddMode.REMOVE,stream);
                                        while (UseItems.Count > 0)
                                            client.Inventory.Update(UseItems.Dequeue(), Instance.AddMode.REMOVE,stream);
                                    }
                                }
                            }
                            if (DataItem.Position != 0)
                                client.Equipment.QueryEquipment(client.Equipment.Alternante);
                        }

                        client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.UpgradeDragonball,ItemUID, dwParam1, 0, 0, 0, 0));


                        break;
                    }
                default:
                    {
                        if (Action == ActionType.ChanceUpgrade)
                        {
                            client.SendSysMesage("Quick Compose is not available in Era 1.");
                            return;
                        }
                        if (Action != ActionType.Plus)
                            return;

                        MsgGameItem DataItem;
                        if (!client.TryGetItem(ItemUID, out DataItem))
                            break;
                        if (!Game.Era1.Era1Economy.CanComposeTarget(DataItem.ITEM_ID))
                            return;
                        if (DataItem.Plus >= 12)
                            return;

                        // Patch 5002-era +10/+11/+12 refining: fixed Dragon Ball costs.
                        if (DataItem.Plus >= 9)
                        {
                            if (client.Player.Level < Game.Era1.Era1Economy.ClassicHighPlusPlayerLevel)
                            {
                                client.SendSysMesage("You must be level 130 or higher to refine +9 equipment further.");
                                return;
                            }

                            byte dragonBallCost = Game.Era1.Era1Economy.ClassicHighPlusDragonBallCost(DataItem.Plus);
                            if (dragonBallCost == 0 || !client.Inventory.CheckDragonBalls(dragonBallCost, false, stream))
                            {
                                client.SendSysMesage("You do not have enough Dragon Balls for this refinement.");
                                return;
                            }

                            byte oldplus = DataItem.Plus;
                            if (!client.Inventory.CheckDragonBalls(dragonBallCost, true, stream))
                                return;

                            DataItem.Plus++;
                            DataItem.PlusProgress = 0;
                            DataItem.Mode = Role.Flags.ItemMode.Update;
                            DataItem.Send(client, stream);
                            Game.Era1.Era1Economy.RecordEquipmentTransformation(client, DataItem.ITEM_ID, oldplus, DataItem.ITEM_ID, DataItem.Plus);

                            client.Map.SendSysMesage("Congratulations, " + client.Player.Name + " has upgraded " + Pool.ItemsBase[DataItem.ITEM_ID].Name + " to +" + DataItem.Plus + "!");
                            if (DataItem.Position != 0)
                                client.Equipment.QueryEquipment(client.Equipment.Alternante);
                            break;
                        }

                        // 5017 composition uses one main item + two compatible minor items.
                        // Composition Points/Quick Compose are post-5017 (patch 5066+) and are not used.
                        Queue<MsgGameItem> MinorItems = new Queue<MsgGameItem>();
                        Queue<MsgGameItem> GemItems = new Queue<MsgGameItem>();
                        HashSet<uint> UniqueItems = new HashSet<uint>();
                        bool valid = true;
                        byte requiredMinorPlus = DataItem.Plus == 0 ? (byte)1 : DataItem.Plus;

                        for (int x = 0; x < ItemsUIDS.Count; x++)
                        {
                            MsgGameItem itemuse;
                            if (!UniqueItems.Add(ItemsUIDS[x])
                                || !client.Inventory.ClientItems.TryGetValue(ItemsUIDS[x], out itemuse)
                                || itemuse.UID == DataItem.UID
                                || itemuse.Locked != 0)
                            {
                                valid = false;
                                break;
                            }

                            if (Game.Era1.Era1Economy.IsClassicGem(itemuse.ITEM_ID))
                            {
                                GemItems.Enqueue(itemuse);
                                continue;
                            }

                            if (!Game.Era1.Era1Economy.IsAllowedCompositionMaterial(DataItem.ITEM_ID, itemuse.ITEM_ID))
                            {
                                valid = false;
                                break;
                            }

                            byte materialPlus = Game.Era1.Era1Economy.CompositionMaterialPlus(itemuse.ITEM_ID, itemuse.Plus);
                            if (materialPlus == 0 || materialPlus > 8 || materialPlus < requiredMinorPlus)
                            {
                                valid = false;
                                break;
                            }
                            MinorItems.Enqueue(itemuse);
                        }

                        byte gemCost = Game.Era1.Era1Economy.ClassicComposeGemCost(DataItem.ITEM_ID, DataItem.Plus);
                        if (!valid
                            || MinorItems.Count != Game.Era1.Era1Economy.ClassicComposeMinorCount
                            || GemItems.Count != gemCost)
                        {
                            client.SendSysMesage("Invalid Era 1 composition materials.");
                            return;
                        }

                        byte previousPlus = DataItem.Plus;
                        while (MinorItems.Count > 0)
                            client.Inventory.Update(MinorItems.Dequeue(), Instance.AddMode.REMOVE, stream);
                        while (GemItems.Count > 0)
                            client.Inventory.Update(GemItems.Dequeue(), Instance.AddMode.REMOVE, stream);

                        DataItem.Plus++;
                        DataItem.PlusProgress = 0;
                        DataItem.Mode = Role.Flags.ItemMode.Update;
                        DataItem.Send(client, stream);
                        Game.Era1.Era1Economy.RecordEquipmentTransformation(client, DataItem.ITEM_ID, previousPlus, DataItem.ITEM_ID, DataItem.Plus);

                        if (DataItem.Plus >= 6)
                            client.Map.SendSysMesage("Congratulations, " + client.Player.Name + " has upgraded " + Pool.ItemsBase[DataItem.ITEM_ID].Name + " to +" + DataItem.Plus + "!");

                        if (DataItem.Position != 0)
                            client.Equipment.QueryEquipment(client.Equipment.Alternante);
                        break;
                    }

            }
        }
    }
}
