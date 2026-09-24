using Core.Interfaces.GameServer;
using Core.Models.GameServer;
using GameServer.Database;
using GameServer.Game.MsgServer;
using System;

namespace GameServer.Game.MsgNpc
{
    using static GameServer.Game.MsgServer.MsgQuestList;
    using ActionInvoker = CachedAttributeInvocation<ProcessAction, NpcAttribute, NpcID>;
    public unsafe delegate void ProcessAction(Client.GameClient user, ServerSockets.Packet stream, byte Option, string Input, uint id);

    public class Procesor
    {
        public static ExecuteNpcInvoker ExecuteNpc = new ExecuteNpcInvoker();
        public unsafe class InvokerClient
        {
            public Client.GameClient client;
            public byte InteractType;
            public byte option;
            public string input;
            public uint npcid;

            public InvokerClient(Client.GameClient Client, ServerSockets.Packet Server_Replay, uint _npcid, byte _InteractType, byte _option, string _input)
            {
                client = Client;
                option = _option;
                InteractType = _InteractType;
                input = _input;
                npcid = _npcid;
            }
        }

        public static ActionInvoker invoker = new ActionInvoker(NpcAttribute.Translator);

        [PacketAttribute(GamePackets.NpcServerReplay)]
        private unsafe static void NpcServerReplay(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.InTrade == true || user.IsVendor || !user.Socket.Alive)
                return;
            if (!user.Player.Alive)
                return;
            if (user.PokerPlayer != null)
                return;
            uint npcid;
            ushort Mesh;
            byte option;
            byte type;
            NpcServerReplay.Mode Action;
            string input;
            //action ==6 place!
            stream.NpcDialog(out npcid, out Mesh, out option, out type, out Action, out input);
            
            if (Action == MsgNpc.NpcServerReplay.Mode.PlaceFurniture)
            {
                if (!user.Inventory.HaveSpace(1))
                {
                    user.CreateBoxDialog("Please make 1 more space your inventory.");
                    return;
                }
                Npc furniture;
                if (user.MyHouse.Furnitures.TryGetValue(npcid, out furniture))
                {
                    var npc = Database.NpcServer.GetNpcFromMesh(furniture.Mesh);
                    if (npc != null)
                    {
                        user.Inventory.Add(stream, npc.ItemID);
                        user.MyHouse.Furnitures.TryRemove(npcid, out furniture);
                        Database.ItemType.DBItem item;
                        if (Pool.ItemsBase.TryGetValue(npc.ItemID, out item))
                            user.SendSysMesage("You got a " + item.Name + "!", MsgMessage.ChatMode.System);
                        var action = new ActionQuery()
                        {
                            ObjId = npcid,
                            Type = ActionType.RemoveEntity
                        };
                        user.Send(stream.ActionCreate(&action));
                    }
                }
                return;
            }
            if (Action == MsgNpc.NpcServerReplay.Mode.Statue)
            {
                Npc furniture;
                if (user.MyHouse.Furnitures.TryGetValue(npcid, out furniture))
                {
                    user.MyHouse.Furnitures.TryRemove(npcid, out furniture);

                    var action = new ActionQuery()
                    {
                        ObjId = npcid,
                        Type = ActionType.RemoveEntity
                    };
                    user.Send(stream.ActionCreate(&action));
                }
                return;
            }
            if (option == 255)
                return;
            if (Game.Era1.Era1Services.IsBlockedNpcService(npcid, option))
            {
                user.SendSysMesage("This service is not available in Era 1.");
                return;
            }
            user.ActiveNpc = (uint)npcid;
            if (user.EditNPC)
            {
                user.EditNPCID = npcid;
                npcid = (uint)NpcID.EditNPC;
                user.ActiveNpc = (uint)npcid;
                ExecuteNpc.Enqueue(new InvokerClient(user, stream, (uint)npcid, type, option, input));
            } else
            {
                ExecuteNpc.Enqueue(new InvokerClient(user, stream, (uint)npcid, type, option, input));
            }
        }
        [PacketAttribute(GamePackets.NpcServerRequest)]
        private unsafe static void NpcServerRequest(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.InTrade == true || user.IsVendor || !user.Socket.Alive)
                return;
            if (!user.Player.Alive)
                return;
            uint npcid;
            ushort Mesh;
            byte option;
            byte type;
            NpcServerReplay.Mode Action;
            string input;

            stream.NpcDialog(out npcid, out Mesh, out option, out type, out Action, out input);

           
            if (type == (byte)NpcReply.InteractTypes.MessageBox)
            {
                if (Pool.BlockTeleportMap.Contains(user.Player.Map))
                    return;
                if (user.Player.StartMessageBox > DateTime.Now)
                {
                    if (option == 0 && user.Player.MessageOK != null)
                        user.Player.MessageOK.Invoke(user);
                    else if (user.Player.MessageCancel != null)
                        user.Player.MessageCancel.Invoke(user);
                }
                user.Player.MessageOK = null;
                user.Player.MessageCancel = null;
                return;
            }
            if (type == 102)
            {
                // Try get quest
                QuestListItem n_quest;
                uint questID = uint.Parse(npcid.ToString().Substring(npcid.ToString().Length - 1, 1));
                if (user.Player.QuestGUI.src.TryGetValue(questID, out n_quest))
                {
                    Quest qToRemove = new Quest
                    {
                        UID = n_quest.UID,
                        PlayerUID = user.Player.UID,
                        IntentionsJson = System.Text.Json.JsonSerializer.Serialize(n_quest.Intentions),
                        Status = (QuestStatus)n_quest.Status,
                        Time = n_quest.Time
                    };
                    RoleQuests.Remove(user, qToRemove);
                    user.Player.QuestGUI.SendSinglePacket(n_quest, MsgQuestList.QuestMode.QuitQuest);
                    user.Player.QuestGUI.RemoveQuest(n_quest.UID);
                }
                if (user.Player.GuildRank == Role.Flags.GuildMemberRank.GuildLeader || user.Player.GuildRank == Role.Flags.GuildMemberRank.DeputyLeader)
                {
                    if (user.Player.MyGuild != null)
                    {
                        user.Player.MyGuild.Quit(input, true, stream);
                        return;
                    }
                }
            }
            // Auto Hunt V2 dialogs are opened outside the normal NPC interaction flow.
            // Consume only replies while an Auto Hunt dialog context is active so these
            // option IDs cannot collide with ordinary NPC dialogs.
            if (user.AutoHunting != null && user.AutoHunting.DialogContext != 0)
            {
                if (Catching.HandleDialogOption(user, option))
                    return;
                user.AutoHunting.DialogContext = 0;
            }

            if (option == 255 || option == 0 || user.InTrade)
                return;
            if (user.ActiveNpc == 987977854)
            {
                switch (option)
                {
                    case 1:
                        {
                            Catching.Start(user);
                            break;
                        }
                    case 2:
                        {
                            Catching.End(user);
                            break;
                        }
                    case 3:
                        {
                            user.ActiveNpc = 987977854;
                            Dialog dialog = new Dialog(user, stream);
                            dialog.Text("You are currently collecting items in order to selected below do you want to change them. (Page 1/2)");
                            dialog.Option(user.AutoHunting.DBallsStatus + " DB Items.", 4);
                            dialog.Option(user.AutoHunting.MeteorsStatus + " Meteor.", 5);
                            dialog.Option(user.AutoHunting.QualityItemsStatus + " Quality Items.", 6);
                            dialog.Option(user.AutoHunting.ExpBallEventItemsStatus + " ExpBall/PowerExpball Items.", 7);
                            dialog.Option(user.AutoHunting.SocketedItemsStatus + " 2Socket Items.", 8);
                            dialog.Option(user.AutoHunting.BlessedItemsStatus + " Blessed Items.", 9);
                            dialog.Option("Next.", 100);
                            dialog.Option("Thanks!", 255);
                            dialog.AddAvatar(0);
                            dialog.FinalizeDialog();
                            break;
                        }
                    case 4:
                        {
                            if (user.AutoHunting.DBalls)
                                user.AutoHunting.DBalls = false;
                            else user.AutoHunting.DBalls = true;
                            break;
                        }
                    case 5:
                        {
                            if (user.AutoHunting.Meteors)
                                user.AutoHunting.Meteors = false;
                            else
                                user.AutoHunting.Meteors = true;
                            break;
                        }
                    case 6:
                        {
                            if (user.AutoHunting.QualityItems)
                                user.AutoHunting.QualityItems = false;
                            else
                                user.AutoHunting.QualityItems = true;
                            break;
                        }
                    case 7:
                        {
                            if (user.AutoHunting.ExpBallEventItems)
                                user.AutoHunting.ExpBallEventItems = false;
                            else
                                user.AutoHunting.ExpBallEventItems = true;
                            break;
                        }
                    case 8:
                        {
                            if (user.AutoHunting.SocketedItems)
                                user.AutoHunting.SocketedItems = false;
                            else
                                user.AutoHunting.SocketedItems = true;
                            break;
                        }
                    case 9:
                        {
                            if (user.AutoHunting.BlessedItems)
                                user.AutoHunting.BlessedItems = false;
                            else
                                user.AutoHunting.BlessedItems = true;
                            break;
                        }
                    case 10:
                        {
                            if (user.AutoHunting.MaterialItems)
                                user.AutoHunting.MaterialItems = false;
                            else
                                user.AutoHunting.MaterialItems = true;
                            break;
                        }
                    case 11:
                        {
                            if (user.AutoHunting.SoulItems)
                                user.AutoHunting.SoulItems = false;
                            else
                                user.AutoHunting.SoulItems = true;
                            break;
                        }
                    case 12:
                        {
                            if (user.AutoHunting.PlusItems)
                                user.AutoHunting.PlusItems = false;
                            else
                                user.AutoHunting.PlusItems = true;
                            break;
                        }
                    case 13:
                        {
                            if (user.AutoHunting.LootMoney)
                                user.AutoHunting.LootMoney = false;
                            else
                                user.AutoHunting.LootMoney = true;
                            break;
                        }
                    case 100:
                        {
                            user.ActiveNpc = 987977854;
                            Dialog dialog = new Dialog(user, stream);
                            dialog.Text("You are currently collecting items in order to selected below do you want to change them. (Page 2/2)");
                            dialog.Option(user.AutoHunting.MaterialItemsStatus + " Random Items.", 10);
                            //dialog.Option(user.AutoHunting.SoulItemsStatus + " Soul Items.", 11);
                            dialog.Option(user.AutoHunting.PlusItemsStatus + " Plus Items.", 12);
                            dialog.Option(user.AutoHunting.LootMoneyStatus + " Pickup Money.", 13);
                            dialog.Option("Thanks!", 255);
                            dialog.AddAvatar(0);
                            dialog.FinalizeDialog();
                            break;
                        }
                    case 150:
                        {
                            Dialog dialog = new Dialog(user, stream);
                            user.AutoHunting.FastMode = !user.AutoHunting.FastMode;
                            dialog.Text($"You have now {(user.AutoHunting.FastMode ? "Enabled" : "Disabled")} the FastMode. Created by DaRkFox and it is experimental!");
                            dialog.Option("Thanks!", 255);
                            dialog.AddAvatar(0);
                            dialog.FinalizeDialog();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                return;
            }
            if (user.ActiveNpc == 9999997 && user.Player.WaitingKillCaptcha)
            {
                if (option == 255) return;
                if (input == user.Player.KillCountCaptcha)
                {
                    user.Player.SolveCaptcha();
                }
                else
                {
                    Dialog dialog = new Dialog(user, stream);
                    dialog.Text("Input the current text: " + user.Player.KillCountCaptcha + " to verify your humanity.");
                    dialog.AddInput("Captcha message:", (byte)user.Player.KillCountCaptcha.Length);
                    dialog.Option("No thank you.", 255);
                    dialog.AddAvatar(39);
                    dialog.FinalizeDialog();
                }
                return;
            }
            npcid = (uint)user.ActiveNpc;
            if (Game.Era1.Era1Services.IsBlockedNpcService(npcid, option))
            {
                user.SendSysMesage("This service is not available in Era 1.");
                return;
            }
            ExecuteNpc.Enqueue(new InvokerClient(user, stream, (uint)npcid, type, option, input));
        }
        public class ExecuteNpcInvoker : ConcurrentSmartThreadQueue<InvokerClient>
        {
            public ExecuteNpcInvoker()
                : base(3)
            {
                Start(5);
            }
            public void TryEnqueue(InvokerClient action)
            {
                Enqueue(action);
            }
            protected unsafe override void OnDequeue(InvokerClient action, int time)
            {
                try
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        using var _econScope = GameServer.Telemetry.Economy.Scope(GameServer.Telemetry.SourceKind.Npc, (uint)action.npcid, action.option);

                        if (!action.client.Player.OnMyOwnServer)
                        {
                            if (action.npcid == (ushort)NpcID.KingdomMissionEnvoy
                                || action.npcid == (ushort)NpcID.RealmEnvoy)
                            {
                                Tuple<NpcAttribute, ProcessAction> processFolded;
                                if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                    processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                            }
                            else if (action.client.Player.Map == 3935 || action.client.Player.Map == Game.MsgTournaments.MsgEliteGroup.WaitingAreaID)
                            {
                                 Game.MsgNpc.Npc _obj;
                                if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out _obj))
                                {
                                    if (action.client.ProjectManager)
                                        action.client.SendSysMesage("Active Npc [" + action.npcid + "] X[" + _obj.X + "] Y[" + _obj.Y + "]", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

                                    if (action.npcid >= (ushort)NpcID.Crystal1 && action.npcid <= (ushort)NpcID.Crystal5)
                                    {
                                        NpcHandler.KingDoomCrystals(action.client, stream, action.option, action.input, action.npcid);
                                        return;
                                    }

                                    Tuple<NpcAttribute, ProcessAction> processFolded;
                                    if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                    {
                                        processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                                    }
                                }
                                else
                                {
                                    Role.IMapObj inpc;
                                    if (action.client.Player.View.TryGetValue((uint)action.npcid, out inpc, Role.MapObjectType.SobNpc))
                                    {
                                        var npc = inpc as Role.SobNpc;
                                        Tuple<NpcAttribute, ProcessAction> processFolded;
                                        if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                            processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);

                                    }
                                }
                            }
                            return;
                        }
                        // Era 1 hard gate: post-classic promotion NPCs must never execute,
                        // even if a stale client/database entry still exposes one.
                        if (action.npcid == (uint)NpcID.PromotionNinja
                            || action.npcid == (uint)NpcID.PromotionMonk
                            || action.npcid == (uint)NpcID.PromotionPirate
                            || action.npcid == (uint)NpcID.PromotionLeeLong
                            || action.npcid == (uint)NpcID.PromotionWindWalker)
                        {
                            action.client.SendSysMesage("This profession is not available in Era 1.");
                            return;
                        }

                        // Later-patch convenience route: the Equipment Blacksmith offers
                        // instant level-to-140 upgrades for CPs in options 61-68.
                        // Keep the classic Meteor/DragonBall paths, but reject this shortcut
                        // even if an old or modified client invokes the option directly.
                        if (action.npcid == (uint)NpcID.EquipmentBlacksmith
                            && action.option >= 61 && action.option <= 68
                            && !Game.Era1.Era1Economy.EnableDirectLevelUpgradeWithCps)
                        {
                            action.client.SendSysMesage("Direct equipment level upgrade with CPs is not available in Era 1.");
                            return;
                        }

                        if (action.InteractType == (byte)NpcReply.InteractTypes.MessageBox)
                        {
                            if (action.client.Player.StartMessageBox > DateTime.Now)
                            {
                                if (action.option == 255 && action.client.Player.MessageOK != null)
                                    action.client.Player.MessageOK.Invoke(action.client);
                                else if (action.client.Player.MessageCancel != null)
                                    action.client.Player.MessageCancel.Invoke(action.client);
                            }
                            action.client.Player.MessageOK = null;
                            action.client.Player.MessageCancel = null;
                            return;
                        }
                        if ((uint)action.npcid == 3124)//house WH
                        {
                            if (action.client.MyHouse != null && action.client.Player.DynamicID == action.client.Player.UID)
                            {
                                ActionQuery query = new ActionQuery()
                          {
                              Type = ActionType.OpenDialog,
                              ObjId = action.client.Player.UID,
                              dwParam = MsgServer.DialogCommands.Warehouse,
                              wParam1 = action.client.Player.X,
                              wParam2 = action.client.Player.Y
                          };
                                action.client.Send(stream.ActionCreate(&query));

                                return;
                            }
                            else
                            {
                                action.client.SendSysMesage("I'm sorry but you dont own this house !");                               
                            }
                        }
                        if (action.client.Player.Map == 1038)//Guild War
                        {
                            Role.IMapObj inpc;
                            if (action.client.Player.View.TryGetValue((uint)action.npcid, out inpc, Role.MapObjectType.SobNpc))
                            {
                                var npc = inpc as Role.SobNpc;
                                Tuple<NpcAttribute, ProcessAction> processFolded;
                                if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                    processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                                return;
                            }

                        }
                        if (action.npcid >= 7832 && action.npcid <= 7840)
                        {
                            Game.MsgNpc.Npc _obj;
                            if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out _obj))
                            {
                                action.client.OnRemoveNpc = _obj;
                                NpcHandler.CheckDesertGuardian(action.client, stream, action.option, action.input, action.npcid);

                            }
                            return;

                        }
                        if (action.npcid >= 8546 && action.npcid <= 8550)
                        {
                            Game.MsgNpc.Npc _obj;
                            if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out _obj))
                            {
                                action.client.OnRemoveNpc = _obj;
                                NpcHandler.SoldierBird(action.client, stream, action.option, action.input, action.npcid);

                            }
                            return;

                        }
                        if (action.npcid >= 8551 && action.npcid <= 8555)
                        {
                            Game.MsgNpc.Npc _obj;
                            if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out _obj))
                            {
                                action.client.OnRemoveNpc = _obj;
                                NpcHandler.BandittiFlowers(action.client, stream, action.option, action.input, action.npcid);

                            }
                            return;

                        }
                        if (action.npcid == (uint)NpcID.SelectSacredRefineryPack || action.npcid == (uint)NpcID.SelectP7WeaponSoulPack
                            || action.npcid == (uint)NpcID.SelectP7EquipmentSoulPack
                            || action.npcid == (uint)NpcID.Steed1
                            || action.npcid == (uint)NpcID.Steed3
                            || action.npcid == (uint)NpcID.Steed6 || action.npcid == (uint)NpcID.DailyItem1
                            || action.npcid == (uint)NpcID.DailyEliteSpiritBead
                            || action.npcid == (uint)NpcID.DailyNormalSpiritBead
                            || action.npcid == (uint)NpcID.DailyRefinedSpiritBead
                            || action.npcid == (uint)NpcID.DailyUniqueSpiritBead
                            || action.npcid == (uint)NpcID.DailySuperSpiritBead
                            || action.npcid == (uint)NpcID.Level43UniqueRingPack
                            || action.npcid == (uint)NpcID.NobleSteedPack
                            || action.npcid == (uint)NpcID.ChiToken
                            || action.npcid == (uint)NpcID.DazzlingDiamondBox
                            || action.npcid == (uint)NpcID.RareSteedPack6
                            || action.npcid == (uint)NpcID.TempestSecretLetter
                            || action.npcid == (uint)NpcID.SashFragment_Realm
                            || action.npcid == (uint)NpcID.GarmentPacket
                            || action.npcid == (uint)NpcID.GarmentPacket2
                            || action.npcid == (uint)NpcID.MountPacket
                            || action.npcid == (uint)NpcID.MountPacket2
                            || action.npcid == (uint)NpcID.AccesoryPacket
                            || action.npcid == (uint)NpcID.AccesoryPacket2
                            || action.npcid == (uint)NpcID.MountPacket3
                            || action.npcid == (uint)NpcID.GoldPrizeToken
                            || action.npcid == (uint)NpcID.SuperGuildToken
                            || action.npcid == (uint)NpcID.SuperClanToken
                            || action.npcid == (uint)NpcID.BlackFridayGarmentPack
                            || action.npcid == (uint)NpcID.BlackFridayMountPack || action.npcid == (uint)NpcID.BlackFridayAccesory
                            || action.npcid == (uint)NpcID.Steed1Pack
                            || action.npcid == (uint)NpcID.Steed3Pack
                            || action.npcid == (uint)NpcID.HeavenDemonBox
                            || action.npcid == (uint)NpcID.ChaosDemonBox
                            || action.npcid == (uint)NpcID.SacredDemonBox
                            || action.npcid == (uint)NpcID.AuroraDemonBox
                            || action.npcid == (uint)NpcID.DemonBox
                            || action.npcid == (uint)NpcID.AncientDemonBox
                            || action.npcid == (uint)NpcID.FloodDemonBox
                            || action.npcid == (uint)NpcID.MrMirror2
                            || action.npcid == (uint)NpcID.SuperHeadgearPack || action.npcid == (uint)NpcID.RingPack
                            || action.npcid == (uint)NpcID.ClothingPack || action.npcid == (uint)NpcID.PowerBook
                            || action.npcid == (uint)NpcID.Level50UniqueWeaponPack
                            || action.npcid == (uint)NpcID.Level52UniqueHeadgearPack
                            || action.npcid == (uint)NpcID.Level55EliteWeaponPack
                            || action.npcid == (uint)NpcID.Level67EliteHeadgearPack
                            || action.npcid == (uint)NpcID.VIPBook
                            || action.npcid == (uint)NpcID.L60UniqueGearPack
                            || action.npcid == (uint)NpcID.EditNPC)
                        {
                            Tuple<NpcAttribute, ProcessAction> processFolded;
                            if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                            return;
                        }
                        Game.MsgNpc.Npc obj;
                        if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out obj))
                        {
                            if (action.client.ProjectManager)
                                action.client.SendSysMesage("Active Npc [" + action.npcid + "] X[" + obj.X + "] Y[" + obj.Y + "]", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                            if (action.client.Player.Map == 5263)
                            {
                                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.TreasureThief)
                                {
                                    var tournament = Game.MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgTreasureThief;
                                    tournament.Reward(action.client, obj, stream);
                                    return;
                                }
                            }
                            if (action.client.Player.Map == 1511)
                            {
                                NpcHandler.Furnitures(action.client, stream, action.option, action.input, action.npcid);
                                return;
                            }
                            Tuple<NpcAttribute, ProcessAction> processFolded;
                            if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                            {
                                processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                            }
                            else
                            {

                                if (action.client.Player.Map == 1038)
                                {
                                    if (action.npcid == (uint)NpcID.GuildConductor1 || action.npcid == (uint)NpcID.GuildConductor2
                                        || action.npcid == (uint)NpcID.GuildConductor3 || action.npcid == (uint)NpcID.GuildConductor4)
                                        NpcHandler.GuildConductorsProces(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else if (((int)action.npcid >= 10031 && (int)action.npcid <= 10041 || (int)action.npcid == 10043) && action.client.Player.DynamicID == 0)
                                {
                                    NpcHandler.SpaceMarks(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else if (action.npcid == (uint)NpcID.TeleGuild1 || action.npcid == (uint)NpcID.TeleGuild2
                                   || action.npcid == (uint)NpcID.TeleGuild3 || action.npcid == (uint)NpcID.TeleGuild4)
                                {
                                    NpcHandler.GuildCondTeleBack(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else if (action.npcid == (uint)NpcID.WHTwin || action.npcid == (uint)NpcID.wHPheonix
                                   || action.npcid == (uint)NpcID.WHMarket || action.npcid == (uint)NpcID.WHBird
                                   || action.npcid == (uint)NpcID.WHDesert || action.npcid == (uint)NpcID.WHApe
                                    || action.npcid == (uint)NpcID.WHPoker)
                                {
                                    NpcHandler.Warehause(action.client, stream, action.option, action.input, action.npcid);
                                }

                                else if ((int)action.npcid >= 925 && (int)action.npcid <= 930 && action.client.Player.Map == 700 && action.client.Player.DynamicID == 0)
                                {
                                    NpcHandler.LotteryBoxes(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else
                                {
                                    if (action.client.ProjectManager)
                                        Console.WriteLine("Not find Npc -> " + action.npcid + " ");
                                }
                            }
                        }
                        else if (action.npcid == 12)
                        {
                            if (action.client.Player.VipLevel > 0)
                            {

                                ActionQuery query = new ActionQuery()
                                {
                                    Type = ActionType.OpenDialog,
                                    ObjId = action.client.Player.UID,
                                    dwParam = MsgServer.DialogCommands.VIPWarehouse,
                                    wParam1 = action.client.Player.X,
                                    wParam2 = action.client.Player.Y
                                };
                                action.client.Send(stream.ActionCreate(&query));



                            }
                        }
                        //else if (Pool.UmbralTree != null && action.npcid == Pool.UmbralTree.ID)
                        //{
                        //    Pool.UmbralTree.Talk(action.client);
                        //}
                        else
                        {
                            Role.IMapObj inpc;
                            if (action.client.Player.View.TryGetValue((uint)action.npcid, out inpc, Role.MapObjectType.SobNpc))
                            {
                                var npc = inpc as Role.SobNpc;
                                Tuple<NpcAttribute, ProcessAction> processFolded;
                                if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                {
                                    processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                                }
                          
                            }
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
    }
}
