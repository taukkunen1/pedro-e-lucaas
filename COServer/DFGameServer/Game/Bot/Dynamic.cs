using GameServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Bot
{
    class Dynamic
    {
        private static ushort[] SkillRobotTrojan = new ushort[] { 1045, 1046, 1115 };//FastBlade,ScentSword,Hercules
        private static ushort[] SkillRobotArcher = new ushort[] { 8001 };//Scatter
        private static ushort[] SkillRobotNinja = new ushort[] { 6000/*, 6001 */};//TwofoldBlades,ToxicFog
        private static ushort[] SkillRobotMonk = new ushort[] { 10381, 10415 };//RadiantPalm,WhirlwindKick
        private static ushort[] SkillRobotWater = new ushort[] { 1000 };//Thunder
        private static ushort[] SkillRobotFire = new ushort[] { 1002 };//Tornado

        private static ushort[] SkillRobotAttacked = new ushort[] { 6000, 10381, 10415, 1000, 1002 };
        private static ushort[] SkillXPRobot = new ushort[] { 1110, 6011 };//CycloneXP FatalStrike
        public unsafe static void Jumb_DoWork(AI bot)
        {
            if (bot.BEntity != null)
            {
                ushort _x = 2;
                bool ExistMonsters = false;
                foreach (Role.IMapObj Obj in bot.BEntity.Player.View.Roles(Role.MapObjectType.Monster))
                {
                    var entity = Obj as Game.MsgMonster.MonsterRole;
                    if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
                    {
                        ExistMonsters = true;
                        break;
                    }
                }
                int Count = bot.BEntity.Player.View.Roles(Role.MapObjectType.Monster).Count();
                if (Count > 0 && ExistMonsters)
                {
                    foreach (Role.IMapObj Obj in bot.BEntity.Player.View.Roles(Role.MapObjectType.Monster))
                    {
                        if (bot.BEntity.Player.X == (ushort)(Obj.X - _x) && bot.BEntity.Player.Y == Obj.Y) continue;
                        var entity = Obj as Game.MsgMonster.MonsterRole;
                        if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
                        {
                            ushort X = (ushort)(Obj.X - _x), Y = Obj.Y;
                            Role.GameMap Map = Pool.ServerMaps[bot.BEntity.Map.ID];
                            if (bot.BEntity.Map.AddGroundItem(ref X, ref Y, 0))
                            {
                                if (ValidCoord(bot, X, Y, true))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                                        {
                                            Mode = MsgInterAction.Action.Jump,
                                            X = X,
                                            Y = Y,
                                            UID = bot.BEntity.Player.UID,
                                            OponentUID = 1
                                        };
                                        bot.BEntity.Player.View.SendView(stream.InterActionWalk(&inter), true);
                                        bot.BEntity.Player.Angle = Role.Core.GetAngle(bot.BEntity.Player.X, bot.BEntity.Player.Y, X, Y);
                                        bot.BEntity.Player.Action = Role.Flags.ConquerAction.Jump;
                                        bot.BEntity.Map.View.MoveTo<Role.IMapObj>(bot.BEntity.Player, X, Y);
                                        bot.BEntity.Player.X = X;
                                        bot.BEntity.Player.Y = Y;
                                        bot.BEntity.Player.View.Role(false, stream);
                                        bot.BEntity.DirectionChange = 0;
                                        bot.BEntity.Player.LastMove = DateTime.Now;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    ushort X = 0, Y = 0;
                    if (bot.BEntity.Player.Rate(50))
                    {
                        X = (ushort)(bot.BEntity.Player.X + Pool.GetRandom.Next(5, 15));
                        Y = bot.BEntity.Player.Y;
                    }
                    else
                    {
                        X = bot.BEntity.Player.X;
                        Y = (ushort)(bot.BEntity.Player.Y + Pool.GetRandom.Next(5, 15));
                    }
                    Role.GameMap Map = Pool.ServerMaps[bot.BEntity.Map.ID];
                    if (bot.BEntity.Map.AddGroundItem(ref X, ref Y) && bot.BEntity.DirectionChange < 10)
                    {
                        if (ValidCoord(bot, X, Y, true))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                                {
                                    Mode = MsgInterAction.Action.Jump,
                                    X = X,
                                    Y = Y,
                                    UID = bot.BEntity.Player.UID,
                                    OponentUID = 1
                                };
                                bot.BEntity.Player.View.SendView(stream.InterActionWalk(&inter), true);
                                bot.BEntity.Player.Angle = Role.Core.GetAngle(bot.BEntity.Player.X, bot.BEntity.Player.Y, X, Y);
                                bot.BEntity.Player.Action = Role.Flags.ConquerAction.Jump;
                                bot.BEntity.Map.View.MoveTo<Role.IMapObj>(bot.BEntity.Player, X, Y);
                                bot.BEntity.Player.X = X;
                                bot.BEntity.Player.Y = Y;
                                bot.BEntity.Player.View.Role(false, stream);
                                bot.BEntity.Player.LastMove = DateTime.Now;
                            }
                        }
                        return;
                    }
                    else bot.BEntity.DirectionChange++;
                    if (bot.BEntity.DirectionChange > 10)
                    {
                        foreach (var Obj in bot.BEntity.Map.View.GetAllMapRoles(Role.MapObjectType.Monster))
                        {
                            var entity = Obj as Game.MsgMonster.MonsterRole;
                            if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
                            {
                                Game.MsgServer.AttackHandler.Algoritms.InLineAlgorithm Line = new Game.MsgServer.AttackHandler.Algoritms.InLineAlgorithm(bot.BEntity.Player.X, Obj.X, bot.BEntity.Player.Y, Obj.Y, bot.BEntity.Map, 15, 0);
                                X = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].X; Y = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].Y;
                                if (bot.BEntity.Map.AddGroundItem(ref X, ref Y/*, 0, bot.BEntity.AutoHunting.Angle*/))
                                {
                                    if (ValidCoord(bot, X, Y, true))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            Game.MsgServer.InterActionWalk inter = new Game.MsgServer.InterActionWalk()
                                            {
                                                Mode = MsgInterAction.Action.Jump,
                                                X = X,
                                                Y = Y,
                                                UID = bot.BEntity.Player.UID,
                                                OponentUID = 1
                                            };
                                            bot.BEntity.Player.View.SendView(stream.InterActionWalk(&inter), true);
                                            bot.BEntity.Player.Angle = Role.Core.GetAngle(bot.BEntity.Player.X, bot.BEntity.Player.Y, X, Y);
                                            bot.BEntity.Player.Action = Role.Flags.ConquerAction.Jump;
                                            bot.BEntity.Map.View.MoveTo<Role.IMapObj>(bot.BEntity.Player, X, Y);
                                            bot.BEntity.Player.X = X;
                                            bot.BEntity.Player.Y = Y;
                                            bot.BEntity.Player.View.Role(false, stream);
                                            bot.BEntity.Player.LastMove = DateTime.Now;
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
        public unsafe static void Hit_DoWork(AI bot)
        {
            if (bot.BEntity != null)
            {
                bool isbow = Database.ItemType.IsBow(bot.BEntity.Equipment.RightWeapon);
                foreach (Role.IMapObj Obj in bot.BEntity.Player.View.Roles(Role.MapObjectType.Monster))
                {
                    if (Role.Core.GetDistance(Obj.X, Obj.Y, bot.BEntity.Player.X, bot.BEntity.Player.Y) > 8) continue;
                    var entity = Obj as Game.MsgMonster.MonsterRole;
                    if (entity.HitPoints > 0 && !entity.ContainFlag(MsgUpdate.Flags.Ghost) && !entity.Name.Contains("Guard"))
                    {
                        ushort SpellID = 0;
                        if (bot.BEntity.Player.Class >= 10 && bot.BEntity.Player.Class <= 15)
                            SpellID = SkillRobotTrojan[Pool.GetRandom.Next(SkillRobotTrojan.Length)];
                        if (bot.BEntity.Player.Class >= 40 && bot.BEntity.Player.Class <= 45) 
                            SpellID = SkillRobotArcher[Pool.GetRandom.Next(SkillRobotArcher.Length)];
                        if (bot.BEntity.Player.Class >= 50 && bot.BEntity.Player.Class <= 55)
                            SpellID = SkillRobotNinja[Pool.GetRandom.Next(SkillRobotNinja.Length)];
                        if (bot.BEntity.Player.Class >= 60 && bot.BEntity.Player.Class <= 65)
                            SpellID = SkillRobotMonk[Pool.GetRandom.Next(SkillRobotMonk.Length)];
                        if (bot.BEntity.Player.Class >= 130 && bot.BEntity.Player.Class <= 135) 
                            SpellID = SkillRobotWater[Pool.GetRandom.Next(SkillRobotWater.Length)];
                        if (bot.BEntity.Player.Class >= 140 && bot.BEntity.Player.Class <= 145) 
                            SpellID = SkillRobotFire[Pool.GetRandom.Next(SkillRobotFire.Length)];

                        if (!bot.BEntity.MySpells.ClientSpells.ContainsKey(SpellID))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                bot.BEntity.MySpells.Add(stream, SpellID);
                            }
                        }

                        if (!bot.BEntity.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !bot.BEntity.Player.ContainFlag(MsgUpdate.Flags.FatalStrike) && bot.BEntity.Player.ContainFlag(MsgUpdate.Flags.XPList))
                        {
                            List<ushort> SkillsXP = new List<ushort>();
                            ushort SkillXP = 0;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                for (int i = 0; i < SkillXPRobot.Length; i++)
                                {
                                    if (bot.BEntity.MySpells.ClientSpells.ContainsKey(SkillXPRobot[i]))
                                        SkillsXP.Add(SkillXPRobot[i]);
                                    else
                                    {
                                        bot.BEntity.MySpells.Add(stream, SkillXP);
                                        SkillsXP.Add(SkillXPRobot[i]);
                                    }
                                }
                            }
                            if (SkillsXP.Count > 0)
                            {
                                SkillXP = SkillsXP[(ushort)Pool.GetRandom.Next(SkillsXP.Count)];
                                if (SkillXP != 0)
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        InteractQuery action = new InteractQuery()
                                        {
                                            AtkType = MsgAttackPacket.AttackID.Magic,
                                            UID = bot.BEntity.Player.UID,
                                            OpponentUID = bot.BEntity.Player.UID,
                                            X = bot.BEntity.Player.X,
                                            Y = bot.BEntity.Player.Y,
                                            Damage = (int)SkillXP
                                        };
                                        MsgAttackPacket.Process(bot.BEntity, action);
                                    }
                                }
                            }
                        }

                        if (!bot.BEntity.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !bot.BEntity.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                        {
                            Dictionary<ushort, Database.MagicType.Magic> Spells;
                            if (Pool.Magic.TryGetValue(SpellID, out Spells))
                            {
                                MsgSpell ClientSpell;
                                if (bot.BEntity.MySpells.ClientSpells.TryGetValue(SpellID, out ClientSpell))
                                {
                                    Database.MagicType.Magic spell;
                                    if (Spells.TryGetValue(ClientSpell.Level, out spell))
                                    {
                                        if (SpellID != 0 && spell != null && spell.UseStamina <= bot.BEntity.Player.Stamina && spell.UseMana <= bot.BEntity.Player.Mana)
                                        {
                                            if (bot.BEntity.Player.Rate(50) || bot.BEntity.Player.Class >= 130 && bot.BEntity.Player.Class <= 135 || bot.BEntity.Player.Class >= 140 && bot.BEntity.Player.Class <= 145)
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    InteractQuery action = new InteractQuery();
                                                    action.AtkType = MsgAttackPacket.AttackID.Magic;
                                                    action.UID = bot.BEntity.Player.UID;
                                                    if (SkillRobotAttacked.Contains(SpellID))
                                                        action.OpponentUID = Obj.UID;
                                                    action.X = Obj.X;
                                                    action.Y = Obj.Y;
                                                    action.Damage = (int)SpellID;
                                                    action.SpellID = (ushort)SpellID;
                                                    MsgAttackPacket.Process(bot.BEntity, action);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Role.Core.GetDistance(Obj.X, Obj.Y, bot.BEntity.Player.X, bot.BEntity.Player.Y) <= 2 || bot.BEntity.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                        {
                            if (bot.BEntity.Player.Class != 135 && bot.BEntity.Player.Class != 145)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    InteractQuery action = new InteractQuery();
                                    action.AtkType = MsgAttackPacket.AttackID.Physical;
                                    if (bot.BEntity.Player.Class >= 40 && bot.BEntity.Player.Class <= 45)
                                    {
                                        if (isbow)
                                        {
                                            action.AtkType = MsgAttackPacket.AttackID.Magic;
                                            action.Damage = 8001;
                                            action.SpellID = 8001;
                                        }
                                    }
                                    action.UID = bot.BEntity.Player.UID;
                                    action.OpponentUID = Obj.UID;
                                    action.X = Obj.X;
                                    action.Y = Obj.Y;
                                    MsgAttackPacket.Process(bot.BEntity, action);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        public static bool ValidCoord(AI bot, ushort X = 0, ushort Y = 0, bool NextDit = false)
        {
            if (bot.BEntity.Map.ID == 1000)
            {
                if (NextDit)
                {
                    if ((X > 468 && X < 544) && (Y > 525 && Y < 696))
                        return false;
                }
                else
                {
                    if ((bot.BEntity.Player.X > 468 && bot.BEntity.Player.X < 544) && (bot.BEntity.Player.Y > 525 && bot.BEntity.Player.Y < 696))
                        return false;
                }
            }
            if (bot.BEntity.Map.ID == 1002)
            {
                if (NextDit)
                {
                    if ((X > 349 && X < 508) && (Y > 212 && Y < 432))
                        return false;
                }
                else
                {
                    if ((bot.BEntity.Player.X > 349 && bot.BEntity.Player.X < 508) && (bot.BEntity.Player.Y > 212 && bot.BEntity.Player.Y < 432))
                        return false;
                }
            }
            if (bot.BEntity.Map.ID == 1011)
            {
                if (NextDit)
                {
                    if ((X > 151 && X < 254) && (Y > 195 && Y < 305))
                        return false;
                }
                else
                {
                    if ((bot.BEntity.Player.X > 151 && bot.BEntity.Player.X < 254) && (bot.BEntity.Player.Y > 195 && bot.BEntity.Player.Y < 305))
                        return false;
                }
            }
            if (bot.BEntity.Map.ID == 1015)
            {
                if (NextDit)
                {
                    if ((X > 684 && X < 782) && (Y > 509 && Y < 617))
                        return false;
                }
                else
                {
                    if ((bot.BEntity.Player.X > 684 && bot.BEntity.Player.X < 782) && (bot.BEntity.Player.Y > 509 && bot.BEntity.Player.Y < 617))
                        return false;
                }
            }
            if (bot.BEntity.Map.ID == 1020)
            {
                if (NextDit)
                {
                    if ((X > 542 && X < 590) && (Y > 544 && Y < 616))
                        return false;
                }
                else
                {
                    if ((bot.BEntity.Player.X > 542 && bot.BEntity.Player.X < 590) && (bot.BEntity.Player.Y > 544 && bot.BEntity.Player.Y < 616))
                        return false;
                }
            }
            return true;
        }
    }
}
