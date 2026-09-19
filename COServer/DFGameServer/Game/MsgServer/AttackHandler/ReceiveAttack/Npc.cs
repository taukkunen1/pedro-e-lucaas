namespace GameServer.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Npc
    {
        public static uint Execute(ServerSockets.Packet stream, MsgSpellAnimation.SpellObj obj, Client.GameClient client, Role.SobNpc attacked)
        {

            if (obj.Damage >= attacked.HitPoints)
            {
                uint exp = (uint)attacked.HitPoints;
                attacked.Die(stream, client);
                if (attacked.Map == 1039)
                    return exp / 10;
            }
            else
            {
                attacked.HitPoints -= (int)obj.Damage;
                bool HasPoledominationFurniture = Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures.ContainsKey(Role.SobNpc.StaticMesh.Pole) && Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures[Role.SobNpc.StaticMesh.Pole] != null;
                #region GuildWar
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.GuildWar.UpdateScore(client.Player, obj.Damage);
                #endregion
                #region ClassicClanWar
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.ClassicClanWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.ClassicClanWar.UpdateScore(client.Player, obj.Damage);
                #endregion
                #region EliteGuildwar
                else if (attacked.UID == Game.MsgTournaments.MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.EliteGuildWar.UpdateScore(client.Player, obj.Damage);
                #endregion
                #region Pole
                else if (HasPoledominationFurniture && attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    #region Code Items
                    if (Role.Core.Rate(15))
                    {
                        uint TargetItem = 0;
                        ulong type = 0;
                        #region CPPacks
                        type = (byte)Pool.GetRandom.Next(1, 3);
                        switch (type)
                        {
                            case 1:
                                TargetItem = 721026; // 50 CPs
                                break;
                            case 2:
                                TargetItem = 721027; // 100 CPs
                                break;
                            case 3:
                                TargetItem = 721028; // 200 CPs
                                break;
                        }
                        if (Role.Core.Rate(20)) // Rare drop
                        {
                            TargetItem = 3003749; // 3000 CPs [ConquerPointsBag]
                            client.SendSysMesage($"[PoleDomination] WOW!!! The player{client.Player.Name} has lucky and get dropped ConquerPointsBag(3000 CPs Reward).");
                        }
                        #endregion
                        ushort X = client.Player.X, Y = client.Player.Y;
                        MsgServer.MsgGameItem Items = new MsgGameItem();

                        Items.ITEM_ID = TargetItem;
                        Database.ItemType.DBItem DBItem;
                        if (Pool.ItemsBase.TryGetValue(TargetItem, out DBItem))
                        {
                            Items.Durability = Items.MaximDurability = Items.Durability;//duracion

                        }
                        Items.Color = Role.Flags.Color.Red;

                        if (client.Map.AddGroundItem(ref X, ref Y, 1))
                        {
                            MsgFloorItem.MsgItem dropitem = new MsgFloorItem.MsgItem(Items, X, Y, MsgFloorItem.MsgItem.ItemType.Item, 0, client.Player.DynamicID, client.Player.Map, client.Player.UID, true, client.Map);
                            if (client.Map.EnqueueItem(dropitem))
                            {
                                dropitem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                            }
                        }

                    }

                    #endregion
                    Game.MsgTournaments.MsgSchedules.PoleDomination.UpdateScore(client.Player, obj.Damage);
                }
                else if (HasPoledominationFurniture && attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDominationBI.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    #region Code Items
                    if (Role.Core.Rate(15))
                    {
                        uint TargetItem = 0;
                        ulong type = 0;
                        #region CPPacks
                        type = (byte)Pool.GetRandom.Next(1, 3);
                        switch (type)
                        {
                            case 1:
                                TargetItem = 721026; // 50 CPs
                                break;
                            case 2:
                                TargetItem = 721027; // 100 CPs
                                break;
                            case 3:
                                TargetItem = 721028; // 200 CPs
                                break;
                        }
                        if (Role.Core.Rate(20)) // Rare drop
                        {
                            TargetItem = 3003749; // 3000 CPs [ConquerPointsBag]
                            client.SendSysMesage($"[PoleDomination] WOW!!! The player{client.Player.Name} has lucky and get dropped ConquerPointsBag(3000 CPs Reward).");
                        }
                        #endregion
                        ushort X = client.Player.X, Y = client.Player.Y;
                        MsgServer.MsgGameItem Items = new MsgGameItem();

                        Items.ITEM_ID = TargetItem;
                        Database.ItemType.DBItem DBItem;
                        if (Pool.ItemsBase.TryGetValue(TargetItem, out DBItem))
                        {
                            Items.Durability = Items.MaximDurability = Items.Durability;//duracion

                        }
                        Items.Color = Role.Flags.Color.Red;

                        if (client.Map.AddGroundItem(ref X, ref Y, 1))
                        {
                            MsgFloorItem.MsgItem dropitem = new MsgFloorItem.MsgItem(Items, X, Y, MsgFloorItem.MsgItem.ItemType.Item, 0, client.Player.DynamicID, client.Player.Map, client.Player.UID, true, client.Map);
                            if (client.Map.EnqueueItem(dropitem))
                            {
                                dropitem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                            }
                        }

                    }

                    #endregion
                    Game.MsgTournaments.MsgSchedules.PoleDominationBI.UpdateScore(client.Player, obj.Damage);
                }
                else if (HasPoledominationFurniture && attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDominationDC.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    #region Code Items

                    if (Role.Core.Rate(15))
                    {
                        uint TargetItem = 0;
                        ulong type = 0;
                        #region CPPacks
                        type = (byte)Pool.GetRandom.Next(1, 3);
                        switch (type)
                        {
                            case 1:
                                TargetItem = 721026; // 50 CPs
                                break;
                            case 2:
                                TargetItem = 721027; // 100 CPs
                                break;
                            case 3:
                                TargetItem = 721028; // 200 CPs
                                break;
                        }
                        if (Role.Core.Rate(20)) // Rare drop
                        {
                            TargetItem = 3003749; // 3000 CPs [ConquerPointsBag]
                            client.SendSysMesage($"[PoleDomination] WOW!!! The player{client.Player.Name} has lucky and get dropped ConquerPointsBag(3000 CPs Reward).");
                        }
                        #endregion
                        ushort X = client.Player.X, Y = client.Player.Y;
                        MsgServer.MsgGameItem Items = new MsgGameItem();

                        Items.ITEM_ID = TargetItem;
                        Database.ItemType.DBItem DBItem;
                        if (Pool.ItemsBase.TryGetValue(TargetItem, out DBItem))
                        {
                            Items.Durability = Items.MaximDurability = Items.Durability;//duracion

                        }
                        Items.Color = Role.Flags.Color.Red;

                        if (client.Map.AddGroundItem(ref X, ref Y, 1))
                        {
                            MsgFloorItem.MsgItem dropitem = new MsgFloorItem.MsgItem(Items, X, Y, MsgFloorItem.MsgItem.ItemType.Item, 0, client.Player.DynamicID, client.Player.Map, client.Player.UID, true, client.Map);
                            if (client.Map.EnqueueItem(dropitem))
                            {
                                dropitem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                            }
                        }

                    }

                    #endregion
                    Game.MsgTournaments.MsgSchedules.PoleDominationDC.UpdateScore(client.Player, obj.Damage);
                }
                else if (HasPoledominationFurniture && attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDominationPC.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    #region Code Items

                    if (Role.Core.Rate(15))
                    {
                        uint TargetItem = 0;
                        ulong type = 0;
                        #region CPPacks
                        type = (byte)Pool.GetRandom.Next(1, 3);
                        switch (type)
                        {
                            case 1:
                                TargetItem = 721026; // 50 CPs
                                break;
                            case 2:
                                TargetItem = 721027; // 100 CPs
                                break;
                            case 3:
                                TargetItem = 721028; // 200 CPs
                                break;
                        }
                        if (Role.Core.Rate(20)) // Rare drop
                        {
                            TargetItem = 3003749; // 3000 CPs [ConquerPointsBag]
                            client.SendSysMesage($"[PoleDomination] WOW!!! The player{client.Player.Name} has lucky and get dropped ConquerPointsBag(3000 CPs Reward).");
                        }
                        #endregion
                        ushort X = client.Player.X, Y = client.Player.Y;
                        MsgServer.MsgGameItem Items = new MsgGameItem();

                        Items.ITEM_ID = TargetItem;
                        Database.ItemType.DBItem DBItem;
                        if (Pool.ItemsBase.TryGetValue(TargetItem, out DBItem))
                        {
                            Items.Durability = Items.MaximDurability = Items.Durability;//duracion

                        }
                        Items.Color = Role.Flags.Color.Red;

                        if (client.Map.AddGroundItem(ref X, ref Y, 1))
                        {
                            MsgFloorItem.MsgItem dropitem = new MsgFloorItem.MsgItem(Items, X, Y, MsgFloorItem.MsgItem.ItemType.Item, 0, client.Player.DynamicID, client.Player.Map, client.Player.UID, true, client.Map);
                            if (client.Map.EnqueueItem(dropitem))
                            {
                                dropitem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                            }
                        }

                    }

                    #endregion
                    Game.MsgTournaments.MsgSchedules.PoleDominationPC.UpdateScore(client.Player, obj.Damage);
                }
                #endregion
                #region CityWar
                else if (attacked.UID >= 821 && attacked.UID <= 825)
                    Game.MsgTournaments.MsgSchedules.CityWar.UpdateScore(client.Player, obj.Damage, attacked.UID);
                #endregion
                #region Forstrees
                else if (MsgTournaments.MsgFortressWar.Proces == MsgTournaments.ProcesType.Alive && attacked.UID == MsgTournaments.MsgFortressWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    MsgTournaments.MsgFortressWar.UpdateScore(stream, obj.Damage, client.Player);
                #endregion
                #region CaptureTheflags
                if (Game.MsgTournaments.MsgSchedules.CaptureTheFlag.Bases.ContainsKey(attacked.UID))
                {
                    Game.MsgTournaments.MsgSchedules.CaptureTheFlag.UpdateFlagScore(client.Player, attacked, obj.Damage, stream);
                }
                #endregion
                if (attacked.Map == 1039 || attacked.Map == 1038)
                    return obj.Damage / 1000;
            }
            return 0;
        }
    }
}
