using Core;
using Core.Extensions.Random;
using GameServer.Game.MsgServer;
using System;
using System.Collections.Generic;

namespace GameServer.Game.MsgMonster
{
    public class ActionHandler
    {
        public MSRandom Random = new MSRandom();
        public Random SystemRandom = new Random();

        public bool Rate(int value)
        {
            return value > Random.Next() % 100;
        }
        public void ExecuteAction(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (monster.Family.ID == 40121 || monster.Family.ID == 40122 || monster.Family.ID == 40123 || monster.Family.ID == 40124)
                return;
            switch (monster.State)
            {
                case MobStatus.MoveNext:
                    {
                        CheckTarget(client, monster);
                        break;
                    }
                case MobStatus.Idle:
                    {
                        monster.Target = null;
                        CheckTarget(client, monster);
                        break;
                    }
                case MobStatus.SearchTarget: SearchTarget(client, monster); break;
                case MobStatus.Attacking: AttackingTarget(client, monster); break;
            }
        }
        public unsafe void CheckGuardPosition(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (monster.Alive)
            {
                if (monster.X != monster.RespawnX || monster.Y != monster.RespawnY)
                {
                    if (DateTime.Now > monster.MoveStamp.AddMilliseconds(300))
                    {
                        monster.MoveStamp = DateTime.Now;

                        Role.Flags.ConquerAngle dir = GetAngle(monster.X, monster.Y, monster.RespawnX, monster.RespawnY);
                        ushort WalkX = monster.X; ushort WalkY = monster.Y;
                        IncXY(dir, ref WalkX, ref WalkY);

                        client.Owner.Map.View.MoveTo<Role.IMapObj>(monster, WalkX, WalkY);

                        monster.X = WalkX;
                        monster.Y = WalkY;

                        WalkQuery walk = new WalkQuery()
                        {
                            Direction = (byte)dir,
                            Running = 1,
                            UID = monster.UID
                        };
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            monster.Send(stream.MovementCreate(&walk));
                        }
                    }
                }
            }
        }
        public unsafe bool GuardAttackPlayer(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (!monster.Alive)
                return false;
            if (client.ContainFlag(MsgServer.MsgUpdate.Flags.FlashingName) && client.Alive)
            {
                short distance = MonsterView.GetDistance(client.X, client.Y, monster.X, monster.Y);
                if (distance < monster.Family.AttackRange)
                {
                    if (!CheckRespouseDamage(client, monster))
                    {
                        uint Damage = MagicAttack(client.Owner, monster);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) == MsgMonster.MonsterSettings.Guard)
                                Damage = 1000;
                            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                , 0, client.X, client.Y, (ushort)monster.Family.SpellId, 0, 0);
                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(client.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                            SpellPacket.SetStream(stream);
                            SpellPacket.Send(monster);
                        }
                        CheckForOponnentDead(client, Damage, monster);
                        return true;
                    }
                }
            }
            return false;
        }
        public unsafe bool GuardAttackMonster(Role.GameMap map, Game.MsgMonster.MonsterRole attacked, Game.MsgMonster.MonsterRole monster)
        {
            if (!monster.Alive)
                return false;
            if (attacked.Alive)
            {
                short distance = MonsterView.GetDistance(attacked.X, attacked.Y, monster.X, monster.Y);
                if (distance < monster.Family.AttackRange)
                {
                    uint Damage = MagicAttack(attacked, monster);
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        MsgSpellAnimation SpellPacket = new MsgSpellAnimation(monster.UID
                            , 0, attacked.X, attacked.Y, (ushort)monster.Family.SpellId, 0, 0);
                        SpellPacket.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, Damage, MsgAttackPacket.AttackEffect.None));
                        SpellPacket.SetStream(stream);
                        SpellPacket.Send(monster);

                        if (Damage >= attacked.HitPoints)
                        {
                            map.SetMonsterOnTile(attacked.X, attacked.Y, false);
                            attacked.Dead(stream, null, monster.UID, map);
                        }
                        else
                            attacked.HitPoints -= Damage;

                    }
                    return true;
                }
            }
            return false;
        }
        public bool ExtraBoss(Game.MsgMonster.MonsterRole monster)
        {
            if (monster.Family.ID == 4145 || monster.Family.ID == 3738)
                return false;
            return monster.Family.MaxHealth > 300000 && monster.Family.MaxHealth < 7000000;
        }
        public unsafe void AttackingTarget(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            try
            {
                if (monster == null)
                    return;
                if (!monster.Alive)
                    return;
                if (monster.Target == null)
                    return;

                short distance = MonsterView.GetDistance(monster.Target.X, monster.Target.Y, monster.X, monster.Y);

                if (monster.Boss == 1 && monster.HitPoints >= 2000000)
                    monster.Family.AttackRange = 18;

                if (distance > monster.Family.AttackRange || monster.Target == null || !monster.Target.Alive || monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly)
                    || monster.Target.Owner.Socket != null && !monster.Target.Owner.Socket.Alive)
                {
                    monster.State = MobStatus.SearchTarget;
                }
                else
                {
                    if (DateTime.Now > monster.AttackSpeed.AddMilliseconds(monster.Family.AttackSpeed))
                    {
                        monster.HitTargetState();
                        monster.AttackSpeed = DateTime.Now;
                        if (ExtraBoss(monster))
                        {
                            if (!CheckRespouseDamage(client, monster))
                            {
                                ushort SpellID = 9999;
                                if (monster.Level < 80)
                                    SpellID = 9998;
                                else if (monster.Level < 60)
                                    SpellID = 9966;

                                uint Damage = MagicAttack(monster.Target.Owner, monster);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                        , 0, monster.Target.X, monster.Target.Y, (ushort)SpellID, 0, 0);
                                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                    SpellPacket.SetStream(stream);
                                    SpellPacket.Send(monster);
                                }
                                CheckForOponnentDead(monster.Target, Damage, monster);
                            }
                            return;
                        }
                        if (monster.Boss == 0 && monster.Family.SpellId == 0)
                        {
                            if (!CheckRespouseDamage(client, monster))
                            {
                                uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                Damage = CheckDodge(monster.Target.Owner.Status.Dodge) ? Damage : 0;
                                if (Damage >= 1)
                                {
                                    MsgServer.AttackHandler.CheckAttack.CheckItems.RespouseDurability(monster.Target.Owner);
                                }
                                InteractQuery action = new InteractQuery()
                                {
                                    AtkType = MsgAttackPacket.AttackID.Physical,
                                    X = monster.Target.X,
                                    Y = monster.Target.Y,
                                    UID = monster.UID,
                                    OpponentUID = monster.Target.UID,
                                    Damage = (int)Damage
                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    monster.Send(stream.InteractionCreate(&action));
                                }

                                CheckForOponnentDead(monster.Target, Damage, monster);
                            }
                        }
                        else if (monster.Family.SpellId != 0 && monster.Boss == 0)
                        {
                            if (!CheckRespouseDamage(client, monster))
                            {
                                uint Damage = MagicAttack(monster.Target.Owner, monster);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                        , 0, monster.Target.X, monster.Target.Y, (ushort)monster.Family.SpellId, 0, 0);
                                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                    SpellPacket.SetStream(stream);
                                    SpellPacket.Send(monster);
                                }

                                CheckForOponnentDead(monster.Target, Damage, monster);
                            }
                        }
                        else if (monster.Boss != 0)
                        {
                            if (!monster.Target.Alive || Role.Core.GetDistance(monster.X, monster.Y, monster.Target.X, monster.Target.Y) > 18)
                            {
                                monster.State = MobStatus.SearchTarget;
                                return;
                            }
                            switch (monster.Family.ID)
                            {
                                case 3737:
                                case 40946:
                                case 40936:
                                case 20160:
                                    {
                                        List<ushort> Spells = new List<ushort>() { 8003, 11311, 10364, 11309, 11310 };
                                        ushort rand = (byte)Pool.GetRandom.Next(0, Spells.Count);
                                        switch (Spells[rand])
                                        {
                                            case 10364:
                                            case 8003:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);


                                                        CheckForOponnentDead(monster.Target, Damage, monster);
                                                    }
                                                    break;
                                                }

                                            case 11311:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.ResistEarth));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);


                                                        CheckForOponnentDead(monster.Target, Damage, monster);
                                                    }
                                                    break;
                                                }
                                            case 11309:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();

                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                           , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.X, monster.Y, player.X, player.Y) <= 7)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                            case 11310:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 3)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(10) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 5, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                case 40938:
                                case 40948:
                                case 20300:
                                case 40940:
                                case 20136:
                                case 20135:
                                case 20021:
                                case 3130:
                                case 20020:
                                case 20070://snow bamshee
                                    {
                                        List<ushort> Spells = new List<ushort>() { 30010, 30011, 30012, 30013, 30014, 10372 };
                                        ushort rand = (byte)Pool.GetRandom.Next(0, Spells.Count);
                                        switch (Spells[rand])
                                        {
                                            case 30010:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();

                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                           , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.X, monster.Y, player.X, player.Y) <= 14)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                            case 30011:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();

                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                           , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 8)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                            case 30012:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 3)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(10) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 5, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                            case 30014:
                                            case 30013:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);


                                                        CheckForOponnentDead(monster.Target, Damage, monster);
                                                    }
                                                    if (Rate(5) && !monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                                        monster.Target.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);
                                                    break;
                                                }
                                            case 10372:
                                                {
                                                    if (monster.Family.ID != 3130)
                                                    {
                                                        uint Damage = 80000;
                                                        monster.HitPoints = (uint)Math.Min(monster.Family.MaxHealth, (int)(monster.HitPoints + Damage));
                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                        {
                                                            var stream = rec.GetStream();
                                                            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                            , 0, monster.X, monster.Y, (ushort)Spells[rand], 0, 0);
                                                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                            SpellPacket.SetStream(stream);
                                                            SpellPacket.Send(monster);
                                                        }
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                case 213883://chaos Guard
                                    {
                                        List<ushort> Spells = new List<ushort>() { 7013, 7017, 10362, 10531, 11322 };
                                        ushort rand = (byte)Pool.GetRandom.Next(0, Spells.Count);
                                        switch (Spells[rand])
                                        {
                                            case 11322:
                                                {
                                                    uint Damage = PhysicalAttack(monster.Target.Owner, monster);

                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    CheckForOponnentDead(monster.Target, Damage, monster);

                                                    if (Rate(5) && !monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                                        monster.Target.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);
                                                    break;
                                                }
                                            case 10531:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 18)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Dizzy))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Frightened, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);

                                                    }
                                                    break;
                                                }
                                            case 10362:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 10)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Dizzy))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Frightened, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);

                                                    }
                                                    break;
                                                }
                                            case 7013:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 18)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Dizzy))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Frightened, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);

                                                    }
                                                    break;
                                                }
                                            case 7017:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();

                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                           , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 18)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                case 20101:
                                case 40937:
                                case 40947:
                                case 40939:
                                case 40949:
                                case 20060://terato Dragon
                                case 4145:
                                    {
                                        List<ushort> Spells = new List<ushort>() { 7013, 7014, 7015, 7016, 7017 };
                                        ushort rand = (byte)Pool.GetRandom.Next(0, Spells.Count);
                                        switch (Spells[rand])
                                        {
                                            case 7017:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();

                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                           , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 8)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                            case 7014:
                                            case 7013:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 6)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Dizzy))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Frightened, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);

                                                    }
                                                    break;
                                                }
                                            case 7015:
                                                {
                                                    uint Damage = PhysicalAttack(monster.Target.Owner, monster);

                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    CheckForOponnentDead(monster.Target, Damage, monster);

                                                    if (Rate(5) && !monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                                        monster.Target.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);
                                                    break;
                                                }
                                            case 7016:
                                                {
                                                    if (monster.Family.ID != 4145)
                                                    {
                                                        uint Damage = 80000;
                                                        monster.HitPoints = (uint)Math.Min(monster.Family.MaxHealth, (int)(monster.HitPoints + Damage));
                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                        {
                                                            var stream = rec.GetStream();
                                                            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                            , 0, monster.X, monster.Y, (ushort)Spells[rand], 0, 0);
                                                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                            SpellPacket.SetStream(stream);
                                                            SpellPacket.Send(monster);
                                                        }
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                case 3739:
                                case 40935:
                                case 40945:
                                case 20055://lava beast
                                    {
                                        List<ushort> Spells = new List<ushort>() { 10000, 10001, 10003 };
                                        ushort rand = (byte)Pool.GetRandom.Next(0, Spells.Count);
                                        switch (Spells[rand])
                                        {
                                            case 10003:
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                          , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                        foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                        {
                                                            if (!targent.Alive)
                                                                continue;
                                                            var player = targent as Role.Player;
                                                            if (Role.Core.GetDistance(monster.X, monster.Y, player.X, player.Y) <= 3)
                                                            {
                                                                uint Damage = PhysicalAttack(player.Owner, monster);
                                                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                    , MsgServer.MsgAttackPacket.AttackEffect.None));
                                                                CheckForOponnentDead(player, Damage, monster);

                                                                if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                                                    player.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);
                                                            }
                                                        }
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    break;
                                                }
                                            case 10001:
                                                {

                                                    uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                         , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    CheckForOponnentDead(monster.Target, Damage, monster);

                                                    if (Rate(5) && !monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Dizzy))
                                                    {
                                                        monster.Target.AddFlag(MsgServer.MsgUpdate.Flags.Frightened, 6, true);
                                                    }
                                                    break;
                                                }
                                            default:
                                                {
                                                    uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                  , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                                        SpellPacket.SetStream(stream);
                                                        SpellPacket.Send(monster);
                                                    }
                                                    CheckForOponnentDead(monster.Target, Damage, monster);
                                                    break;
                                                }

                                        }
                                        break;
                                    }
                                case 66432:
                                case 40934:
                                case 40944:
                                case 3738:
                                case 6643://SwordMaster
                                    {
                                        List<ushort> Spells = new List<ushort>() { 10500, 10502, 10503, 10504, 10506 };
                                        ushort rand = (byte)Pool.GetRandom.Next(0, Spells.Count);
                                        uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                                        if (rand == 0)
                                            Damage = (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100);
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                          , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage, MsgServer.MsgAttackPacket.AttackEffect.None));
                                            SpellPacket.SetStream(stream);
                                            SpellPacket.Send(monster);
                                        }
                                        CheckForOponnentDead(monster.Target, Damage, monster);

                                        if (Rate(5))
                                        {
                                            monster.Target.AddFlag(MsgServer.MsgUpdate.Flags.Frightened, 6, true);
                                        }
                                        else if (Rate(5) && !monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Frightened))
                                            monster.Target.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 3, true);


                                        break;
                                    }

                            }
                            //bosses
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (monster == null)
                    Console.WriteLine("catch monster action is Null");
                else Console.WriteLine($"{monster.Name} have exception in map : {monster.Map}");
                if (monster.Target == null)
                    Console.WriteLine($"{monster.Name}.target have exception in map : {monster.Map}");
                if (client == null)
                    Console.WriteLine("catch client action is Null");
                Console.SaveException(e);
            }
        }
        public unsafe void SearchTarget(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (monster == null)
                return;

            if (monster.Family == null)
                return;

            if (!monster.Alive)
                return;

            if (monster.Target == null || 
                monster.Target != null && 
                monster.Target.Owner != null && 
                monster.Target.Owner.Socket != null &&
                !monster.Target.Owner.Socket.Alive)
            {
                monster.State = MobStatus.Idle;
                return;
            }

            if (monster.Target != null)
            {
                if (!monster.Target.Alive || monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                {
                    monster.State = MobStatus.Idle;
                    return;
                }
            }

            short distance = MonsterView.GetDistance(monster.Target.X, monster.Target.Y, monster.X, monster.Y);
           
            if (distance <= monster.Family.ViewRange && monster.Target != null && monster.Target.Alive)
            {

                monster.State = MobStatus.Attacking;

                if (DateTime.Now > monster.MoveStamp.AddMilliseconds(monster.Family.MoveSpeed))
                {
                    monster.MoveStamp = DateTime.Now;

                    bool Walk = Random.Next() % 100 < 70;
                    if (Walk)
                    {
                        Role.Flags.ConquerAngle dir = GetAngle(monster.X, monster.Y, monster.Target.X, monster.Target.Y);
                        ushort WalkX = monster.X; ushort WalkY = monster.Y;
                        IncXY(dir, ref WalkX, ref WalkY);

                        var Map = monster.GMap;
                        if (Map.ValidLocation(WalkX, WalkY) && !Map.MonsterOnTile(WalkX, WalkY))
                        {
                            Map.SetMonsterOnTile(monster.X, monster.Y, false);
                            Map.SetMonsterOnTile(WalkX, WalkY, true);

                            Map.View.MoveTo<Role.IMapObj>(monster, WalkX, WalkY);

                            monster.X = WalkX; monster.Y = WalkY;


                            WalkQuery action = new WalkQuery()
                            {
                                Direction = (byte)dir,
                                Running = 1,
                                UID = monster.UID
                            };
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                monster.Send(stream.MovementCreate(&action));
                            }
                        }
                        else
                        {
                            dir = (Role.Flags.ConquerAngle)(Random.Next() % 8);
                            WalkX = monster.X; WalkY = monster.Y;
                            IncXY(dir, ref WalkX, ref WalkY);

                            if (Map.ValidLocation(WalkX, WalkY) && !Map.MonsterOnTile(WalkX, WalkY))
                            {
                                Map.SetMonsterOnTile(monster.X, monster.Y, false);
                                Map.SetMonsterOnTile(WalkX, WalkY, true);

                                Map.View.MoveTo<Role.IMapObj>(monster, WalkX, WalkY);

                                monster.X = WalkX; monster.Y = WalkY;


                                WalkQuery action = new WalkQuery()
                                {
                                    Direction = (byte)dir,
                                    Running = 1,
                                    UID = monster.UID
                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    monster.Send(stream.MovementCreate(&action));

                                    monster.UpdateMonsterView(monster.Target.View, stream);
                                }
                            }
                        }
                    }
                }
            }
            else monster.State = MobStatus.Idle;
        }



        public void CheckTarget(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (monster == null)
                return;
            if (!monster.Alive)
                return;
            if (monster.Target == null && !client.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
            {
                short distance = MonsterView.GetDistance(client.X, client.Y, monster.X, monster.Y);
                if (distance <= monster.Family.ViewRange && client.Alive)
                {
                    var targ = monster.View.GetTarget(client.Owner.Map, Role.MapObjectType.Player, monster.UIDTarget);
                    if (targ != null)
                    {
                        monster.Target = targ as Role.Player;
                        if (monster.Target != null && !monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        {
                            monster.State = MobStatus.SearchTarget;
                            monster.HitTargetState(true);
                        }
                        else
                        {
                            foreach (var OtherTarget in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                            {
                                var obj = OtherTarget as Role.Player;
                                if (obj.Alive && !obj.ContainFlag(MsgServer.MsgUpdate.Flags.Fly) && obj.UID != monster.UIDTarget)
                                {
                                    monster.Target = obj;
                                    monster.State = MobStatus.SearchTarget;
                                    monster.HitTargetState(true);
                                }
                            }
                        }
                    }
                    else monster.HitTargetState(true);
                }
            }
            else
            {
                short distance = MonsterView.GetDistance(client.X, client.Y, monster.X, monster.Y);

                var targ = monster.View.GetTarget(client.Owner.Map, Role.MapObjectType.Player, monster.UIDTarget);
                if (targ != null)
                {
                    monster.Target = targ as Role.Player;
                    if (!monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly) || monster.Boss == 1)
                    {
                        monster.State = MobStatus.SearchTarget;
                        monster.HitTargetState(true);
                    }
                    else
                    {
                        foreach (var OtherTarget in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                        {
                            var obj = OtherTarget as Role.Player;
                            if (obj.Alive && !obj.ContainFlag(MsgServer.MsgUpdate.Flags.Fly) && obj.UID != monster.UIDTarget)
                            {
                                monster.Target = obj;
                                monster.State = MobStatus.SearchTarget;
                                monster.HitTargetState(true);
                            }
                        }
                    }
                }
                else monster.HitTargetState(true);

                if (monster.Target != null && (distance > monster.Family.ViewRange || monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly)))
                {
                    monster.State = MobStatus.Idle;
                }
            }
        }
        public bool CheckDodge(uint Dodge)
        {
            bool allow = true;
            uint Noumber = (uint)Random.Next() % 150;
            if (Noumber > 60)
                Noumber = (uint)Random.Next() % 150;
            if (Noumber < Dodge)
                allow = false;
            return allow;
        }
        public void CheckForOponnentDead(Role.Player Player, uint Damage, MonsterRole monster)
        {
            if (Player == null) return;
            if (monster == null) return;
            if (Player.Alive == false)
                return;

            if (Player.ActivePick)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.RemovePick(stream);
                }
            }
            if (Player.Owner.Socket != null)
                if (!Player.Owner.Socket.Alive)
                    return;
            if (Damage >= Player.HitPoints)
            {
                ushort X = Player.X;
                ushort Y = Player.Y;
                // Era 1 (5017): black name morto por guarda ou patrulha derruba
                // equipamento e vai para a cadeia.
                bool byLaw = (monster.Family.Settings & MonsterSettings.Guard) == MonsterSettings.Guard
                    || monster.Family.Name == "Patroller";
                Player.KilledByLawEnforcer = byLaw && Player.PKPoints >= 100;
                try
                {
                    Player.Dead(null, X, Y, monster.UID);
                }
                finally
                {
                    Player.KilledByLawEnforcer = false;
                }
                if (byLaw && Player.PKPoints >= 100)
                    Player.SendToJailByGuard();
            }
            else
            {
                if (Player.Action == Role.Flags.ConquerAction.Sit)
                {
                    if (Player.Stamina >= 20)
                        Player.Stamina -= 20;
                    else
                        Player.Stamina = 0;

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Player.SendUpdate(stream, Player.Stamina, MsgServer.MsgUpdate.DataType.Stamina);
                    }


                    Player.Action = Role.Flags.ConquerAction.None;
                }

                Player.HitPoints -= (int)Damage;
            }
        }
        public unsafe bool CheckRespouseDamage(Role.Player player, MonsterRole Monster)
        {
            if (Rate(15))
            {
                if (player.ContainReflect)
                {
                    Game.MsgServer.MsgSpellAnimation.SpellObj DmgObj = new Game.MsgServer.MsgSpellAnimation.SpellObj();
                    DmgObj.Damage = PhysicalAttack(player.Owner, Monster);
                    DmgObj.Damage /= 10;
                    if (DmgObj.Damage == 0)
                        DmgObj.Damage = 1;

                    InteractQuery action = new InteractQuery()
                    {
                        Damage = (int)DmgObj.Damage,
                        ResponseDamage = DmgObj.Damage,
                        AtkType = MsgAttackPacket.AttackID.Reflect,
                        X = player.X,
                        Y = player.Y,
                        OpponentUID = Monster.UID,
                        UID = player.UID,
                        Effect = DmgObj.Effect
                    };
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        Monster.Send(stream.InteractionCreate(&action));

                        Game.MsgServer.AttackHandler.ReceiveAttack.Monster.Execute(stream, DmgObj, player.Owner, Monster);
                    }
                    return true;


                }

                if (player.ActivateCounterKill)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        if (player.ContainFlag(MsgServer.MsgUpdate.Flags.ShurikenVortex))
                            return false;
                        Database.MagicType.Magic DBSpell;
                        Game.MsgServer.MsgSpell ClientSpell;
                        if (player.Owner.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out ClientSpell))
                        {
                            Dictionary<ushort, Database.MagicType.Magic> DBSpells;
                            if (Pool.Magic.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out DBSpells))
                            {
                                if (DBSpells.TryGetValue(ClientSpell.Level, out DBSpell))
                                {
                                    Game.MsgServer.MsgSpellAnimation.SpellObj DmgObj = new Game.MsgServer.MsgSpellAnimation.SpellObj();
                                    Game.MsgServer.AttackHandler.Calculate.Physical.OnMonster(player, Monster, DBSpell, out DmgObj);

                                    //update spell
                                    if (ClientSpell.Level < DBSpells.Count - 1)
                                    {
                                        int SpecialRate = 1;
                                        if (player.Level < 120)
                                            SpecialRate = 100;
                                        ClientSpell.Experience += (int)(DmgObj.Damage * ServerConfig.ExpRateSpell * SpecialRate);
                                        if (ClientSpell.Experience > DBSpells[ClientSpell.Level].Experience)
                                        {
                                            ClientSpell.Level++;
                                            ClientSpell.Experience = 0;
                                        }
                                        player.Send(stream.SpellCreate(ClientSpell));
                                    }

                                    InteractQuery action = new InteractQuery()
                                    {
                                        ResponseDamage = DmgObj.Damage,
                                        AtkType = MsgAttackPacket.AttackID.Scapegoat,
                                        X = player.X,
                                        Y = player.Y,
                                        OpponentUID = Monster.UID,
                                        UID = player.UID,
                                        Effect = DmgObj.Effect
                                    };

                                    Monster.Send(stream.InteractionCreate(&action));


                                    Game.MsgServer.AttackHandler.ReceiveAttack.Monster.Execute(stream, DmgObj, player.Owner, Monster, true);
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public uint MagicAttack(MonsterRole attacked, MonsterRole monster)
        {
            uint power = (uint)monster.Family.MaxAttack;
            if (power > attacked.Family.Defense)
                power -= attacked.Family.Defense;
            else power = 1;
            return power;
        }
        public uint MagicAttack(Client.GameClient client, MonsterRole monster)
        {
            if (client.Socket == null) return 0;
            if (!client.Socket.Alive)
                return 0;
            if (client.Player.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
                return 1;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.CheckRespouseDamage(client);

            uint power = (uint)monster.Family.MaxAttack;
            power = DecreaseBless(power, client.Status.ItemBless);

            if (power > client.Status.MagicDefence)
                power -= client.Status.MagicDefence;
            else power = 1;
            if (power > client.Status.MagicDamageDecrease)
                power -= client.Status.MagicDamageDecrease;
            else power = 1;
            return power;
        }
        public uint PhysicalAttack(Client.GameClient client, MonsterRole monster)
        {
            if (client.Socket == null)
                return 0;
            if (!client.Socket.Alive)
                return 0;
            if (client.Player.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
                return 1;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.CheckRespouseDamage(client);

            uint power = 0;
            if (monster.Family.MaxAttack > monster.Family.MinAttack)
                power = (uint)SystemRandom.Next(monster.Family.MinAttack, monster.Family.MaxAttack);
            else
                power = (uint)SystemRandom.Next(monster.Family.MaxAttack, monster.Family.MinAttack);

            power += (uint)(power * ServerConfig.PhysicalDamage / 100);

            power = DecreaseBless(power, client.Status.ItemBless);

            if (power > client.AjustDefense)
                power -= client.AjustDefense;
            else
                power = 1;

            //power = (uint)Database.Disdain.MonsterAttackUser(monster, client.Player, (int)power);

            if (power > client.AjustPhysicalDamageDecrease())
                power -= client.AjustPhysicalDamageDecrease();
            else
                power = 1;

            /*  if (monster.Family.ExtraCritical > 0)
              {
                  if (client.AjustImunity() < monster.Family.ExtraCritical)
                  {
                      power += monster.Family.ExtraCritical / 100 * power / 100;

                      //check target imunity;
                      if (client.AjustImunity() > 0)
                      {
                          power -= power * (client.AjustImunity() / 100) / 100;
                      }
                  }
              }
              if (monster.Family.ExtraBreack > 0)
              {
                  if (client.AjustAntiBreack() < monster.Family.ExtraBreack)
                  {
                      power += monster.Family.ExtraBreack / 10 * power / 100;

                      //check target imunity;
                      if (client.AjustImunity() > 0)
                      {
                          power -= power * (client.AjustAntiBreack() / 10) / 100;
                      }
                  }
              }*/
            return power;
        }
        public uint DecreaseBless(uint Damage, uint bless)
        {
            uint power = Damage;
            power = (power * bless) / 100;
            power = Damage - power;
            return power;
        }
        public Role.Flags.ConquerAngle GetAngle(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            double direction = 0;

            double AddX = X2 - X;
            double AddY = Y2 - Y;
            double r = (double)Math.Atan2(AddY, AddX);

            if (r < 0) r += (double)Math.PI * 2;

            direction = 360 - (r * 180 / (double)Math.PI);

            byte Dir = (byte)((7 - (Math.Floor(direction) / 45 % 8)) - 1 % 8);
            return (Role.Flags.ConquerAngle)(byte)((int)Dir % 8);
        }
        public static void IncXY(Role.Flags.ConquerAngle Facing, ref ushort x, ref ushort y)
        {
            sbyte xi, yi;
            xi = yi = 0;
            switch (Facing)
            {
                case Role.Flags.ConquerAngle.North: xi = -1; yi = -1; break;
                case Role.Flags.ConquerAngle.South: xi = 1; yi = 1; break;
                case Role.Flags.ConquerAngle.East: xi = 1; yi = -1; break;
                case Role.Flags.ConquerAngle.West: xi = -1; yi = 1; break;
                case Role.Flags.ConquerAngle.NorthWest: xi = -1; break;
                case Role.Flags.ConquerAngle.SouthWest: yi = 1; break;
                case Role.Flags.ConquerAngle.NorthEast: yi = -1; break;
                case Role.Flags.ConquerAngle.SouthEast: xi = 1; break;
            }
            x = (ushort)(x + xi);
            y = (ushort)(y + yi);
        }
    }
}
