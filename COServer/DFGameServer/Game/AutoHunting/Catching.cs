using Core;
using GameServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameServer
{
    class Catching
    {
        private Thread JumpPlayer, Skill;
        public Catching()
        {
            JumpPlayer = new Thread(new ThreadStart(JumpHunting));
            JumpPlayer.Start();

            Skill = new Thread(new ThreadStart(SkillHunting));
            Skill.Start();

        }
        public static bool Auto = true;
        private static Random RobotRandom = new Random();
        private static ushort[] SkillRobotTrojan = new ushort[] { 1045, 1046, 1115 };//FastBlade,ScentSword,Hercules
        private static ushort[] SkillRobotArcher = new ushort[] { 8001 };//Scatter
        private static ushort[] SkillRobotWater = new ushort[] { 1000 };//Thunder
        private static ushort[] SkillRobotFire = new ushort[] { 1000, 1002 };//Tornado
        private static ushort[] SkillRobotAttacked = new ushort[] { 1000, 1002 };
        private static ushort[] SkillXPRobot = new ushort[] { 1110 };//CycloneXP
        public static bool ValidClient(Client.GameClient client)
        {
            if (client == null)
                return false;
            if (client.Player == null)
                return false;
            if (!client.Player.Alive)
                return false;
            if (!client.FullLoading)
                return false;
            if (client.Player.CompleteLogin == false)
                return false;
            return true;
        }
        public static bool ValidCoord(Client.GameClient client, ushort X = 0, ushort Y = 0, bool NextDit = false)
        {
            if (client.Map.ID == 1000)
            {
                if (NextDit)
                {
                    if ((X > 468 && X < 544) && (Y > 525 && Y < 696))
                        return false;
                }
                else
                {
                    if ((client.Player.X > 468 && client.Player.X < 544) && (client.Player.Y > 525 && client.Player.Y < 696))
                        return false;
                }
            }
            if (client.Map.ID == 1002)
            {
                if (NextDit)
                {
                    if ((X > 349 && X < 508) && (Y > 212 && Y < 432))
                        return false;
                }
                else
                {
                    if ((client.Player.X > 349 && client.Player.X < 508) && (client.Player.Y > 212 && client.Player.Y < 432))
                        return false;
                }
            }
            if (client.Map.ID == 1011)
            {
                if (NextDit)
                {
                    if ((X > 151 && X < 254) && (Y > 195 && Y < 305))
                        return false;
                }
                else
                {
                    if ((client.Player.X > 151 && client.Player.X < 254) && (client.Player.Y > 195 && client.Player.Y < 305))
                        return false;
                }
            }
            if (client.Map.ID == 1015)
            {
                if (NextDit)
                {
                    if ((X > 684 && X < 782) && (Y > 509 && Y < 617))
                        return false;
                }
                else
                {
                    if ((client.Player.X > 684 && client.Player.X < 782) && (client.Player.Y > 509 && client.Player.Y < 617))
                        return false;
                }
            }
            if (client.Map.ID == 1020)
            {
                if (NextDit)
                {
                    if ((X > 542 && X < 590) && (Y > 544 && Y < 616))
                        return false;
                }
                else
                {
                    if ((client.Player.X > 542 && client.Player.X < 590) && (client.Player.Y > 544 && client.Player.Y < 616))
                        return false;
                }
            }
            return true;
        }
        public static void ShowSettings(Client.GameClient client)
        {
            if (client == null || client.AutoHunting == null)
                return;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                dialog.AddText("Auto Hunt V2 settings. These options are saved per character.");
                dialog.AddOption("Radius: " + client.AutoHunting.RadiusLabel, 240);
                dialog.AddOption("Skills: " + client.AutoHunting.SkillsStatus, 241);
                dialog.AddOption("EXP: " + client.AutoHunting.ExpDeliveryLabel, 242);
                dialog.AddOption("Fast mode: " + client.AutoHunting.FastModeStatus, 243);
                dialog.AddOption("HP potion: " + client.AutoHunting.HpPotionPercent + "%", 245);
                dialog.AddOption("MP potion: " + client.AutoHunting.MpPotionPercent + "%", 246);
                dialog.AddOption("Pickup settings", 244);
                dialog.AddOption("Close", 255);
                dialog.FinalizeDialog();
            }
        }

        public static bool HandleSettingsOption(Client.GameClient client, byte option)
        {
            if (client == null || client.AutoHunting == null)
                return false;

            switch (option)
            {
                case 240:
                    client.AutoHunting.CycleRadius();
                    ShowSettings(client);
                    return true;
                case 241:
                    client.AutoHunting.UseSkills = !client.AutoHunting.UseSkills;
                    ShowSettings(client);
                    return true;
                case 242:
                    if (client.AutoHunting.Enable && client.AutoHunting.ExpDeliveryMode == AutoHunting.AutoHuntExpDelivery.OnStop)
                        FlushPendingExperience(client);
                    client.AutoHunting.ToggleExpDelivery();
                    ShowSettings(client);
                    return true;
                case 243:
                    client.AutoHunting.FastMode = !client.AutoHunting.FastMode;
                    ShowSettings(client);
                    return true;
                case 244:
                    ShowPickupSettings(client);
                    return true;
                case 245:
                    client.AutoHunting.HpPotionPercent = NextPotionThreshold(client.AutoHunting.HpPotionPercent);
                    ShowSettings(client);
                    return true;
                case 246:
                    client.AutoHunting.MpPotionPercent = NextPotionThreshold(client.AutoHunting.MpPotionPercent);
                    ShowSettings(client);
                    return true;
            }
            return false;
        }

        private static byte NextPotionThreshold(byte current)
        {
            switch (current)
            {
                case 0: return 20;
                case 20: return 30;
                case 30: return 40;
                case 40: return 50;
                case 50: return 60;
                case 60: return 70;
                case 70: return 80;
                default: return 0;
            }
        }

        public static void ShowPickupSettings(Client.GameClient client)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                dialog.AddText("VIP 4+ Auto Pick Up filters.");
                dialog.AddOption("Dragon Balls " + client.AutoHunting.DBallsStatus, 230);
                dialog.AddOption("Meteors " + client.AutoHunting.MeteorsStatus, 231);
                dialog.AddOption("Plus items " + client.AutoHunting.PlusItemsStatus, 232);
                dialog.AddOption("Quality items " + client.AutoHunting.QualityItemsStatus, 233);
                dialog.AddOption("Socketed " + client.AutoHunting.SocketedItemsStatus, 234);
                dialog.AddOption("Blessed " + client.AutoHunting.BlessedItemsStatus, 235);
                dialog.AddOption("Materials " + client.AutoHunting.MaterialItemsStatus, 236);
                dialog.AddOption("EXP/Event items " + client.AutoHunting.ExpBallEventItemsStatus, 237);
                dialog.AddOption("Silver " + client.AutoHunting.LootMoneyStatus, 238);
                dialog.AddOption("Back", 239);
                dialog.FinalizeDialog();
            }
        }

        public static bool HandlePickupSettingsOption(Client.GameClient client, byte option)
        {
            if (client == null || client.AutoHunting == null)
                return false;
            switch (option)
            {
                case 230: client.AutoHunting.DBalls = !client.AutoHunting.DBalls; break;
                case 231: client.AutoHunting.Meteors = !client.AutoHunting.Meteors; break;
                case 232: client.AutoHunting.PlusItems = !client.AutoHunting.PlusItems; break;
                case 233: client.AutoHunting.QualityItems = !client.AutoHunting.QualityItems; break;
                case 234: client.AutoHunting.SocketedItems = !client.AutoHunting.SocketedItems; break;
                case 235: client.AutoHunting.BlessedItems = !client.AutoHunting.BlessedItems; break;
                case 236: client.AutoHunting.MaterialItems = !client.AutoHunting.MaterialItems; break;
                case 237: client.AutoHunting.ExpBallEventItems = !client.AutoHunting.ExpBallEventItems; break;
                case 238: client.AutoHunting.LootMoney = !client.AutoHunting.LootMoney; break;
                case 239: ShowSettings(client); return true;
                default: return false;
            }
            ShowPickupSettings(client);
            return true;
        }

        public static void Start(Client.GameClient client)
        {
            if (!ValidClient(client))
                return;
            string CurrentClientIP = client.Socket.RemoteIp;
            int TotalAutohuntsWithSameIP = Pool.GamePoll.Where(x => x.Value.Socket != null && x.Value.Socket.RemoteIp == CurrentClientIP && x.Value.AutoHunting.Enable).Count();
            if (TotalAutohuntsWithSameIP >= 3)
            {
                client.CreateBoxDialog("Sorry, cannot use Autohunting with more than 3 accounts at same time.");
                return;
            }
            // Official Conquer Auto Hunt behavior (2013):
            // Auto Hunt is available to every player.
            // VIP 3+ changes movement to Auto Jump; VIP 4+ unlocks Auto Pick Up.
            // Keep the server authoritative for both privileges.
            {
                if (ValidCoord(client))
                {
                    client.AutoHunting.DirectionChange = 0;
                    client.AutoHunting.OriginX = client.Player.X;
                    client.AutoHunting.OriginY = client.Player.Y;
                    client.AutoHunting.X = 0;
                    client.AutoHunting.Y = 0;
                    client.AutoHunting.AttackStamp = DateTime.Now;
                    client.AutoHunting.Angle = (Role.Flags.ConquerAngle)Pool.GetRandom.Next(0, 7);
                    client.AutoHunting.Enable = true;
                    if (client.Player.VipLevel > 0)
                    {
                        if (client.Player.MyTitle != 9)
                        {
                            client.AutoHunting.Mytitle = client.Player.MyTitle;
                            client.Player.MyTitle = 9;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.Send(stream.TitleCreate(client.Player.UID, client.Player.MyTitle, MsgTitle.QueueTitle.Change));
                            }
                        }
                    }
                    else
                    {
                        if (client.Player.MyTitle != 10)
                        {
                            client.AutoHunting.Mytitle = client.Player.MyTitle;
                            client.Player.MyTitle = 10;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.Send(stream.TitleCreate(client.Player.UID, client.Player.MyTitle, MsgTitle.QueueTitle.Change));
                            }
                        }
                    }
                }
                else
                {
                    client.SendSysMesage("You~can't~use~autohunt~here.", MsgMessage.ChatMode.Whisper, MsgMessage.MsgColor.red);
                }
            }
        }
        public static void FlushPendingExperience(Client.GameClient client)
        {
            if (client == null || client.Player == null)
                return;

            ulong pending = client.AutoHunting.TakePendingExperience();
            if (pending == 0)
                return;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                // PendingExperience stores the final kill EXP value. Apply it without
                // recalculating server/gem/double-EXP multipliers at stop time.
                client.IncreaseExperienceRaw(stream, pending);
            }
        }

        public static void End(Client.GameClient client)
        {
            if (!ValidClient(client))
                return;
            FlushPendingExperience(client);
            client.AutoHunting.Enable = false;
            client.OnAutoAttack = false;
            client.Player.MyTitle = client.AutoHunting.Mytitle;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Player.Send(stream.TitleCreate(client.Player.UID, client.Player.MyTitle, MsgTitle.QueueTitle.Change));
            }
        }
        private unsafe static void CatchMob(Client.GameClient client)
        {
            if (!ValidClient(client))
                return;
            if (client != null && client.Map != null && client.Player.View != null && client.Player != null && client.Player.HitPoints > 0)
            {
                if (AutoPickUp(client))
                    return;
                if (DateTime.Now < client.AutoHunting.AttackStamp.AddMilliseconds(client.AutoHunting.FastMode ? 800 : 1000))
                    return;
                bool ExistMonsters = false;
                foreach (Role.IMapObj Obj in client.Player.View.Roles(Role.MapObjectType.Monster))
                {
                    var entity = Obj as Game.MsgMonster.MonsterRole;
                    if (entity.HitPoints > 0 && !entity.Name.Contains("Guard") && client.AutoHunting.IsInsideHuntRadius(entity.X, entity.Y))
                    {
                        ExistMonsters = true;
                        break;
                    }
                }
                client.AutoHunting.Angle = Role.Flags.ConquerAngle.South; //(Role.Flags.ConquerAngle)Program.GetRandom.Next(0, 7);
                int Xx = 2;//2
                int Count = client.Player.View.Roles(Role.MapObjectType.Monster).Count();
                if (Count > 0 && ExistMonsters)
                {
                    if (!client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                    {
                        foreach (Role.IMapObj Obj in client.Player.View.Roles(Role.MapObjectType.Monster))
                        {
                            if (client.Player.X == (ushort)(Obj.X - Xx) && client.Player.Y == Obj.Y) continue;

                            var entity = Obj as Game.MsgMonster.MonsterRole;
                            if (entity.HitPoints > 0 && !entity.Name.Contains("Guard") && client.AutoHunting.IsInsideHuntRadius(entity.X, entity.Y))
                            {
                                ushort X = (ushort)(Obj.X - Xx), Y = Obj.Y;
                                if (!client.AutoHunting.IsInsideHuntRadius(X, Y))
                                    continue;
                                Role.GameMap Map = Pool.ServerMaps[client.Map.ID];
                                if (client.Map.AddGroundItemWithAngle(ref X, ref Y, 0, client.AutoHunting.Angle))
                                {
                                    if (ValidCoord(client, X, Y, true))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                                            {
                                                Mode = AutoHunting.CanAutoJump(client.Player.VipLevel)
                                                    ? MsgInterAction.Action.Jump
                                                    : MsgInterAction.Action.Walk,
                                                X = X,
                                                Y = Y,
                                                UID = client.Player.UID,
                                                OponentUID = 1
                                            };
                                            client.Player.View.SendView(stream.InterActionWalk(&inter), true);
                                            client.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, X, Y);
                                            client.Player.Action = AutoHunting.CanAutoJump(client.Player.VipLevel)
                                                ? Role.Flags.ConquerAction.Jump
                                                : Role.Flags.ConquerAction.None;
                                            client.Map.View.MoveTo<Role.IMapObj>(client.Player, X, Y);
                                            client.Player.X = X;
                                            client.Player.Y = Y;
                                            client.Player.View.Role(false, stream);
                                            client.AutoHunting.DirectionChange = 0;
                                            client.Player.LastMove = DateTime.Now;
                                            client.AutoHunting.AttackStamp = DateTime.Now;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    ushort X = 0, Y = 0;
                    if (client.Player.Rate(50))
                    {
                        X = (ushort)(client.Player.X + RobotRandom.Next(5, 15));
                        Y = client.Player.Y;
                    }
                    else
                    {
                        X = client.Player.X;
                        Y = (ushort)(client.Player.Y + RobotRandom.Next(5, 15));
                    }
                    Role.GameMap Map = Pool.ServerMaps[client.Map.ID];
                    if (!client.AutoHunting.IsInsideHuntRadius(X, Y))
                    {
                        X = client.AutoHunting.OriginX;
                        Y = client.AutoHunting.OriginY;
                    }
                    if (client.Map.AddGroundItemWithAngle(ref X, ref Y, 0, client.AutoHunting.Angle) && client.AutoHunting.DirectionChange < 10)
                    {
                        if (ValidCoord(client, X, Y, true))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                                {
                                    Mode = AutoHunting.CanAutoJump(client.Player.VipLevel)
                                                    ? MsgInterAction.Action.Jump
                                                    : MsgInterAction.Action.Walk,
                                    X = X,
                                    Y = Y,
                                    UID = client.Player.UID,
                                    OponentUID = 1
                                };
                                client.Player.View.SendView(stream.InterActionWalk(&inter), true);
                                client.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, X, Y);
                                client.Player.Action = AutoHunting.CanAutoJump(client.Player.VipLevel)
                                                ? Role.Flags.ConquerAction.Jump
                                                : Role.Flags.ConquerAction.None;
                                client.Map.View.MoveTo<Role.IMapObj>(client.Player, X, Y);
                                client.Player.X = X;
                                client.Player.Y = Y;
                                client.Player.View.Role(false, stream);
                                client.Player.LastMove = DateTime.Now;
                            }
                        }
                        return;
                    }
                    else client.AutoHunting.DirectionChange++;

                    if (client.AutoHunting.DirectionChange > 10)
                    {
                        foreach (var Obj in client.Map.View.GetAllMapRoles(Role.MapObjectType.Monster))
                        {
                            var entity = Obj as Game.MsgMonster.MonsterRole;
                            if (entity.HitPoints > 0 && !entity.Name.Contains("Guard") && client.AutoHunting.IsInsideHuntRadius(entity.X, entity.Y))
                            {
                                Game.MsgServer.AttackHandler.Algoritms.InLineAlgorithm Line = new Game.MsgServer.AttackHandler.Algoritms.InLineAlgorithm(client.Player.X, Obj.X, client.Player.Y, Obj.Y, client.Map, 15, 0);
                                X = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].X; Y = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].Y;
                                if (!client.AutoHunting.IsInsideHuntRadius(X, Y))
                                    continue;
                                if (client.Map.AddGroundItemWithAngle(ref X, ref Y, 0, client.AutoHunting.Angle))
                                {
                                    if (ValidCoord(client, X, Y, true))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                                            {
                                                Mode = AutoHunting.CanAutoJump(client.Player.VipLevel)
                                                    ? MsgInterAction.Action.Jump
                                                    : MsgInterAction.Action.Walk,
                                                X = X,
                                                Y = Y,
                                                UID = client.Player.UID,
                                                OponentUID = 1
                                            };
                                            client.Player.View.SendView(stream.InterActionWalk(&inter), true);
                                            client.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, X, Y);
                                            client.Player.Action = AutoHunting.CanAutoJump(client.Player.VipLevel)
                                                ? Role.Flags.ConquerAction.Jump
                                                : Role.Flags.ConquerAction.None;
                                            client.Map.View.MoveTo<Role.IMapObj>(client.Player, X, Y);
                                            client.Player.X = X;
                                            client.Player.Y = Y;
                                            client.Player.View.Role(false, stream);
                                            client.Player.LastMove = DateTime.Now;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        private unsafe static bool MoveForAutoHunt(Client.GameClient client, ushort x, ushort y)
        {
            if (!client.AutoHunting.IsInsideHuntRadius(x, y) || !ValidCoord(client, x, y, true))
                return false;

            ushort targetX = x, targetY = y;
            client.AutoHunting.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, targetX, targetY);
            if (!client.Map.AddGroundItemWithAngle(ref targetX, ref targetY, 0, client.AutoHunting.Angle))
                return false;
            if (!client.AutoHunting.IsInsideHuntRadius(targetX, targetY))
                return false;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                {
                    Mode = AutoHunting.CanAutoJump(client.Player.VipLevel) ? MsgInterAction.Action.Jump : MsgInterAction.Action.Walk,
                    X = targetX,
                    Y = targetY,
                    UID = client.Player.UID,
                    OponentUID = 1
                };
                client.Player.View.SendView(stream.InterActionWalk(&inter), true);
                client.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, targetX, targetY);
                client.Player.Action = AutoHunting.CanAutoJump(client.Player.VipLevel) ? Role.Flags.ConquerAction.Jump : Role.Flags.ConquerAction.None;
                client.Map.View.MoveTo<Role.IMapObj>(client.Player, targetX, targetY);
                client.Player.X = targetX;
                client.Player.Y = targetY;
                client.Player.View.Role(false, stream);
                client.Player.LastMove = DateTime.Now;
            }
            return true;
        }

        private static bool AutoPickUp(Client.GameClient client)
        {
            if (!ValidClient(client) || !client.AutoHunting.Enable || !AutoHunting.CanAutoPickUp(client.Player.VipLevel))
            {
                if (client != null && client.AutoHunting != null)
                    client.AutoHunting.PursuingLoot = false;
                return false;
            }

            var floorItems = client.Map.View.Roles(Role.MapObjectType.Item, client.Player.X, client.Player.Y)
                .OfType<Game.MsgFloorItem.MsgItem>()
                .Where(item => client.AutoHunting.IsInsideHuntRadius(item.X, item.Y))
                .Where(item => client.AutoHunting.ShouldAutoPickUp(item))
                .OrderBy(item => client.AutoHunting.GetAutoPickUpPriority(item))
                .ThenBy(item => Role.Core.GetDistance(client.Player.X, client.Player.Y, item.X, item.Y))
                .ToArray();

            if (floorItems.Length == 0)
            {
                client.AutoHunting.PursuingLoot = false;
                return false;
            }

            var target = floorItems[0];
            int distance = Role.Core.GetDistance(client.Player.X, client.Player.Y, target.X, target.Y);

            // Pick up immediately when already close enough; otherwise movement toward
            // the selected loot gets one hunting tick before combat resumes.
            if (distance > 5)
            {
                client.AutoHunting.PursuingLoot = true;
                MoveForAutoHunt(client, target.X, target.Y);
                return true;
            }

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Game.MsgFloorItem.MsgItemPacket.TryAutoPickup(client, target, stream);
            }
            client.AutoHunting.PursuingLoot = false;
            return true;
        }

        private unsafe static void HitMob(Client.GameClient client)
        {
            if (!ValidClient(client))
                return;
            if (client.AutoHunting.PursuingLoot)
                return;
            if (client != null && client.Map != null && client.Player.View != null && client.Player != null && client.Player.HitPoints > 0)
            {
                foreach (Role.IMapObj Obj in client.Player.View.Roles(Role.MapObjectType.Monster))
                {
                    if (Role.Core.GetDistance(Obj.X, Obj.Y, client.Player.X, client.Player.Y) > Role.RoleView.ViewThreshold) continue;
                    var entity = Obj as Game.MsgMonster.MonsterRole;
                    if (entity.HitPoints > 0 && !entity.ContainFlag(MsgUpdate.Flags.Ghost) && !entity.Name.Contains("Guard") &&
                        client.AutoHunting.IsInsideHuntRadius(entity.X, entity.Y))
                    {
                        ushort SpellID = 0;
                        if (client.Player.Class >= 10 && client.Player.Class <= 15) SpellID = SkillRobotTrojan[RobotRandom.Next(SkillRobotTrojan.Length)];
                        if (client.Player.Class >= 40 && client.Player.Class <= 45) SpellID = SkillRobotArcher[RobotRandom.Next(SkillRobotArcher.Length)];
                        if (client.Player.Class >= 130 && client.Player.Class <= 135) SpellID = SkillRobotWater[RobotRandom.Next(SkillRobotWater.Length)];
                        if (client.Player.Class >= 140 && client.Player.Class <= 145) SpellID = SkillRobotFire[RobotRandom.Next(SkillRobotFire.Length)];
                        if (!client.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike) && client.Player.ContainFlag(MsgUpdate.Flags.XPList))
                        {
                            List<ushort> SkillsXP = new List<ushort>();
                            ushort SkillXP = 0;
                            for (int i = 0; i < SkillXPRobot.Length; i++)
                            {
                                if (client.MySpells.ClientSpells.ContainsKey(SkillXPRobot[i]))
                                    SkillsXP.Add(SkillXPRobot[i]);
                            }
                            if (SkillsXP.Count > 0)
                            {
                                SkillXP = SkillsXP[(ushort)RobotRandom.Next(SkillsXP.Count)];
                                if (SkillXP != 0)
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        InteractQuery action = new InteractQuery()
                                        {
                                            AtkType = MsgAttackPacket.AttackID.Magic,
                                            UID = client.Player.UID,
                                            OpponentUID = client.Player.UID,
                                            X = client.Player.X,
                                            Y = client.Player.Y,
                                            Damage = (int)SkillXP
                                        };
                                        MsgAttackPacket.Process(client, action);
                                    }
                                }
                            }
                        }

                        if (!client.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike) && client.AutoHunting.UseSkills)
                        {
                            Dictionary<ushort, Database.MagicType.Magic> Spells;
                            if (Pool.Magic.TryGetValue(SpellID, out Spells))
                            {
                                MsgSpell ClientSpell;
                                if (client.MySpells.ClientSpells.TryGetValue(SpellID, out ClientSpell))
                                {
                                    Database.MagicType.Magic spell;
                                    if (Spells.TryGetValue(ClientSpell.Level, out spell))
                                    {
                                        if (SpellID != 0 && spell != null && spell.UseStamina <= client.Player.Stamina && spell.UseMana <= client.Player.Mana)
                                        {
                                            if (client.Player.Rate(50) || client.Player.Class >= 130 && client.Player.Class <= 135 || client.Player.Class >= 140 && client.Player.Class <= 145)
                                            {
                                                if (!(client.AutoHunting.X == client.Player.X && client.AutoHunting.Y == client.Player.Y) || client.AutoHunting.X == 0 && client.AutoHunting.Y == 0)
                                                {
                                                    client.AutoHunting.X = client.Player.X;
                                                    client.AutoHunting.Y = client.Player.Y;
                                                    client.AutoHunting.AttackStamp = DateTime.Now;
                                                }

                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    InteractQuery action = new InteractQuery();
                                                    action.AtkType = MsgAttackPacket.AttackID.Magic;
                                                    action.UID = client.Player.UID;
                                                    if (SkillRobotAttacked.Contains(SpellID))
                                                        action.OpponentUID = Obj.UID;
                                                    action.X = Obj.X;
                                                    action.Y = Obj.Y;
                                                    action.Damage = (int)SpellID;
                                                    action.SpellID = (ushort)SpellID;
                                                    MsgAttackPacket.Process(client, action);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Role.Core.GetDistance(Obj.X, Obj.Y, client.Player.X, client.Player.Y) <= 2 || client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                        {
                            if (client.Player.Class != 135 && client.Player.Class != 145)
                            {
                                if (!(client.AutoHunting.X == client.Player.X && client.AutoHunting.Y == client.Player.Y) || client.AutoHunting.X == 0 && client.AutoHunting.Y == 0)
                                {
                                    client.AutoHunting.X = client.Player.X;
                                    client.AutoHunting.Y = client.Player.Y;
                                    client.AutoHunting.AttackStamp = DateTime.Now;
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    InteractQuery action = new InteractQuery();
                                    action.AtkType = MsgAttackPacket.AttackID.Physical;
                                    if (client.Player.Class >= 40 && client.Player.Class <= 45)
                                    {
                                        action.AtkType = MsgAttackPacket.AttackID.Magic;
                                        action.Damage = 8001;
                                        action.SpellID = 8001;
                                    }
                                    action.UID = client.Player.UID;
                                    action.OpponentUID = Obj.UID;
                                    action.X = Obj.X;
                                    action.Y = Obj.Y;
                                    MsgAttackPacket.Process(client, action);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        private unsafe void JumpHunting()
        {
            while (true)
            {
                try
                {
                    if (Auto)
                    {
                        foreach (Client.GameClient client in Pool.GamePoll.Values.Where(p => p.AutoHunting.Enable))
                        {
                            CatchMob(client);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                }
                Thread.Sleep(1000);
            }
        }
        private static void AutoUsePotions(Client.GameClient client)
        {
            if (!ValidClient(client) || !client.AutoHunting.Enable)
                return;

            int maxHp = (int)client.Status.MaxHitpoints;
            int maxMp = (int)client.Status.MaxMana;
            bool needHp = client.AutoHunting.HpPotionPercent > 0 && maxHp > 0 &&
                          client.Player.HitPoints * 100 <= maxHp * client.AutoHunting.HpPotionPercent;
            bool needMp = client.AutoHunting.MpPotionPercent > 0 && maxMp > 0 &&
                          client.Player.Mana * 100 <= maxMp * client.AutoHunting.MpPotionPercent;
            if (!needHp && !needMp)
                return;

            Game.MsgServer.MsgGameItem selected = null;
            Database.ItemType.DBItem selectedBase = null;
            foreach (var item in client.Inventory.ClientItems.Values)
            {
                Database.ItemType.DBItem dbItem;
                if (!Pool.ItemsBase.TryGetValue(item.ITEM_ID, out dbItem))
                    continue;

                bool matches = (needHp && dbItem.ItemHP > 0) || (needMp && dbItem.ItemMP > 0);
                if (!matches)
                    continue;

                // Prefer the smallest potion that still has useful recovery, preserving
                // stronger consumables when a weaker one is sufficient.
                if (selectedBase == null ||
                    (needHp && dbItem.ItemHP > 0 && dbItem.ItemHP < selectedBase.ItemHP) ||
                    (needMp && dbItem.ItemMP > 0 && dbItem.ItemMP < selectedBase.ItemMP))
                {
                    selected = item;
                    selectedBase = dbItem;
                }
            }

            if (selected == null || selectedBase == null)
                return;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (needHp && selectedBase.ItemHP > 0)
                {
                    client.Player.HitPoints = Math.Min(client.Player.HitPoints + selectedBase.ItemHP, maxHp);
                    client.Player.SendUpdate(stream, client.Player.HitPoints, MsgUpdate.DataType.Hitpoints, false);
                }
                if (needMp && selectedBase.ItemMP > 0)
                {
                    client.Player.Mana = (ushort)Math.Min(client.Player.Mana + selectedBase.ItemMP, maxMp);
                    client.Player.SendUpdate(stream, client.Player.Mana, MsgUpdate.DataType.Mana, false);
                }
                client.Inventory.Update(selected, Instance.AddMode.REMOVE, stream);
            }
        }

        private unsafe void SkillHunting()
        {
            while (true)
            {
                try
                {
                    if (Auto)
                    {
                        foreach (Client.GameClient client in Pool.GamePoll.Values.Where(p => p.AutoHunting.Enable))
                        {
                            if (client != null)
                            {
                                AutoUsePotions(client);
                                HitMob(client);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                }
                Thread.Sleep(1000);
            }
        }

    }
}
