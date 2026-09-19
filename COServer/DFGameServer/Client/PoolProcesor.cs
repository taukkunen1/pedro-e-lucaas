using Core;
using GameServer.Database;
using GameServer.Game.MsgFloorItem;
using GameServer.Game.MsgServer;
using GameServer.Game.MsgTournaments;
using GameServer.MadeByDaRkFox;
using System;
using System.Linq;

namespace GameServer.Client
{
    public class PoolProcesses
    {
        public static unsafe void ItemsCallBack(Client.GameClient client/*, int time*/)
        {
            try
            {
                if (client == null || !client.FullLoading || client.Player == null)
                    return;

                DateTime Now = DateTime.Now;
                foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                {
                    if (item.Alive == false)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            var PItem = item as Game.MsgFloorItem.MsgItem;
                            if (PItem.IsTrap())
                            {
                                PItem.SendAll(stream, MsgDropID.RemoveEffect);
                            }
                            else
                                PItem.SendAll(stream, MsgDropID.Remove);
                            client.Map.View.LeaveMap<Role.IMapObj>(PItem);
                        }
                    }
                    else if (item.IsTrap())
                    {
                        var FloorItem = item as Game.MsgFloorItem.MsgItem;
                        if (FloorItem.ItemBase == null)
                            continue;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public static unsafe void SecondsCallback(Client.GameClient client)
        {
            try
            {
                if (client == null || !client.FullLoading || client.Player == null || client.Player.CompleteLogin == false)
                    return;

                DateTime timer = DateTime.Now;
                #region Set PK Flags
                if (client.Player.PKPoints > 99)
                {
                    client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.RedName);
                    client.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.BlackName, Role.StatusFlagsBigVector32.PermanentFlag, false, 6 * 60);
                }
                else if (client.Player.PKPoints > 29)
                {
                    client.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.RedName, Role.StatusFlagsBigVector32.PermanentFlag, false, 6 * 60);
                    client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackName);
                }
                else if (client.Player.PKPoints < 30)
                {
                    client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.RedName);
                    client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackName);
                }
                if (client.Player.LastPKPoints != client.Player.PKPoints)
                {
                    client.Player.LastPKPoints = client.Player.PKPoints;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendUpdate(stream, client.Player.PKPoints, Game.MsgServer.MsgUpdate.DataType.PKPoints);
                    }
                }
                #endregion
                #region No horse allowed maps
                if (client.Player.Map == 5051 || client.Player.Map == 1005 || client.Player.Map == 501 || client.Player.Map == 26392 || client.Player.Map == 1860 || client.Player.Map == 1858 || client.Player.Map == 700 || client.Player.Map == 1004 || Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                {
                    if (client.Player.ContainFlag(MsgUpdate.Flags.Ride))
                    {
                        client.Player.RemoveFlag(MsgUpdate.Flags.Ride);
                    }
                }
                #endregion
                #region Nobility System
                if (DateTime.Now >= client.Player.PayNobilitySystem.PeriodTime && client.Player.PayNobilitySystem.IsActive)
                {
                    client.Player.Nobility = new Role.Instance.Nobility(client);
                    client.Player.Nobility.Donation = client.Player.PayNobilitySystem.LastNobilityDonation;
                    Pool.NobilityRanking.UpdateRank(client.Player.Nobility);
                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                    client.Player.PayNobilitySystem.SetDataAnyWay(0, 0, 0, 0, false);
                }
                #endregion
                #region Squama System
                foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                {
                    var RoleItem = item as MsgItem;
                    if (RoleItem.MsgFloor.m_ID == 11 && client.Player.X == RoleItem.X && client.Player.Y == RoleItem.Y)
                    {
                        SquamaLocation sqLocation = SquamaManager.SquamaConfigurations.Find(x => x.MapId == client.Player.Map && x.MapX == RoleItem.X && x.MapY == RoleItem.Y);
                        if (sqLocation != null)
                        {
                            SquamaReward sqReward = null;
                            SquamaReward sqRewardRate = sqLocation.Rewards.Find((x) =>
                            {
                                return Utils.Rate(x.RewardPercentage);
                            }
                            );
                            if (sqRewardRate != null)
                            {
                                sqReward = sqRewardRate;
                            } else
                            {
                                    sqReward = sqLocation.Rewards.Find((x) =>
                                    {
                                        return x.IsDefaultReward;
                                    }
                               );
                            }
                            // TODO hay que guardar en el Server de forma global k este trap ya se desactivado para que no se reclame mas de 1 vez, tambien hacer reset de el estado a las 21:00 creo k era
                            if (sqReward != null)
                            {
                                sqLocation.Claimed = true;
                                switch (sqReward.RewardType)
                                {
                                    case SquamaLocationRewardType.Gold:
                                        {
                                            client.Player.Money += sqReward.RewardValue;
                                            client.CreateBoxDialog($"You get {sqReward.RewardValue} of Gold.");
                                            break;
                                        }
                                    case SquamaLocationRewardType.ConquerPoints:
                                        {
                                            client.Player.ConquerPoints += sqReward.RewardValue;
                                            client.CreateBoxDialog($"You get {sqReward.RewardValue} of ConquerPoints.");
                                            break;
                                        }
                                    case SquamaLocationRewardType.BoundConquerPoints:
                                        {
                                            client.Player.BoundConquerPoints += (int)sqReward.RewardValue;
                                            client.CreateBoxDialog($"You get {sqReward.RewardValue} of BoundConquerPoints.");
                                            break;
                                        }
                                    case SquamaLocationRewardType.Item:
                                        {
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();
                                                client.Inventory.AddItemWitchStack(sqReward.RewardValue, 0, 1, stream);
                                                ItemType.DBItem DBItem;
                                                if (Pool.ItemsBase.TryGetValue(sqReward.RewardValue, out DBItem))
                                                {
                                                    client.CreateBoxDialog($"You get {DBItem.Name}.");
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        RoleItem.Map = 5000;
                        RoleItem.X = 0;
                        RoleItem.Y = 0;
                    }
                }
                #endregion
                if (client.Player.Map == 601)
                {
                    if (!client.Map.ValidLocation(client.Player.X, client.Player.Y))
                    {
                        client.Teleport(64, 56, 601);
                    }
                }
                if(Game.MsgTournaments.MsgFortressWar.Proces == ProcesType.Dead)
                {
                    if(client.Player.Map == 501)
                    {
                        client.Teleport(428, 378, 1002);
                    }
                }
                if (client.Player.Map == 44463)
                {
                    if (timer > client.Player.EarthStamp.AddSeconds(10))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Game.MsgFloorItem.MsgItemPacket effect = Game.MsgFloorItem.MsgItemPacket.Create();
                            effect.m_UID = (uint)Game.MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeLeftRight;
                            effect.DropType = MsgDropID.Earth;
                            effect.m_X = client.Player.X;
                            effect.m_Y = client.Player.Y;
                            client.Send(stream.ItemPacketCreate(effect));
                        }
                        client.Player.EarthStamp = DateTime.Now;
                    }
                }
                if (client.Player.ExpProtection > 0)
                    client.Player.ExpProtection -= 1;
                if (DateTime.Now > client.Player.ExpireVip)
                {
                    if (client.Player.EnabledTitles == "11")
                    {
                        client.Player.EnabledTitles = "";
                        client.Player.Titles.Remove(11);
                        client.Player.SwitchTitle(0);
                    }
                    if (client.Player.VipLevel > 1)
                    {
                        client.Player.VipLevel = 0;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendUpdate(stream, client.Player.VipLevel, Game.MsgServer.MsgUpdate.DataType.VIPLevel);
                            client.Player.UpdateVip(stream);
                        }
                    }
                } else
                {
                    if (!client.Player.Titles.Contains(11))
                    {
                        client.Player.AddTitle(11, true);
                        client.SendSysMesage("You got your VIP title!");
                    }
                }
                if (client.Player.Map == 1768)
                {
                    if (client.Player.QuestGUI.CheckQuest(1785, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                    {
                        if (DateTime.Now > client.Player.TaskQuestTimer)
                        {
                            client.SendSysMesage("You fainted and woke up find you are in Ape City. You need to try more toxins to go Kun Lun.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Inventory.Remove(729983, 1, stream);
                                client.Inventory.Remove(729984, 1, stream);
                                client.Inventory.Remove(729985, 1, stream);
                                client.Inventory.Remove(729986, 1, stream);
                                client.Inventory.Remove(729987, 1, stream);
                                client.Inventory.Remove(729988, 1, stream);
                                client.Inventory.Remove(729989, 1, stream);
                            }
                            client.Teleport(55, 55, 1004);
                        }
                    }
                }

                if (client.Player.Map == 1011)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 471, 844) <= 5 && client.Inventory.Contain(721799, 1))
                    {
                        client.Teleport(80, 39, 1792);

                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 643, 622) < 18)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1133, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1133, 1);
                            }
                        }
                    }

                }
                if (client.Player.Map == 44457)
                {

                    if (Role.Core.GetDistance(128, 36, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("You followed the track of the Elder Power and found him on a road leading to the tomb tunnel. Hit the road now?",
                                  new Action<Client.GameClient>(p =>
                                  {
                                      p.Teleport(134, 93, 10089);
                                      p.Player.QuestGUI.SendAutoPatcher("There are too many devil claws here. Try your best to break through and find the Elder Power.", 10089, 28, 89, 0);
                                      p.SendSysMesage("There are too many devil claws here. Try your best to break through and find the Elder Power.");
                                  }), null);
                    }
                }
                else if (client.Player.Map == 10090)
                {
                    if (Role.Core.GetDistance(361, 314, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("The Elder Power is seriously injured. Are you sure you want to leave him alone?",
                             new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                }
                else if (client.Player.Map == 44460)
                {
                    if (Role.Core.GetDistance(361, 314, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("The Elder Power is seriously injured. Are you sure you want to leave him alone?",
                             new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                }
                else if (client.Player.Map == 44461)
                {
                    if (Role.Core.GetDistance(361, 314, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("The Elder Power is seriously injured. Are you sure you want to leave him alone?",
                             new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                }
                else if (client.Player.Map == 44462)
                {
                    if (Role.Core.GetDistance(361, 314, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("The Elder Power is seriously injured. Are you sure you want to leave him alone?",
                             new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                }
                else if (client.Player.Map == 44463)
                {
                    if (Role.Core.GetDistance(361, 314, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("The Elder Power is seriously injured. Are you sure you want to leave him alone?",
                             new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                    if (Role.Core.GetDistance(58, 164, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("You`re very close to the tunnel exit. Exit the tunnel now?",
                             new Action<Client.GameClient>(p =>
                             {
                                 p.Teleport(253, 406, 1002);

                                 p.Player.QuestGUI.FinishQuest(3801);
                                 var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)20032, p.Player.Class, 3802);
                                 p.Player.QuestGUI.Accept(ActiveQuest, 0);
                                 p.Player.QuestGUI.SendAutoPatcher("You`ve successfully escaped before the mausoleum completely collapsed. Hurry and report back to the Windwalker Lord in Twin City.", ActiveQuest.FinishNpcId.Map, ActiveQuest.FinishNpcId.X, ActiveQuest.FinishNpcId.Y, ActiveQuest.FinishNpcId.ID);
                                 unsafe
                                 {
                                     using (var rec = new ServerSockets.RecycledPacket())
                                     {
                                         var pstream = rec.GetStream();
                                         var action = new ActionQuery()
                                         {
                                             ObjId = client.Player.UID,
                                             Type = ActionType.DrawStory,
                                             dwParam = 1011

                                         };
                                         p.Send(pstream.ActionCreate(&action));
                                     }
                                 }


                             }), null);
                    }
                    foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                    {
                        var RoleItem = item as Game.MsgFloorItem.MsgItem;
                        if (RoleItem.MsgFloor.m_ID == 1616)
                        {
                            if (Role.Core.GetDistance(client.Player.X, client.Player.Y, RoleItem.X, RoleItem.Y) <= 7)
                            {
                                if (Role.Core.Rate(50))
                                {
                                    client.CreateBoxDialog("You carelessly stepped into a sand pit and can move very slow.");
                                    client.Player.AddFlag(MsgUpdate.Flags.Deceleration, 2, true, 0, 40, 39);
                                }
                                else
                                {
                                    client.CreateBoxDialog("You can hardly move forward in the intensive quake.");
                                    client.Player.AddFlag(MsgUpdate.Flags.Deceleration, 2, true, 0, 40, 46);
                                }
                            }
                        }
                    }
                }
                else if (client.Player.Map == 10089)
                {
                    if (Role.Core.GetDistance(136, 89, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("The tomb tunnel is in front of you and the Elder Power must be there now. Enter the tunnel now?",
                             new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                    if (Role.Core.GetDistance(28, 89, client.Player.X, client.Player.Y) <= 2)
                    {


                        client.Player.MessageBox("You haven`t found the Elder Power. Are you sure you want to return to Wind Plain now?",
                                new Action<Client.GameClient>(p =>
                                {
                                    p.Teleport(359, 312, 10090);
                                    p.Player.QuestGUI.SendAutoPatcher("You found the Elder Power sitting in the tomb tunnel. It seems he has been seriously injured.", 10090, 350, 285, (uint)Game.MsgNpc.NpcID.ElderPower2);
                                    p.SendSysMesage("You found the Elder Power sitting in the tomb tunnel. It seems he has been seriously injured.");
                                }), null);
                    }
                    foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                    {
                        var RoleItem = item as Game.MsgFloorItem.MsgItem;
                        if (RoleItem.MsgFloor.m_ID == 24)
                            continue;
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, RoleItem.X, RoleItem.Y) <= 7)
                        {
                            client.Player.AddFlag(MsgUpdate.Flags.Deceleration, 2, true, 0, 40, 39);
                        }
                    }
                }
                else if (client.Player.Map == 10088 || client.Player.Map == 44457
                    || client.Player.Map == 44456 || client.Player.Map == 44455)
                {
                    if (Role.Core.GetDistance(192, 151, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Player.MessageBox("Your school camp was suddenly attacked by the devil force and is in critical condition. Are you sure you want to leave now?",
                                  new Action<Client.GameClient>(p => p.Teleport(334, 625, 1002)), null);
                    }
                }
                else if (client.Player.Map == 1787)
                {
                    if (Role.Core.GetDistance(83, 75, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Teleport(126, 73, 1786);
                        client.CreateBoxDialog("You arrived at Dungeon 2F.");
                    }
                }
                else if (client.Player.Map == 1786)
                {
                    if (Role.Core.GetDistance(122, 67, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Teleport(105, 108, 1785);
                        client.CreateBoxDialog("You arrived at Dungeon 1F.");
                    }
                    else if (Role.Core.GetDistance(43, 65, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Teleport(87, 74, 1787);
                        client.CreateBoxDialog("You arrived at Dungeon 3F.");
                    }
                }
                else if (client.Player.Map == 1785)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 74, 49) <= 2)
                    {
                        if (client.Inventory.Contain(721784, 1))
                        {
                            if (client.Inventory.Contain(721788, 1))
                            {
                                client.Teleport(126, 73, 1786);
                                client.CreateBoxDialog("You arrived at Dungeon 2F.");
                            }
                            else
                            {
                                client.CreateBoxDialog("You still need to collect the Glitter Sword and the Annatto Blade. You can get them from the Mausoleum General.");
                            }
                        }
                        else
                        {
                            client.CreateBoxDialog("You still need to collect the Glitter Sword and the Annatto Blade. You can get them from the Mausoleum General.");
                        }
                    }
                }
                else if (client.Player.Map == 1783)
                {
                    if (client.Player.QuestGUI.CheckQuest(526, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                    {
                        foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                        {
                            var RoleItem = item as Game.MsgFloorItem.MsgItem;
                            if (Role.Core.GetDistance(client.Player.X, client.Player.Y, RoleItem.X, RoleItem.Y) <= 2)
                            {
                                if (RoleItem.MsgFloor.m_ID == 11)
                                {
                                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Poisoned))
                                    {
                                        client.CreateBoxDialog("Oops! You are caught in a trap and were seriously poisoned.");
                                        client.Player.AddFlag(MsgUpdate.Flags.Poisoned, 60, true, 3);
                                    }
                                    else
                                        client.CreateBoxDialog("The poison in the trap doesn`t have any impact on you, as you have already been poisoned.");
                                }
                                else if (RoleItem.MsgFloor.m_ID == 18)
                                {
                                    if (client.Player.QuestGUI.CheckObjectives(526, 20))
                                    {
                                        if (client.Inventory.HaveSpace(1))
                                        {
                                            client.Teleport(94, 333, 1001);
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var msg = rec.GetStream();
                                                client.Inventory.Add(msg, 721786, 1);
                                            }
                                            client.CreateBoxDialog("You got a Weird Invocation from nowhere. Ghoul Kong (85,313), at the entrance to the Dungeon, may be of some help.");

                                        }
                                        else
                                        {
                                            client.CreateBoxDialog("Your inventory is full. You can`t take the Invocation.");
                                        }
                                    }
                                    else
                                        client.CreateBoxDialog("You need to kill 20 Vicious Rats.");
                                }
                            }
                        }
                    }
                }

                else if (client.Player.Map == 1001)
                {

                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 88, 279) <= 3)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1829, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.Inventory.Contain(721876, 1))
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Inventory.Remove(721876, 1, msg);
                                        if (Role.Core.Rate(30))
                                        {
                                            client.Player.QuestGUI.SetQuestObjectives(msg, 1829, 1);
                                            client.Inventory.Add(msg, 721870);
                                            client.CreateBoxDialog("You used the key and unlocked the compartment. There it is, the Scripture Box.");
                                        }
                                        else
                                        {
                                            client.CreateBoxDialog("Oh, the compartment clicked once but the Key disappeared. You`d better go kill some more Tomb Bats and get another key.");
                                        }
                                        if (client.Inventory.Contain(721876, 1) == false)
                                        {
                                            client.Player.QuestGUI.SetQuestObjectives(msg, 1829, 0);
                                        }
                                    }
                                }
                                else
                                    client.CreateBoxDialog("Please make 1 more space in your inventory.");
                            }
                            else
                                client.CreateBoxDialog("A key for a compartment came out of the Tomb Bat`s body. You need the key to unlock the compartment.");
                        }
                    }
                }

                if (client.Player.Map == 4000)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 40, 66) <= 2)
                        client.Player.MessageBox("Do you want to leave the Tower of Mystery?.",
                                  new Action<Client.GameClient>(p => p.Teleport(83, 74, 4020)), null);
                }
                else if (client.Player.Map == 4003)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 42, 64) <= 2)
                        client.Player.MessageBox("Do you want to leave the Tower of Mystery?.",
                                  new Action<Client.GameClient>(p => p.Teleport(83, 74, 4020)), null);
                }
                else if (client.Player.Map == 4006)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 44, 62) <= 2)
                        client.Player.MessageBox("Do you want to leave the Tower of Mystery?.",
                                  new Action<Client.GameClient>(p => p.Teleport(83, 74, 4020)), null);
                }
                else if (client.Player.Map == 4008)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 46, 68) <= 2)
                        client.Player.MessageBox("Do you want to leave the Tower of Mystery?.",
                                  new Action<Client.GameClient>(p => p.Teleport(83, 74, 4020)), null);
                }
                else if (client.Player.Map == 4009)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 46, 44) <= 2)
                        client.Player.MessageBox("Do you want to leave the Tower of Mystery?.",
                                  new Action<Client.GameClient>(p => p.Teleport(83, 74, 4020)), null);
                }

                if (client.Player.Map == 4020)
                {
                    if (Role.Core.GetDistance(73, 98, client.Player.X, client.Player.Y) <= 2)
                    {
                        client.Teleport(78, 349, 3998);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var msg = rec.GetStream();
                            client.Player.SendString(msg, MsgStringPacket.StringID.Effect, true, "movego");
                        }
                    }
                }
                if (client.Player.Map == 3998)
                {
                    if (client.Player.QuestGUI.CheckQuest(3641, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                    {

                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 220, 294) <= 3)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.ChingYan, client.Player.Class, 3641);
                                client.Inventory.Remove(3200344, 1, msg);
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 3641, 0, 1);
                                client.Player.QuestGUI.SendAutoPatcher("You appease the sacrificed Bright people.Hurry and claim the reward!", ActiveQuest.FinishNpcId.Map, ActiveQuest.FinishNpcId.X, ActiveQuest.FinishNpcId.Y, ActiveQuest.FinishNpcId.ID);
                            }
                        }
                    }
                    if (client.Player.X <= 110)
                    {
                        foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                        {
                            if (Role.Core.GetDistance(item.X, item.Y, client.Player.X, client.Player.Y) <= 2)
                            {
                                client.Map.RemoveTrap(item.X, item.Y, item);
                                if (client.Inventory.Contain(3008993, 1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Player.SendString(msg, MsgStringPacket.StringID.Effect, true, "accession3");
                                        if (client.Inventory.HaveSpace(1))
                                        {
                                            if (Role.Core.Rate(60))
                                            {
                                                client.Inventory.Remove(3008993, 1, msg);
                                                client.Inventory.AddItemWitchStack(3008992, 0, 1, msg);
                                                client.CreateBoxDialog("The earth was split apart, with a flash of golden light burst out, and you received a Treasure of Dragon.");
                                            }
                                            else
                                            {
                                                client.CreateBoxDialog("The earth was split apart, but you got nothing inside. Go and check another spot.");
                                            }

                                        }
                                        else
                                        {
                                            client.CreateBoxDialog("You need to make some room in your inventory before you can continue the adventure.");
                                        }
                                    }
                                }
                                else
                                {


                                    client.Player.MessageBox("You felt something strange under the ground. Maybe, the Chief`s Hunting Amulet can clear your confusion.",
                                       new Action<Client.GameClient>(p =>
                                       {
                                           p.Teleport(78, 349, 3998);
                                           using (var rec = new ServerSockets.RecycledPacket())
                                           {
                                               var pstream = rec.GetStream();
                                               client.Player.SendString(pstream, MsgStringPacket.StringID.Effect, true, "moveback");
                                           }
                                       }), null);
                                }
                                break;
                            }

                        }
                    }
                }

                if (client.Player.Map == 1015)
                {


                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 362, 401) <= 1)
                    {
                        if (client.Player.QuestGUI.CheckQuest(522, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.Player.QuestGUI.CheckObjectives(522, 3))
                            {
                                client.Teleport(63, 102, 1784);
                                client.CreateBoxDialog("You arrived at the Hut.");
                                client.Player.QuestGUI.FinishQuest(522);
                                var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.TimeDoor, client.Player.Class, 523);
                                client.Player.QuestGUI.Accept(ActiveQuest, 0);
                            }
                            else
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var msg = rec.GetStream();
                                    client.CreateDialog(msg, "You need to break open all of the stone doors. ALL of them!", "I~heard~you~the~first~time.");
                                }
                            }
                        }
                        else if (client.Player.QuestGUI.CheckQuest(522, MsgQuestList.QuestListItem.QuestStatus.Finished))
                        {
                            client.Teleport(63, 102, 1784);
                            client.CreateBoxDialog("You arrived at the Hut.");
                        }
                        else
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.CreateDialog(msg, "You need to break open all of the stone doors. ALL of them!", "I~heard~you~the~first~time.");
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 365, 393) <= 1)
                    {
                        if (client.Player.QuestGUI.CheckQuest(522, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.Player.QuestGUI.CheckObjectives(522, 3))
                            {
                                client.Teleport(63, 102, 1784);
                                client.CreateBoxDialog("You arrived at the Hut.");
                                client.Player.QuestGUI.FinishQuest(522);
                                var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.TimeDoor, client.Player.Class, 523);
                                client.Player.QuestGUI.Accept(ActiveQuest, 0);
                            }
                            else
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var msg = rec.GetStream();
                                    client.CreateDialog(msg, "You need to break open all of the stone doors. ALL of them!", "I~heard~you~the~first~time.");
                                }
                            }
                        }
                        else if (client.Player.QuestGUI.CheckQuest(522, MsgQuestList.QuestListItem.QuestStatus.Finished))
                        {
                            client.Teleport(63, 102, 1784);
                            client.CreateBoxDialog("You arrived at the Hut.");
                        }
                        else
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.CreateDialog(msg, "You need to break open all of the stone doors. ALL of them!", "I~heard~you~the~first~time.");
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 351, 407) <= 1)
                    {
                        if (client.Player.QuestGUI.CheckQuest(522, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.Player.QuestGUI.CheckObjectives(522, 3))
                            {
                                client.Teleport(63, 102, 1784);
                                client.CreateBoxDialog("You arrived at the Hut.");
                                client.Player.QuestGUI.FinishQuest(522);
                                var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.TimeDoor, client.Player.Class, 523);
                                client.Player.QuestGUI.Accept(ActiveQuest, 0);

                            }
                            else
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var msg = rec.GetStream();
                                    client.CreateDialog(msg, "You need to break open all of the stone doors. ALL of them!", "I~heard~you~the~first~time.");
                                }
                            }
                        }
                        else if (client.Player.QuestGUI.CheckQuest(522, MsgQuestList.QuestListItem.QuestStatus.Finished))
                        {
                            client.Teleport(63, 102, 1784);
                            client.CreateBoxDialog("You arrived at the Hut.");
                        }
                        else
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.CreateDialog(msg, "You need to break open all of the stone doors. ALL of them!", "I~heard~you~the~first~time.");
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 450, 720) <= 3)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1813, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (!(DateTime.Now > client.Player.GallbladerrStamp.AddSeconds(30)))
                                if (client.Inventory.Contain(721909, 1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Inventory.Remove(721909, 1, msg);
                                        if (Role.Core.Rate(20))
                                        {
                                            client.Inventory.Add(msg, 721908);
                                            client.CreateBoxDialog("You've used the Gallbladder and find a Bitter Ginseng!");
                                        }
                                        else
                                            client.CreateBoxDialog("You've used the Gallbladder but didn`t find a Bitter Ginseng!");
                                    }
                                }
                                else
                                    client.CreateBoxDialog("You don`t have a Gallbladder");
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 296, 290) <= 3)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1813, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (!(DateTime.Now > client.Player.GallbladerrStamp.AddSeconds(30)))
                                if (client.Inventory.Contain(721909, 1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Inventory.Remove(721909, 1, msg);
                                        if (Role.Core.Rate(20))
                                        {
                                            client.Inventory.Add(msg, 721908);
                                            client.CreateBoxDialog("You've used the Gallbladder and find a Bitter Ginseng!");
                                        }
                                        else
                                            client.CreateBoxDialog("You've used the Gallbladder but didn`t find a Bitter Ginseng!");
                                    }
                                }
                                else
                                    client.CreateBoxDialog("You don`t have a Gallbladder");
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 255, 228) <= 3)
                    {
                        if (!(DateTime.Now > client.Player.GallbladerrStamp.AddSeconds(30)))
                            if (client.Player.QuestGUI.CheckQuest(1813, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                            {
                                if (client.Inventory.Contain(721909, 1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Inventory.Remove(721909, 1, msg);
                                        if (Role.Core.Rate(20))
                                        {
                                            client.Inventory.Add(msg, 721908);
                                            client.CreateBoxDialog("You've used the Gallbladder and find a Bitter Ginseng!");
                                        }
                                        else
                                            client.CreateBoxDialog("You've used the Gallbladder but didn`t find a Bitter Ginseng!");
                                    }
                                }
                                else
                                    client.CreateBoxDialog("You don`t have a Gallbladder");
                            }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 243, 193) <= 3)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1813, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (!(DateTime.Now > client.Player.GallbladerrStamp.AddSeconds(30)))
                                if (client.Inventory.Contain(721909, 1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Inventory.Remove(721909, 1, msg);
                                        if (Role.Core.Rate(20))
                                        {
                                            client.Inventory.Add(msg, 721908);
                                            client.CreateBoxDialog("You've used the Gallbladder and find a Bitter Ginseng!");
                                        }
                                        else
                                            client.CreateBoxDialog("You've used the Gallbladder but didn`t find a Bitter Ginseng!");
                                    }
                                }
                                else
                                    client.CreateBoxDialog("You don`t have a Gallbladder");
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 229, 142) <= 3)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1813, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (!(DateTime.Now > client.Player.GallbladerrStamp.AddSeconds(30)))
                                if (client.Inventory.Contain(721909, 1))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var msg = rec.GetStream();
                                        client.Inventory.Remove(721909, 1, msg);
                                        if (Role.Core.Rate(20))
                                        {
                                            client.Inventory.Add(msg, 721908);
                                            client.CreateBoxDialog("You've used the Gallbladder and find a Bitter Ginseng!");
                                        }
                                        else
                                            client.CreateBoxDialog("You've used the Gallbladder but didn`t find a Bitter Ginseng!");
                                    }
                                }
                                else
                                    client.CreateBoxDialog("You don`t have a Gallbladder");
                        }
                    }

                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 445, 681) <= 4)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1625, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            client.Player.MessageBox("Are~you~sure~you~want~to~dig~up~treasures,~here?", new Action<Client.GameClient>(user =>
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var msg = rec.GetStream();
                                    if (user.Inventory.HaveSpace(1))
                                        user.Inventory.Add(msg, 711460);
                                    else
                                        user.CreateBoxDialog("Please make 1 more space in your inventory.");
                                }

                            })
                                , null, 99999);
                        }
                    }

                    // client.Player.MessageBox("Do you want to jump off the cliff to prove your courge?", new Action<Client.GameClient>(user => user.Teleport(1011, 375, 48, 0)), null, 99999);


                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 774, 526) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1661, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1661, 1);
                            }
                            client.SendSysMesage("There's nothing abnormal, here. You can go spy on the other places.", MsgMessage.ChatMode.System);


                            if (client.Player.QuestGUI.CheckObjectives(1661, 1, 1, 1, 1))
                                client.SendSysMesage("You found that bridge was bady damaged. Hurry to tell the Bird Island Castelian about it!", MsgMessage.ChatMode.System);


                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 774, 606) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1661, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1661, 0, 1);

                            }
                            client.SendSysMesage("There's nothing abnormal, here. You can go spy on the other places.", MsgMessage.ChatMode.System);


                            if (client.Player.QuestGUI.CheckObjectives(1661, 1, 1, 1, 1))
                                client.SendSysMesage("You found that bridge was bady damaged. Hurry to tell the Bird Island Castelian about it!", MsgMessage.ChatMode.System);


                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 693, 606) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1661, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1661, 0, 0, 1);
                            }
                            client.SendSysMesage("There's nothing abnormal, here. You can go spy on the other places.", MsgMessage.ChatMode.System);


                            if (client.Player.QuestGUI.CheckObjectives(1661, 1, 1, 1, 1))
                                client.SendSysMesage("You found that bridge was bady damaged. Hurry to tell the Bird Island Castelian about it!", MsgMessage.ChatMode.System);


                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 693, 519) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1661, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1661, 0, 0, 1);

                            }
                            client.SendSysMesage("There's nothing abnormal, here. You can go spy on the other places.", MsgMessage.ChatMode.System);


                            if (client.Player.QuestGUI.CheckObjectives(1661, 1, 1, 1, 1))
                                client.SendSysMesage("You found that bridge was bady damaged. Hurry to tell the Bird Island Castelian about it!", MsgMessage.ChatMode.System);


                        }
                    }


                }
                if (client.Player.Map == 1020)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 380, 49) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1352, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.Player.MessageBox("Do you want to jump off the cliff to prove your courge?", new Action<Client.GameClient>(user => user.Teleport(375, 48, 1011, 0)), null, 99999);


                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 027, 375) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1344, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.SendSysMesage("It`s cliff all around.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1344, 1);
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 451, 463) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1344, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.SendSysMesage("It`s cliff all around.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1344, 0, 1);
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 543, 885) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1344, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.SendSysMesage("It`s cliff all around.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1344, 0, 0, 1);
                            }
                        }
                    }

                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 473, 541) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1338, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();
                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1338, 1);
                            }
                            client.Teleport(566, 570, 1020);

                            client.CreateBoxDialog("You~chanted~the~spell!~You~arrived~the~Love~Canyon!");


                        }
                    }
                    //1338
                }
                else if (client.Player.Map == 1000)
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 465, 676) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1452, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.CreateBoxDialog("There`s~no~problem~here.~Go~inspect~the~next~spot.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1452, 1);
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 506, 684) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1452, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.CreateBoxDialog("There`s~no~problem~here.~Go~inspect~the~next~spot.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1452, 0, 1);
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 533, 654) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1452, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.CreateBoxDialog("There`s~no~problem~here.~Go~inspect~the~next~spot.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1452, 0, 0, 1);
                            }
                        }
                    }
                    else if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 533, 626) <= 2)
                    {
                        if (client.Player.QuestGUI.CheckQuest(1452, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            client.CreateBoxDialog("There`s~no~problem~here.~Go~inspect~the~next~spot.");
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var msg = rec.GetStream();

                                client.Player.QuestGUI.IncreaseQuestObjectives(msg, 1452, 0, 0, 0, 1);
                            }
                        }
                    }
                }

                //Database.VoteSystem.CheckUp(client);

                if (client.Player.OnDefensePotion)
                {
                    if (timer > client.Player.OnDefensePotionStamp)
                    {
                        client.Player.OnDefensePotion = false;
                    }
                }
                if (client.Player.OnAttackPotion)
                {
                    if (timer > client.Player.OnAttackPotionStamp)
                    {
                        client.Player.OnAttackPotion = false;
                    }
                }
                if (client.Player.ActivePick)
                {
                    if (timer > client.Player.PickStamp)
                    {
                        client.Player.ActivePick = false;
                        if (client.Player.QuestGUI.CheckQuest(1830, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (client.Player.Money >= 99999)
                                {
                                    client.Player.Money -= 99999;
                                    client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                    if (Role.Core.Rate(60))
                                    {
                                        client.Player.Money += 100;
                                        client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                        client.Inventory.Add(stream, 721878);
                                        client.SendSysMesage("You received 100 Silver!");
                                        client.Player.QuestGUI.FinishQuest(1830);
                                        client.SendSysMesage("Shark is satisfied with your bid and sold the Victory Portrait to you.");
                                        client.ActiveNpc = (uint)Game.MsgNpc.NpcID.Shark;
                                        Game.MsgNpc.NpcHandler.Shark(client, stream, 4, "", 0);
                                    }
                                    else
                                    {
                                        client.CreateDialog(stream, "Too low! Higher!", "I~see.");
                                    }
                                }
                                else
                                {
                                    client.CreateDialog(stream, "Sorry, but you don`t have enough Silver.", "I~see.");
                                }
                            }

                        }
                        if (client.Player.QuestGUI.CheckQuest(3647, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower1 || client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower6
                                || client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower2 || client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower5
                                || client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower3 || client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower4
                                || client.ActiveNpc == (uint)Game.MsgNpc.NpcID.LavaFlower7)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.AddItemWitchStack(3008747, 0, 1, stream);
                                        client.SendSysMesage("You received LavaFlower!", MsgMessage.ChatMode.System);
                                        if (client.Inventory.Contain(3008747, 10))
                                            client.CreateBoxDialog("You`ve collected 10 Lava Flowers. Go and try to extract the Fire Force.");

                                    }
                                    else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");

                                }
                            }

                        }
                        if (client.Player.QuestGUI.CheckQuest(3642, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.ActiveNpc >= (uint)Game.MsgNpc.NpcID.WhiteHerb1 && client.ActiveNpc <= (uint)Game.MsgNpc.NpcID.WhiteHerb6)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.AddItemWitchStack(3008741, 0, 1, stream);
                                        client.SendSysMesage("You received WhiteHerb!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");

                                }
                            }

                        }
                        if (client.Player.QuestGUI.CheckQuest(1653, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.ActiveNpc >= 8551 && client.ActiveNpc <= 8555)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.AddItemWitchStack(711478, 0, 1, stream);
                                        client.SendSysMesage("You~received~a~Rainbow~Flower!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");


                                    if (client.OnRemoveNpc != null)
                                    {
                                        client.OnRemoveNpc.Respawn = DateTime.Now.AddSeconds(10);
                                        client.Map.RemoveNpc(client.OnRemoveNpc, stream);
                                        client.Map.soldierRemains.TryAdd(client.OnRemoveNpc.UID, client.OnRemoveNpc);
                                    }
                                }
                            }
                        }
                        if (client.Player.QuestGUI.CheckQuest(6131, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.Inventory.Contain(720995, 1))
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    ActionQuery action = new ActionQuery()
                                    {
                                        ObjId = client.Player.UID,
                                        Type = ActionType.ClikerON,
                                        Fascing = 7,
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y,
                                        dwParam = 0x0c,


                                    };
                                    client.Send(stream.ActionCreate(&action));
                                }
                            }
                            else if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.SaltedFish)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.Add(stream, 711479);
                                        client.SendSysMesage("You received a pack of Salted Fish!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                }
                            }

                        }
                        if (client.Player.QuestGUI.CheckQuest(1640, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.SaltedFish)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.Add(stream, 711472);
                                        client.SendSysMesage("You receive the Salted Fish!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                }
                            }
                            else if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.FishingNet)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.Add(stream, 711473);
                                        client.SendSysMesage("You received a Fishing Net!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                }

                            }
                        }
                        if (client.Player.QuestGUI.CheckQuest(1594, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.WhiteChrysanthemum)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.Add(stream, 711441);
                                    client.SendSysMesage("You've got a White Chrysanthemum!", MsgMessage.ChatMode.System);
                                }
                            }
                            else if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.Jasmine)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.Add(stream, 711442);
                                    client.SendSysMesage("You've got a Jasmine!", MsgMessage.ChatMode.System);
                                }
                            }
                            else if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.Lily)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.Add(stream, 711440);
                                    client.SendSysMesage("You've got a Lily!", MsgMessage.ChatMode.System);
                                }
                            }
                            else if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.WillowLeaf)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.Add(stream, 711443);
                                    client.SendSysMesage("You've got a Willow Leaf!", MsgMessage.ChatMode.System);
                                }
                            }


                        }
                        if (client.Player.QuestGUI.CheckQuest(1469, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.st1TreeSeed)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    client.Inventory.Add(stream, 720971);
                                    client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1469, 1);
                                    if (client.Player.QuestGUI.CheckObjectives(1469, 1, 1, 1))
                                        client.CreateBoxDialog("You`ve~collected~enough~seeds.~Go~report~to~Wan~Ying,~right~away.");
                                    else
                                        client.CreateBoxDialog("You`ve~received~a~seed.");
                                }
                            }
                            if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.nd2TreeSeed)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.Add(stream, 720971);
                                    client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1469, 0, 1);
                                    if (client.Player.QuestGUI.CheckObjectives(1469, 1, 1, 1))
                                        client.CreateBoxDialog("You`ve~collected~enough~seeds.~Go~report~to~Wan~Ying,~right~away.");
                                    else
                                        client.CreateBoxDialog("You`ve~received~a~seed.");
                                }
                            }
                            if (client.ActiveNpc == (ushort)Game.MsgNpc.NpcID.rd3TreeSeed)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.Add(stream, 720971);
                                    client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1469, 0, 0, 1);
                                    if (client.Player.QuestGUI.CheckObjectives(1469, 1, 1, 1))
                                        client.CreateBoxDialog("You`ve~collected~enough~seeds.~Go~report~to~Wan~Ying,~right~away.");
                                    else
                                        client.CreateBoxDialog("You`ve~received~a~seed.");
                                }
                            }
                        }
                        if (client.Player.QuestGUI.CheckQuest(1330, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "allcure5");
                                switch (client.Player.QuestCaptureType)
                                {

                                    case 1:
                                        {
                                            client.SendSysMesage("You captured a Thunder Ape.", MsgMessage.ChatMode.System);
                                            client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1330, 1);
                                        }
                                        break;
                                    case 2:
                                        {
                                            client.SendSysMesage("You captured a Thunder Ape L58.", MsgMessage.ChatMode.System);
                                            client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1330, 0, 1);
                                        }
                                        break;

                                }
                            }
                        }
                        if (client.Player.QuestGUI.CheckQuest(1317, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.CarpenterJack, client.Player.Class, 1317);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Inventory.AddItemWitchStack(711356, 0, 1, stream);
                                client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1317, 1);
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "allcure5");
                                client.SendSysMesage("You received 1 Chiff Flower.", MsgMessage.ChatMode.System);
                            }
                            if (client.Player.QuestGUI.CheckObjectives(1317, 20))
                            {
                                client.Player.QuestGUI.SendAutoPatcher("You have collected enough CliffFowers. Send it to Carpenter Jack.", ActiveQuest.FinishNpcId.Map, ActiveQuest.FinishNpcId.X, ActiveQuest.FinishNpcId.Y, ActiveQuest.FinishNpcId.ID);
                            }
                        }
                        else if (client.Player.QuestGUI.CheckQuest(1011, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            if (client.Inventory.HaveSpace(1))
                            {
                                if (client.Inventory.Contain(711239, 5))
                                {
                                    var ActiveQuest4 = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.XuLiang, client.Player.Class, 1011);
                                    client.Player.QuestGUI.SendAutoPatcher("You`ve~picked~5~Peach~Blossoms!~Now~give~them~to~Xu~Liang.", ActiveQuest4.FinishNpcId.Map, ActiveQuest4.FinishNpcId.X, ActiveQuest4.FinishNpcId.Y, ActiveQuest4.FinishNpcId.ID);
                                }
                                else
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.QuestGUI.IncreaseQuestObjectives(stream, 1011, 1);
                                        client.Inventory.AddItemWitchStack(711239, 0, 1, stream);
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "allcure5");
                                        client.SendSysMesage("You picked a Peach Blossom from the Peach Tree!", MsgMessage.ChatMode.System);
                                    }

                                }
                            }
                            else
                            {
                                client.SendSysMesage("Please make 1 more space in your inventory.", MsgMessage.ChatMode.System);
                            }
                        }
                        else if (client.Player.QuestGUI.CheckQuest(6049, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "accession1");
                                client.Player.QuestGUI.IncreaseQuestObjectives(stream, 6049, 1, 1);

                                if (client.OnRemoveNpc != null)
                                {
                                    Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
                                    packet.ID = MsgStringPacket.StringID.Effect;
                                    packet.UID = client.OnRemoveNpc.UID;
                                    packet.Strings = new string[1] { "M_Fire1" };
                                    client.Player.View.SendView(stream.StringPacketCreate(packet), true);


                                    client.OnRemoveNpc.Respawn = DateTime.Now.AddSeconds(10);
                                    client.Map.RemoveNpc(client.OnRemoveNpc, stream);
                                    client.Map.soldierRemains.TryAdd(client.OnRemoveNpc.UID, client.OnRemoveNpc);
                                    //add effect here
                                    Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                                    dialog.AddText("What? You said the Desert Guardian sent you here to find us? Well, I had to play dead to keep the bandits from seeing me. I will avenge my comrades, one day!")
                                  .AddText("~I`ll go back and report this to Desert Guardian! Thanks for coming to find us. I thought we would never be seen again.");
                                    dialog.AddOption("No~Problem.", 255);
                                    dialog.AddAvatar(101).FinalizeDialog();

                                }

                                if (client.Player.QuestGUI.CheckObjectives(6049, 8))
                                {

                                    var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)Game.MsgNpc.NpcID.DesertGuardian, client.Player.Class, 6049);
                                    client.Player.QuestGUI.SendAutoPatcher("You~are~too~far~away~from~the~Soldier`s~Remains!", ActiveQuest.FinishNpcId.Map, ActiveQuest.FinishNpcId.X, ActiveQuest.FinishNpcId.Y, ActiveQuest.FinishNpcId.ID);
                                    client.Player.QuestGUI.SendAutoPatcher("You~are~too~far~away~from~the~Soldier`s~Remains!", ActiveQuest.FinishNpcId.Map, ActiveQuest.FinishNpcId.X, ActiveQuest.FinishNpcId.Y, ActiveQuest.FinishNpcId.ID);
                                }
                                else
                                {
                                    client.CreateBoxDialog("This soldier has died. Release his soul!");
                                }
                            }
                        }
                        else if (client.Player.QuestGUI.CheckQuest(6014, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                        {

                            if (client.Inventory.Contain(client.Player.DailyMagnoliaItemId, 1))
                            {


                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    client.Map.AddMagnolia(stream, client.Player.DailyMagnoliaItemId);
                                    Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
                                    packet.ID = MsgStringPacket.StringID.Effect;
                                    packet.UID = client.Map.Magnolia.UID;
                                    packet.Strings = new string[1] { "accession1" };
                                    client.Player.View.SendView(stream.StringPacketCreate(packet), true);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "eidolon");
                                    client.Player.QuestGUI.FinishQuest(6014);
                                    client.Inventory.Remove(client.Player.DailyMagnoliaItemId, 1, stream);
                                    switch (client.Player.DailyMagnoliaItemId)
                                    {
                                        case 729306:
                                            {
                                                client.Player.SubClass.AddStudyPoints(client, 10, stream);
                                                client.Inventory.AddItemWitchStack(729304, 0, 1, stream);
                                                client.GainExpBall(600, true, Role.Flags.ExperienceEffect.angelwing);
                                                client.CreateBoxDialog("Congratulations!~You~received~60 minutes of EXP, 10 Study Points and 1 Chi Token.!");
                                                break;
                                            }
                                        case 729307:
                                            {
                                                client.Player.SubClass.AddStudyPoints(client, 20, stream);
                                                client.Inventory.AddItemWitchStack(729304, 0, 1, stream);
                                                client.GainExpBall(900, true, Role.Flags.ExperienceEffect.angelwing);
                                                client.CreateBoxDialog("Congratulations!~You~received~90 minutes of EXP, 20 Study Points, 1 Chi Token.!");
                                                break;
                                            }
                                        case 729308:
                                            {
                                                client.Player.SubClass.AddStudyPoints(client, 50, stream);
                                                client.Inventory.AddItemWitchStack(729304, 0, 1, stream);
                                                client.GainExpBall(1200, true, Role.Flags.ExperienceEffect.angelwing);
                                                client.CreateBoxDialog("Congratulations!~You~received~120 minutes of EXP, 50 Study Points, 1 Chi Token!");
                                                break;
                                            }
                                        case 729309:
                                            {
                                                client.Player.SubClass.AddStudyPoints(client, 100, stream);
                                                client.Inventory.AddItemWitchStack(729304, 0, 1, stream);
                                                client.GainExpBall(1800, true, Role.Flags.ExperienceEffect.angelwing);
                                                client.CreateBoxDialog("Congratulations!~You~received~180 minutes of EXP, 100 Study Points, 1 Chi Token.!");
                                                break;
                                            }
                                        case 7293010:
                                            {
                                                client.Player.SubClass.AddStudyPoints(client, 300, stream);
                                                client.Inventory.AddItemWitchStack(729304, 0, 1, stream);
                                                client.GainExpBall(3000, true, Role.Flags.ExperienceEffect.angelwing);
                                                client.CreateBoxDialog("Congratulations!~You~received~300 minutes of EXP, 300 Study Points, 1 Chi Token.!");
                                                break;
                                            }
                                    }
                                }
                            }
                            else
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.RemovePick(stream);
                                }
                            }
                        }
                    }
                }

                if (timer > client.Player.OnlineStamp.AddMinutes(1))
                {
                    client.Player.OnlineMinutes += 1;
                    client.Player.OnlineStamp = DateTime.Now;
                }
                if (client.Player.X == 0 || client.Player.Y == 0)
                {
                    client.Teleport(428, 378, 1002);
                }
                if (client.Player.HeavenBlessing > 0)
                {
                    if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing))
                    {
                        if (timer > client.Player.HeavenBlessTime)
                        {
                            client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing);
                            client.Player.HeavenBlessing = 0;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, 0, Game.MsgServer.MsgUpdate.DataType.HeavensBlessing);
                                client.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Remove, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);

                                client.Player.Stamina = (ushort)Math.Min((int)client.Player.Stamina, 100);
                                client.Player.SendUpdate(stream, client.Player.Stamina, Game.MsgServer.MsgUpdate.DataType.Stamina);
                            }
                        }
                        if (client.Player.Map != 601 && client.Player.Map != 1039)
                        {
                            if (timer > client.Player.ReceivePointsOnlineTraining)
                            {
                                client.Player.ReceivePointsOnlineTraining = timer.AddMinutes(1);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.IncreasePoints, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);//+10
                                }
                            }
                            if (timer > client.Player.OnlineTrainingTime)
                            {
                                client.Player.OnlineTrainingPoints += 100000;
                                client.Player.OnlineTrainingTime = timer.AddMinutes(10);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.ReceiveExperience, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                                }
                            }
                        }
                    }
                }
                if (client.Player.EnlightenReceive > 0)
                {
                    if (DateTime.Now > client.Player.EnlightenTime.AddMinutes(20))
                    {
                        client.Player.EnlightenTime = DateTime.Now;
                        client.Player.EnlightenReceive -= 1;
                    }
                }
                if (client.Player.DExpTime > 0)
                {
                    client.Player.DExpTime -= 1;
                    if (client.Player.DExpTime == 0)
                        client.Player.RateExp = 1;
                }



            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public static unsafe void AutoAttackCallback(Client.GameClient client/*, int time*/)
        {
            try
            {
                if (client == null || !client.FullLoading || client.Player == null)
                    return;
                if (client.Player.Alive == false && client.Player.CompleteLogin)
                {
                    if (DateTime.Now > client.Player.GhostStamp)
                    {
                        if (!client.Player.ContainFlag(MsgUpdate.Flags.Ghost))
                        {
                            client.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.Ghost, Role.StatusFlagsBigVector32.PermanentFlag, true);
                            if (client.Player.Body % 10 < 3)
                                client.Player.TransformationID = 99;
                            else
                                client.Player.TransformationID = 98;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Send(stream.MapStatusCreate(client.Player.Map, client.Map.ID, (uint)client.Map.TypeStatus));
                            }
                        }
                    }
                }
                DateTime timer = DateTime.Now;
                if (client.OnAutoAttack && client.Player.Alive)
                {
                    if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Dizzy))
                    {
                        client.OnAutoAttack = false;
                        return;
                    }
                    InteractQuery action = new InteractQuery();
                    action = InteractQuery.ShallowCopy(client.AutoAttack);
                    client.Player.RandomSpell = action.SpellID;
                    if (action.SpellID >= 1000 && action.SpellID <= 1002)
                        if (client.Player.Mana < 100)
                        {
                            client.OnAutoAttack = false;
                            return;
                        }
                    MsgAttackPacket.Process(client, action);
                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public static void StaminaCallback(GameClient client)
        {
            try
            {
                if (client == null || !client.FullLoading || client.Player == null)
                    return;
                DateTime Now = DateTime.Now;

                if (!client.Player.Alive)
                    return;
                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Fly))
                    return;
                byte MaxStamina = (byte)(client.Player.HeavenBlessing > 0 ? 150 : 100);
                if (client.Player.Stamina < MaxStamina)
                {
                    if (DateTime.Now >= client.Player.StaminaStamp.AddMilliseconds(2000))
                    {
                        client.Player.StaminaStamp = DateTime.Now;
                        ushort addstamin = 0;
                        addstamin += client.Player.GetAddStamina();
                        client.Player.Stamina = (ushort)Math.Min((int)(client.Player.Stamina + addstamin), MaxStamina);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendUpdate(stream, client.Player.Stamina, Game.MsgServer.MsgUpdate.DataType.Stamina);
                        }
                    }
                }

                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Ride))
                {
                    if (client.Player.CheckInvokeFlag(Game.MsgServer.MsgUpdate.Flags.Ride, Now))
                    {
                        if (client.Vigor < client.Status.MaxVigor)
                        {
                            client.Vigor = (ushort)Math.Min(client.Vigor + 2, client.Status.MaxVigor);

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Send(stream.ServerInfoCreate(MsgServerInfo.Action.Vigor, client.Vigor));
                            }
                        }
                    }

                }
                if (DateTime.Now > client.LastOnlineStamp.AddMinutes(5))
                {
                    client.LastOnlineStamp = DateTime.Now;
                    client.Player.OnlinePoints++;
                    if (DateTime.Now.Minute % 30 == 0)
                        client.SendSysMesage("You got 1 online point. Total:" + client.Player.OnlinePoints);
                }
                if (client == null || !client.FullLoading || client.Player == null)
                    return;

                DateTime Timer = DateTime.Now;
                if (Database.ItemType.IsTwoHand(client.Equipment.RightWeapon))
                {
                    if (client.Equipment.LeftWeapon != 0 && ((Database.ItemType.IsShield(client.Equipment.LeftWeapon) || Database.ItemType.IsArrow(client.Equipment.LeftWeapon)) == false))
                    {
                        if (client.Inventory.HaveSpace(1))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (client.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream) == false)
                                    client.Equipment.Remove(Role.Flags.ConquerItem.AleternanteLeftWeapon, stream);
                                client.Equipment.LeftWeapon = 0;
                            }
                        }
                    }
                }

                if (client.Player.PKPoints > 0 && client.Player.Alive)
                {
                    if (Timer > client.Player.PkPointsStamp.AddMinutes(6))
                    {
                        client.Player.PKPoints -= 1;
                        client.Player.PkPointsStamp = DateTime.Now;
                    }
                }

                if (Timer > client.Player.XPListStamp.AddSeconds(4) && client.Player.Alive)
                {
                    client.Player.XPListStamp = Timer.AddSeconds(4);
                    if (!client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.XPList))
                    {
                        client.Player.XPCount++;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            client.Player.SendUpdate(stream, client.Player.XPCount, MsgUpdate.DataType.XPCircle);
                            if (client.Player.XPCount >= 100)
                            {
                                client.Player.XPCount = 0;
                                client.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.XPList, 20, true);
                                client.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { "xp" });
                            }
                        }
                    }
                }
                if (client.Player.InUseIntensify)
                {
                    if (Timer > client.Player.IntensifyStamp.AddSeconds(2))
                    {
                        if (!client.Player.Intensify)
                        {
                            client.Player.Intensify = true;
                            client.Player.InUseIntensify = false;
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public unsafe static void BuffersCallback(GameClient client)
        {
            try
            {
                if (client == null || !client.FullLoading)
                    return;
                #region Pk
                if (client.Player.Map == 501)
                {
                    client.Player.SetPkMode(Role.Flags.PKMode.PK);

                }               
                #endregion
                DateTime Timer = DateTime.Now;
                if (client.Player.Map == 501 && client.Player.DynamicID == 0 && !client.Player.Alive && Timer > client.Player.DeadStamp.AddSeconds(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.Revive(stream);
                    }
                }    
                if (client.Player.Map == 1005 && client.Player.DynamicID == 0 && !client.Player.Alive && Timer > client.Player.DeadStamp.AddSeconds(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.Revive(stream);
                    }
                }
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process == Game.MsgTournaments.ProcesType.Alive && client.Player.HasTheBomb && DateTime.Now > client.Player.bombStamp.AddSeconds(Game.MsgTournaments.PassTheBomb.SecondsLeft))
                {
                    client.Player.HasTheBomb = false;
                    client.Teleport(430, 380, 1002);
                    client.SendWhisper("You`ve been knocked out from PassTheBomb. Goodluck next time.", "[PassTheBomb]", client.Player.Name);
                }
                if (client.Player.BlockMovementCo && DateTime.Now > client.Player.BlockMovement)
                {
                    client.Player.Protect = DateTime.Now.AddSeconds(1);
                    client.Player.BlockMovementCo = false;
                    client.SendSysMesage("You`re free to move now. You have 1 second to jump away.");
                }
                if (client.Player.BlackSpot)
                {
                    if (Timer > client.Player.Stamp_BlackSpot)
                    {
                        client.Player.BlackSpot = false;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            client.Player.View.SendView(stream.BlackspotCreate(false, client.Player.UID), true);
                        }
                    }
                }
                foreach (var flag in client.Player.BitVector.GetFlags())
                {
                    if (flag.Expire(Timer))
                    {
                        if (flag.Key >= (int)Game.MsgServer.MsgUpdate.Flags.TyrantAura && flag.Key <= (int)Game.MsgServer.MsgUpdate.Flags.EartAura)
                        {
                            client.Player.AddAura(client.Player.UseAura, null, 0);
                        }
                        else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Cursed)
                        {
                            client.Player.CursedTimer = 0;
                            client.Player.RemoveFlag(MsgUpdate.Flags.Cursed);
                        }
                        else
                        {

                            if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Superman || flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Cyclone)
                            {
                                Role.KOBoard.KOBoardRanking.AddItem(new Role.KOBoard.Entry() { UID = client.Player.UID, Name = client.Player.Name, Points = (uint)client.Player.KillCounter }, true);
                            }
                            if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.CTF_Flag)
                            {
                                if (client.Player.MyGuild != null)
                                {
                                    //client.Player.MyGuildMember.GetFlags = 0;
                                }
                            }
                            client.Player.RemoveFlag((Game.MsgServer.MsgUpdate.Flags)flag.Key);
                        }

                    }
                    else if(flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.SoulShackle)
                    {
                        client.Player.RemovedShackle = DateTime.Now;
                    }
                    else if (flag.Key == (int)MsgUpdate.Flags.CTF_Flag)
                    {
                        if (Game.MsgTournaments.MsgSchedules.CaptureTheFlag != null)
                        {
                            if (client.Player.Map != Game.MsgTournaments.MsgCaptureTheFlag.MapID)
                            {
                                if (client.Player.MyGuild != null)
                                {
                                    //client.Player.MyGuildMember.GetFlags = 0;
                                }
                                client.Player.RemoveFlag((Game.MsgServer.MsgUpdate.Flags)flag.Key);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.CaptureTheFlagUpdateCreate(MsgCaptureTheFlagUpdate.Mode.GenerateTimer, 0, client.Player.UID);
                                    stream.CaptureTheFlagUpdateFinalize();
                                    client.Send(stream);
                                }
                            }
                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Poisoned)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            int damage = (int)Game.MsgServer.AttackHandler.Calculate.Base.CalculatePoisonDamageFog((uint)client.Player.HitPoints, client.Player.PoisonLevel);

                            if (client.Player.HitPoints == 1)
                            {
                                damage = 0;
                                goto jump;
                            }
                            client.Player.HitPoints = Math.Max(1, (int)(client.Player.HitPoints - damage));

                            jump:

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                InteractQuery action = new InteractQuery()
                                {
                                    Damage = damage,
                                    AtkType = MsgAttackPacket.AttackID.Physical,
                                    X = client.Player.X,
                                    Y = client.Player.Y,
                                    OpponentUID = client.Player.UID
                                };
                                client.Player.View.SendView(stream.InteractionCreate(&action), true);
                            }

                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.ShurikenVortex)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                InteractQuery action = new InteractQuery()
                                {
                                    UID = client.Player.UID,
                                    X = client.Player.X,
                                    Y = client.Player.Y,
                                    SpellID = (ushort)Role.Flags.SpellID.ShurikenEffect,
                                    AtkType = MsgAttackPacket.AttackID.Magic
                                };

                                MsgAttackPacket.ProcessMagic(client, stream.InteractionCreate(&action), action);
                            }
                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.RedName || flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.BlackName && client.Player.Map != 6000)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            if (client.Player.PKPoints > 0)
                                client.Player.PKPoints -= 1;

                            client.Player.PkPointsStamp = DateTime.Now;
                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Cursed)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            if (client.Player.CursedTimer > 0)
                                client.Player.CursedTimer -= 1;
                            else
                            {
                                client.Player.CursedTimer = 0;
                                client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Cursed);
                            }
                        }
                    }



                }
                if (client.Player.OnTransform)
                {
                    if (client.Player.TransformInfo != null)
                    {
                        if (client.Player.TransformInfo.CheckUp(Timer))
                            client.Player.TransformInfo = null;
                    }
                }
                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Praying))
                {
                    if (client.Player.BlessTime < 7200000 - 30000)
                    {
                        if (Timer > client.Player.CastPrayStamp.AddSeconds(30))
                        {
                            bool have = false;
                            foreach (var ownerpraying in client.Player.View.Roles(Role.MapObjectType.Player))
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, ownerpraying.X, ownerpraying.Y) <= 2)
                                {
                                    var target = ownerpraying as Role.Player;
                                    if (target.ContainFlag(MsgUpdate.Flags.CastPray))
                                    {
                                        have = true;
                                        break;
                                    }
                                }
                            }
                            if (!have)
                                client.Player.RemoveFlag(MsgUpdate.Flags.Praying);
                            client.Player.CastPrayStamp = new DateTime(Timer.Ticks);
                            client.Player.BlessTime += 30000;
                        }
                    }
                    else
                        client.Player.BlessTime = 3100000;
                }
                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.CastPray))
                {
                    if (client.Player.BlessTime < 7200000 - 60000)
                    {
                        if (Timer > client.Player.CastPrayStamp.AddSeconds(30))
                        {
                            client.Player.CastPrayStamp = new DateTime(Timer.Ticks);
                            client.Player.BlessTime += 60000;
                        }
                    }
                    else
                        client.Player.BlessTime = 7200000;
                    if (Timer > client.Player.CastPrayActionsStamp.AddSeconds(5))
                    {
                        client.Player.CastPrayActionsStamp = new DateTime(Timer.Ticks);
                        foreach (var obj in client.Player.View.Roles(Role.MapObjectType.Player))
                        {
                            if (Role.Core.GetDistance(client.Player.X, client.Player.Y, obj.X, obj.Y) <= 2)
                            {
                                var Target = obj as Role.Player;
                                if (Target.Reborn < 2)
                                {
                                    if (!Target.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Praying))
                                    {
                                        Target.AddFlag(Game.MsgServer.MsgUpdate.Flags.Praying, Role.StatusFlagsBigVector32.PermanentFlag, true);

                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            ActionQuery action = new ActionQuery()
                                            {
                                                ObjId = client.Player.UID,
                                                dwParam = (uint)client.Player.Action,
                                                Timestamp = (int)obj.UID
                                            };
                                            client.Player.View.SendView(stream.ActionCreate(&action), true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (client.Player.BlessTime > 0)
                {
                    if (!client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.CastPray) && !client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Praying))
                    {

                        if (Timer > client.Player.CastPrayStamp.AddSeconds(2))
                        {
                            if (client.Player.BlessTime > 2000)
                                client.Player.BlessTime -= 2000;
                            else
                                client.Player.BlessTime = 0;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.BlessTime, Game.MsgServer.MsgUpdate.DataType.LuckyTimeTimer);
                            }
                            client.Player.CastPrayStamp = new DateTime(Timer.Ticks);
                        }
                    }
                }
                if (client.Team != null)
                {
                    if (client.Team.AutoInvite == true && client.Player.Map != 1036 && client.Team.CkeckToAdd())
                    {
                        if (Timer > client.Team.InviteTimer.AddSeconds(10))
                        {
                            client.Team.InviteTimer = Timer;
                            foreach (var obj in client.Player.View.Roles(Role.MapObjectType.Player))
                            {
                                if (!client.Team.SendInvitation.Contains(obj.UID))
                                {
                                    client.Team.SendInvitation.Add(obj.UID);

                                    if ((obj as Role.Player).Owner.Team == null)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();

                                            obj.Send(stream.PopupInfoCreate(client.Player.UID, obj.UID, client.Player.Level, client.Player.BattlePower));

                                            stream.TeamCreate(MsgTeam.TeamTypes.InviteRequest, client.Player.UID);
                                            obj.Send(stream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (client.Team.TeamLider(client))
                    {
                        if (Timer > client.Team.UpdateLeaderLocationStamp.AddSeconds(4))
                        {
                            client.Team.UpdateLeaderLocationStamp = Timer;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                ActionQuery action = new ActionQuery()
                                {
                                    ObjId = client.Player.UID,
                                    dwParam = 1015,
                                    Type = ActionType.LocationTeamLieder,
                                    wParam1 = client.Team.Leader.Player.X,
                                    wParam2 = client.Team.Leader.Player.Y
                                };
                                client.Team.SendTeam(stream.ActionCreate(&action), client.Player.UID, client.Player.Map);
                            }
                        }
                    }
                }
                if (UnlimitedArenaRooms.Maps.ContainsValue(client.Player.DynamicID))
                {
                    client.SendSysMesage("Accuracy Rates.", MsgMessage.ChatMode.FirstRightCorner);
                    foreach (var player in client.Map.Values.Where(e => e.Player.DynamicID == client.Player.DynamicID))
                        client.SendSysMesage(player.Player.Name + " " + Math.Round((double)(player.Player.Hits * 100.0 / Math.Max(1, player.Player.TotalHits)), 2) +
                            "%, Hits: " + player.Player.Hits + ", Miss: " +
                            (player.Player.TotalHits - player.Player.Hits) + ", M.C: " + player.Player.MaxChains,
                            MsgMessage.ChatMode.ContinueRightCorner);

                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
    }
}
