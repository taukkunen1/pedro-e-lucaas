using Core;
using GameServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Role
{
    public class RoleView
    {
        public const int ViewThreshold = 18;
        public Game.MsgMonster.ActionHandler MobActions = new Game.MsgMonster.ActionHandler();
        public SafeDictionaryAlt<uint, IMapObj>[] src;
        public Client.GameClient Owner;
        private ConcurrentDictionary<uint, bool> visibilityCache = new ConcurrentDictionary<uint, bool>();

        public RoleView(Client.GameClient _client)
        {
            Owner = _client;
            src = new SafeDictionaryAlt<uint, IMapObj>[(byte)MapObjectType.Count];
            for (byte x = 0; x < (byte)MapObjectType.Count; x++)
                src[x] = new SafeDictionaryAlt<uint, IMapObj>();
        }

        public Role.Player GetPlayer() => Owner.Player;

        public void ReSendView(ServerSockets.Packet stream)
            => SendView(Owner.Player.GetArray(stream, false), false);

        public bool SameLocation(MapObjectType typ, out Role.IMapObj obj)
        {
            foreach (var client in Roles(typ))
            {
                if (client.X == GetPlayer().X && client.Y == GetPlayer().Y)
                {
                    obj = client;
                    return true;
                }
            }
            obj = null;
            return false;
        }

        public void SendView(ServerSockets.Packet msg, bool me)
        {
            if (me)
                Owner.Send(msg);

            var roles = Roles(MapObjectType.Player).OrderBy(obj => Core.GetDistance(obj.X, obj.Y, Owner.Player.X, Owner.Player.Y));

            foreach (IMapObj obj in roles)
            {
                obj.Send(msg);
            }
        }

        public unsafe void SendView(byte[] msg, bool me)
        {
            if (me)
                Owner.Send(msg);

            foreach (IMapObj obj in Roles(MapObjectType.Player))
            {
                (obj as Role.Player).Owner.Send(msg);
            }
        }

        public IEnumerable<IMapObj> AttackableRoles()
        {
            foreach (var rule in Owner.Map.View.Roles(MapObjectType.Monster, Owner.Player.X, Owner.Player.Y, CanSee))
                yield return rule;
            foreach (var rule in Owner.Map.View.Roles(MapObjectType.Player, Owner.Player.X, Owner.Player.Y, CanSee))
                yield return rule;
            foreach (var rule in Owner.Map.View.Roles(MapObjectType.SobNpc, Owner.Player.X, Owner.Player.Y, CanSee))
                yield return rule;
        }

        public IEnumerable<IMapObj> Roles(MapObjectType typ, Predicate<bool> P = null)
        {
            if (Owner.Map == null) return Array.Empty<IMapObj>();

            var playerX = Owner.Player.X;
            var playerY = Owner.Player.Y;

            return Owner.Map.View.Roles(typ, playerX, playerY, p => CanSee(p) && (P == null || P(p.Alive)));
        }

        public bool TryGetValue(uint UID, out IMapObj obj, MapObjectType typ)
        {
            if (Owner.Map != null)
                return Owner.Map.View.TryGetObject<IMapObj>(UID, typ, Owner.Player.X, Owner.Player.Y, out obj);

            obj = null;
            return false;
        }

        public bool CanSee(IMapObj obj)
        {
            if (obj == null || obj.Map != Owner.Player.Map || obj.UID == Owner.Player.UID)
                return false;

            if (!obj.AllowDynamic && obj.DynamicID != Owner.Player.DynamicID)
                return false;

            if (visibilityCache.TryGetValue(obj.UID, out var cachedResult))
                return cachedResult;

            bool result = Core.GetDistance(obj.X, obj.Y, Owner.Player.X, Owner.Player.Y) <= ViewThreshold + 5;

            if (result)
                visibilityCache[obj.UID] = result;

            return result;
        }

        public bool Contains(IMapObj obj)
        {
            return obj.UID == Owner.Player.UID ||
                   (Owner.Map != null && Owner.Map.View.Contain(obj.UID, Owner.Player.X, Owner.Player.Y));
        }

        public bool ContainMobInScreen(string name)
        {
            foreach (var obj in Roles(MapObjectType.Monster))
            {
                if ((obj as Game.MsgMonster.MonsterRole)?.Name == name)
                    return true;
            }
            return false;
        }

        public unsafe bool CanAdd(IMapObj obj, bool force, ServerSockets.Packet stream)
        {
            if (!CanSee(obj) || !(Owner.Player.InView(obj.X, obj.Y, ViewThreshold) || force))
                return false;

            try
            {
                switch (obj.ObjType)
                {
                    case MapObjectType.Monster:
                        HandleMonster(obj as Game.MsgMonster.MonsterRole, stream);
                        break;

                    case MapObjectType.Player:
                        HandlePlayer(obj as Role.Player, stream);
                        break;

                    case MapObjectType.Item:
                        HandleItem(obj as Game.MsgFloorItem.MsgItem, stream);
                        break;

                    case MapObjectType.Npc:
                        HandleNpc(obj, stream);
                        break;

                    case MapObjectType.SobNpc:
                    case MapObjectType.StaticRole:
                        Owner.Send(obj.GetArray(stream, false));
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
            return true;
        }

        private void HandleMonster(Game.MsgMonster.MonsterRole monster, ServerSockets.Packet stream)
        {
            if (!monster.Alive)
            {
                if (monster.CanRespawn(Owner.Map))
                    monster.Respawn(false);
            }
            else
            {
                Owner.Send(monster.GetArray(stream, false));
            }
        }

        private void HandlePlayer(Role.Player player, ServerSockets.Packet stream)
        {
            var pClient = player.Owner;

            if (Owner.Player.Map == 700 &&
                (Owner.InQualifier() && pClient.IsWatching() ||
                 Owner.InTeamQualifier() && pClient.IsWatching() ||
                 pClient.IsWatching()))
                return;

            if (!Owner.Player.Invisible && pClient.Player.Invisible)
                return;

            if (pClient.Player.Invisible)
                return;

            Owner.Send(player.GetArray(stream, false));
        }

        private void HandleItem(Game.MsgFloorItem.MsgItem item, ServerSockets.Packet stream)
        {
            if (!item.Alive)
            {
                item.SendAll(stream, Game.MsgFloorItem.MsgDropID.Remove);
                Owner.Map.View.LeaveMap<IMapObj>(item);
            }
            else
            {
                Owner.Send(item.GetArray(stream, false));
            }
        }

        private void HandleNpc(IMapObj obj, ServerSockets.Packet stream)
        {
            if (obj.Map == 1015 && obj.UID == (uint)Game.MsgNpc.NpcID.LittleBen &&
                Owner.Player.QuestGUI.CheckQuest(6129, MsgQuestList.QuestListItem.QuestStatus.Finished))
                return;

            Owner.Send(obj.GetArray(stream, false));
        }

        public unsafe void Role(bool clear = false, ServerSockets.Packet msg = null)
        {
            if (Owner.Player == null || Owner.Map == null)
                return;

            var playerX = Owner.Player.X;
            var playerY = Owner.Player.Y;

            if (clear)
            {
                Owner.Player.Px = 0;
                Owner.Player.Py = 0;
            }

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                try
                {
                    if (Database.HouseTable.InHouse(Owner.Player.Map))
                    {
                        if (Owner.Player.UID == Owner.Player.DynamicID)
                        {
                            if (Owner.MyHouse != null)
                            {
                                foreach (var npc in Owner.MyHouse.Furnitures.Values)
                                {
                                    npc.Send(stream);
                                }
                            }
                        }
                        else if (global::GameServer.Role.Instance.House.HousePoll.TryGetValue(Owner.Player.DynamicID, out var house))
                        {
                            foreach (var npc in house.Furnitures.Values)
                            {
                                npc.Send(stream);
                            }
                        }
                    }

                    foreach (var m_client in Owner.Map.View.Roles(MapObjectType.Player, playerX, playerY, null))
                    {
                        if (m_client == null) continue;

                        if (CanAdd(m_client, clear, stream) && (m_client as Role.Player).View.CanAdd(Owner.Player, true, stream))
                        {
                            var client = (m_client as Role.Player).Owner;

                            if (client.Socket?.Alive == false)
                            {
                                Owner.Map.Denquer(client);
                                ActionQuery action = new ActionQuery()
                                {
                                    ObjId = client.Player.UID,
                                    Type = ActionType.RemoveEntity
                                };
                                SendView(stream.ActionCreate(&action), false);
                                continue;
                            }

                            if (msg != null)
                                client.Send(msg);

                            client.Player.SendScrennXPSkill(Owner.Player);
                            Owner.Player.SendScrennXPSkill(client.Player);

                            if (client.Player.BlackSpot)
                                Owner.Send(stream.BlackspotCreate(true, client.Player.UID));

                            if (Owner.Player.BlackSpot)
                                client.Send(stream.BlackspotCreate(true, Owner.Player.UID));

                            if (client.Player.OnInteractionEffect)
                            {
                                client.Player.InteractionEffect.X = client.Player.X;
                                client.Player.InteractionEffect.Y = client.Player.Y;

                                var action = InteractQuery.ShallowCopy(client.Player.InteractionEffect);
                                Owner.Send(stream.InteractionCreate(&action));
                            }

                            if (Owner.Player.OnInteractionEffect)
                            {
                                Owner.Player.InteractionEffect.X = Owner.Player.X;
                                Owner.Player.InteractionEffect.Y = Owner.Player.Y;

                                var action = InteractQuery.ShallowCopy(Owner.Player.InteractionEffect);
                                client.Send(stream.InteractionCreate(&action));
                            }

                            if (Owner.IsVendor && Owner.MyVendor.HalkMeesaje != null)
                                client.Send(Owner.MyVendor.HalkMeesaje.GetArray(stream));

                            if (client.IsVendor && client.MyVendor.HalkMeesaje != null)
                                Owner.Send(client.MyVendor.HalkMeesaje.GetArray(stream));

                            if (Owner.Player.OnFairy)
                            {
                                client.Send(stream.TransformFairyCreate(Owner.Player.FairySpawn.Mode, Owner.Player.FairySpawn.FairyType, Owner.Player.FairySpawn.UID));
                            }

                            if (client.Player.OnFairy)
                            {
                                Owner.Send(stream.TransformFairyCreate(client.Player.FairySpawn.Mode, client.Player.FairySpawn.FairyType, client.Player.FairySpawn.UID));
                            }
                        }
                    }

                    foreach (var npc in Owner.Map.View.Roles(MapObjectType.Npc, Owner.Player.X, Owner.Player.Y, p => CanAdd(p, clear, stream)))
                    {
                        // NPCs.
                    }

                    foreach (var mob in Owner.Map.View.Roles(MapObjectType.Monster, Owner.Player.X, Owner.Player.Y, p => CanAdd(p, clear, stream)))
                    {
                        if (mob == null) continue;

                        var monster = mob as Game.MsgMonster.MonsterRole;
                        if (monster.HitPoints > ushort.MaxValue || monster.Boss == 1)
                        {
                            var update = new Game.MsgServer.MsgUpdate(stream, monster.UID, 2);
                            stream = update.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, monster.Family.MaxHealth);
                            stream = update.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, monster.HitPoints);
                            Owner.Send(update.GetArray(stream));
                            monster.SendScores(stream);
                        }

                        if (monster.BlackSpot)
                        {
                            Owner.Send(stream.BlackspotCreate(true, monster.UID));
                        }
                    }

                    foreach (var subnpc in Owner.Map.View.Roles(MapObjectType.SobNpc, Owner.Player.X, Owner.Player.Y, p => CanAdd(p, clear, stream)))
                    {
                        if (subnpc == null) continue;

                        var sobMobNpcs = subnpc as Role.SobNpc;
                        if (sobMobNpcs.BitVector.ArrayFlags.Count != 0)
                        {
                            var update = new Game.MsgServer.MsgUpdate(stream, subnpc.UID, 1);
                            stream = update.Append(stream, MsgUpdate.DataType.StatusFlag, sobMobNpcs.BitVector.bits);
                            Owner.Send(update.GetArray(stream));
                        }
                    }

                    foreach (var item in Owner.Map.View.Roles(MapObjectType.Item, Owner.Player.X, Owner.Player.Y, p => CanAdd(p, clear, stream)))
                    {
                        if (item is Game.MsgFloorItem.MsgItem pItem)
                            pItem.Send(stream, Owner.Player);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                }
            }
        }

        public unsafe void Clear(ServerSockets.Packet stream)
        {
            Owner.Player.Px = 0;
            Owner.Player.Py = 0;

            ActionQuery action = new ActionQuery()
            {
                ObjId = Owner.Player.UID,
                Type = ActionType.RemoveEntity
            };
            SendView(stream.ActionCreate(&action), false);
        }
    }
}
