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
        private Thread JumpPlayer, Skill, Revive;
        public Catching()
        {
            JumpPlayer = new Thread(new ThreadStart(JumpHunting));
            JumpPlayer.Start();

            Skill = new Thread(new ThreadStart(SkillHunting));
            Skill.Start();

            Revive = new Thread(new ThreadStart(ReviveHunting));
            Revive.Start();
        }
        public static bool Auto = true;
        private static Random RobotRandom = new Random();
        private static ushort[] SkillRobotTrojan = new ushort[] { 1045, 1046, 1115 };//FastBlade,ScentSword,Hercules
        private static ushort[] SkillRobotArcher = new ushort[] { 8001 };//Scatter
        private static ushort[] SkillRobotNinja = new ushort[] { 6000 };//TwofoldBlades
        private static ushort[] SkillRobotMonk = new ushort[] { 10381, 10415 };//RadiantPalm,WhirlwindKick
        private static ushort[] SkillRobotWater = new ushort[] { 1000 };//Thunder
        private static ushort[] SkillRobotFire = new ushort[] { 1000, 1002 };//Tornado
        private static ushort[] SkillPirate = new ushort[] { 11110, 11070, 11030 };

        private static ushort[] SkillRobotAttacked = new ushort[] { 6000, 10381, 10415, 1000, 1002 };
        private static ushort[] SkillXPRobot = new ushort[] { 1110, 6011 };//CycloneXP FatalStrike
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
        public static void End(Client.GameClient client)
        {
            if (!ValidClient(client))
                return;
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
                if (DateTime.Now < client.AutoHunting.AttackStamp.AddMilliseconds(client.AutoHunting.FastMode ? 800 : 1000))
                    return;
                bool ExistMonsters = false;
                foreach (Role.IMapObj Obj in client.Player.View.Roles(Role.MapObjectType.Monster))
                {
                    var entity = Obj as Game.MsgMonster.MonsterRole;
                    if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
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
                            if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
                            {
                                ushort X = (ushort)(Obj.X - Xx), Y = Obj.Y;
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
                                                : Role.Flags.ConquerAction.Walk;
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
                                                : Role.Flags.ConquerAction.Walk;
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
                            if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
                            {
                                Game.MsgServer.AttackHandler.Algoritms.InLineAlgorithm Line = new Game.MsgServer.AttackHandler.Algoritms.InLineAlgorithm(client.Player.X, Obj.X, client.Player.Y, Obj.Y, client.Map, 15, 0);
                                X = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].X; Y = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].Y;
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
                                                : Role.Flags.ConquerAction.Walk;
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
        private static void AutoPickUp(Client.GameClient client)
        {
            if (!ValidClient(client) || !client.AutoHunting.Enable)
                return;
            if (!AutoHunting.CanAutoPickUp(client.Player.VipLevel))
                return;

            // Snapshot first because a successful pickup removes the object from MapView.
            var floorItems = client.Map.View.Roles(Role.MapObjectType.Item, client.Player.X, client.Player.Y)
                .OfType<Game.MsgFloorItem.MsgItem>()
                .Where(item => Role.Core.GetDistance(client.Player.X, client.Player.Y, item.X, item.Y) <= 5)
                .OrderBy(item => Role.Core.GetDistance(client.Player.X, client.Player.Y, item.X, item.Y))
                .ToArray();

            if (floorItems.Length == 0)
                return;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var item in floorItems)
                {
                    if (client.AutoHunting.ShouldAutoPickUp(item))
                        Game.MsgFloorItem.MsgBuilder.TryAutoPickup(client, item, stream);
                }
            }
        }

        private unsafe static void HitMob(Client.GameClient client)
        {
            if (!ValidClient(client))
                return;
            if (client != null && client.Map != null && client.Player.View != null && client.Player != null && client.Player.HitPoints > 0)
            {
                foreach (Role.IMapObj Obj in client.Player.View.Roles(Role.MapObjectType.Monster))
                {
                    if (Role.Core.GetDistance(Obj.X, Obj.Y, client.Player.X, client.Player.Y) > Role.RoleView.ViewThreshold) continue;
                    var entity = Obj as Game.MsgMonster.MonsterRole;
                    if (entity.HitPoints > 0 && !entity.ContainFlag(MsgUpdate.Flags.Ghost) && !entity.Name.Contains("Guard"))
                    {
                        ushort SpellID = 0;
                        if (client.Player.Class >= 10 && client.Player.Class <= 15) SpellID = SkillRobotTrojan[RobotRandom.Next(SkillRobotTrojan.Length)];
                        if (client.Player.Class >= 40 && client.Player.Class <= 45) SpellID = SkillRobotArcher[RobotRandom.Next(SkillRobotArcher.Length)];
                        if (client.Player.Class >= 50 && client.Player.Class <= 55) SpellID = SkillRobotNinja[RobotRandom.Next(SkillRobotNinja.Length)];
                        if (client.Player.Class >= 60 && client.Player.Class <= 65) SpellID = SkillRobotMonk[RobotRandom.Next(SkillRobotMonk.Length)];
                        if (client.Player.Class >= 130 && client.Player.Class <= 135) SpellID = SkillRobotWater[RobotRandom.Next(SkillRobotWater.Length)];
                        if (client.Player.Class >= 140 && client.Player.Class <= 145) SpellID = SkillRobotFire[RobotRandom.Next(SkillRobotFire.Length)];
                        if (client.Player.Class >= 70 && client.Player.Class <= 75) SpellID = SkillPirate[RobotRandom.Next(SkillPirate.Length)];
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

                        if (!client.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
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
                                AutoPickUp(client);
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
        private void ReviveHunting()
        {
            while (true)
            {
                try
                {
                    if (Auto)
                    {
                        foreach (Client.GameClient client in Pool.GamePoll.Values.Where(p => p.AutoHunting.Enable))
                        {
                            if (client != null && client.Map != null && client.Player.View != null && client.Player != null)
                            {
                                #region Revive
                                if (client.Player.ContainFlag(MsgUpdate.Flags.Ghost) && DateTime.Now > client.Player.DeadStamp.AddSeconds(20))
                                {
                                    client.Player.Action = Role.Flags.ConquerAction.None;
                                    client.Player.TransformationID = 0;
                                    client.Player.RemoveFlag(MsgUpdate.Flags.Dead);
                                    client.Player.RemoveFlag(MsgUpdate.Flags.Ghost);
                                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                                }
                                #endregion
                                if (client.Player.HitPoints > 0)
                                {
                                    #region Hitpoints
                                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Ghost) && client.Player.HitPoints < client.Status.MaxHitpoints)
                                        client.Player.HitPoints = Math.Min(client.Player.HitPoints + 3000, (int)client.Status.MaxHitpoints);
                                    #endregion
                                    #region Mana
                                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Ghost) && client.Player.Mana < client.Status.MaxMana)
                                        client.Player.Mana = (ushort)Math.Min(client.Player.Mana + 3000, client.Status.MaxMana);
                                    #endregion
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                }
                Thread.Sleep(3000);
            }
        }
    }
}
