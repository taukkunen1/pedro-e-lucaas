using System;

namespace GameServer.Game.MsgServer.AttackHandler.Calculate
{
    public class Range
    {
        public static void OnMonster(Role.Player player, MsgMonster.MonsterRole monster, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, byte MultipleDamage = 0)
        {

            SpellObj = new MsgSpellAnimation.SpellObj(monster.UID, 0, MsgAttackPacket.AttackEffect.None);
            if (monster.IsFloor)
            {
                SpellObj.Damage = 1;
                return;
            }
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);

            Damage = (int)player.Owner.AjustAttack((uint)Damage);
            Damage = (int)player.Owner.AjustMaxAttack((uint)Damage);
            if (player.Level > monster.Level)
                Damage *= 2;
            if (MultipleDamage != 0)
            {
                Damage = Damage * MultipleDamage;
            }
            if (DBSpell != null)
            {
                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : ServerConfig.PhysicalDamage), 100);
            }
            else
            {
                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : ServerConfig.PhysicalDamage), 100);
                //  Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense2, player.Owner.GetDefense2());
            }

            var rawDefense = monster.Family.Defense;

            Damage = Math.Max(0, Damage - rawDefense);

            Damage = (int)Base.BigMulDiv(Damage, monster.Family.Defense2, Client.GameClient.DefaultDefense2);
            Damage = Base.MulDiv((int)Damage, (int)(100 - (int)(monster.Family.Dodge * 0.4)), 100);

            // if (monster.Boss == 0)
            {
                Damage = Base.CalcDamageUser2Monster(Damage, monster.Family.Defense, player.Level, monster.Level, true);
                Damage = Base.AdjustMinDamageUser2Monster(Damage, player.Owner);

            }

            Damage = (int)Calculate.Base.CalculateExtraAttack((uint)Damage, player.Owner.Status.PhysicalDamageIncrease, 0);
            if (monster.Family.Defense2 == 0)
                Damage = 1;


            SpellObj.Damage = (uint)Math.Max(1, Damage);
            //  MyConsole.WriteLine("My Range Damage 1 -> Monster " + SpellObj.Damage.ToString());
#if TEST
            Console.WriteLine("My Range Damage -> Monster " + SpellObj.Damage.ToString());
#endif

            if (monster.Boss == 0)
            {
                if (player.ContainFlag(MsgUpdate.Flags.Superman))
                    SpellObj.Damage *= 10;
            }

            if (Base.GetRefinery())
            {
                if (player.Owner.Status.CriticalStrike > 0)
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;

                    SpellObj.Damage += (SpellObj.Damage * (player.Owner.AjustCriticalStrike() / 100)) / 100;
                }
            }

            //   MyConsole.WriteLine("My Range Damage 2 -> Monster " + SpellObj.Damage.ToString());
            //   MyConsole.WriteLine("My Range Damage 2 -> Monster " + SpellObj.Effect.ToString());
            //   SpellObj.Damage += player.Owner.Status.PhysicalDamageIncrease;

            if (player.SuperManMode)
            {
                SpellObj.Damage *= 100;
            }
        }
        public static void OnPlayer(Role.Player player, Role.Player target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, int increasedmg = 0)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0, MsgAttackPacket.AttackEffect.None);
            if (target.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
            {
                SpellObj.Damage = 1;
                return;
            }
            if (DBSpell == null)
            {
                if (Base.Dodged(player.Owner, target.Owner))
                {
                    SpellObj.Damage = 0;
                    return;
                }
            }
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);


            Damage = (int)player.Owner.AjustAttack((uint)Damage);
            bool update = false;
            if (!update)
            {
                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : ServerConfig.PhysicalDamage), 100);
            }
            if (target.Owner.GemValues(Role.Flags.Gem.SuperTortoiseGem) > 0)
            {
                int reduction = Base.MulDiv((int)target.Owner.GemValues(Role.Flags.Gem.SuperTortoiseGem), 50, 100);

                Damage = Base.MulDiv((int)Damage, (int)(100 - Math.Min(67, reduction)), 100);
            }
            Damage = Damage * (int)(110 - target.Owner.Status.Dodge) / 100;
            Damage = Base.MulDiv((int)Damage, 65, 100);
            Damage = (int)Base.BigMulDiv((int)Damage, player.Owner.GetDefense2(), Client.GameClient.DefaultDefense2);
            //bool onbreak = false;

            if (player.Owner.Status.CriticalStrike > 0)
            {
                if (Base.GetRefinery(player.Owner.Status.CriticalStrike / 100, target.Owner.Status.Immunity / 100))
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                    Damage = Base.MulDiv((int)Damage, 120, 100);
                }
            }

            if (player.Owner.Status.Breakthrough > 0)
            {
                if (player.BattlePower > target.BattlePower)
                {
                    if (player.Owner.Status.Breakthrough > target.Owner.Status.Counteraction)
                    {
                        double Power = (double)(player.Owner.Status.Breakthrough - target.Owner.Status.Counteraction);
                        Power = (double)(Power / 10);
                        if (Base.Success(Power))
                        {
                            //onbreak = true;
                            SpellObj.Effect |= MsgAttackPacket.AttackEffect.Break;
                        }
                    }
                }
            }

            //if (!onbreak && player.Owner.InSkillTeamPk() == false)
            //    Damage = Base.CalculatePotencyDamage(Damage, player.BattlePower, target.BattlePower, true);

            Damage = (int)Calculate.Base.CalculateExtraAttack((uint)Damage, player.Owner.Status.PhysicalDamageIncrease, target.Owner.Status.PhysicalDamageDecrease);

            SpellObj.Damage = (uint)Math.Max(1, Damage);
            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
            {
                if (SpellObj.Damage > target.AzureShieldDefence)
                {
                    Calculate.AzureShield.CreateDmg(player, target, target.AzureShieldDefence);
                    target.RemoveFlag(MsgUpdate.Flags.AzureShield);
                    SpellObj.Damage -= target.AzureShieldDefence;

                }
                else
                {
                    target.AzureShieldDefence -= (ushort)SpellObj.Damage;
                    Calculate.AzureShield.CreateDmg(player, target, SpellObj.Damage);
                    SpellObj.Damage = 1;
                }
            }

            if (target.ContainFlag(MsgUpdate.Flags.DefensiveStance))
            {
                SpellObj.Damage = Calculate.Base.CalculateBless(SpellObj.Damage, 40);
                SpellObj.Effect = MsgAttackPacket.AttackEffect.Block;
                return;
            }

            MsgSpellAnimation.SpellObj InRedirect;
            if (BackDmg.Calculate(player, target, DBSpell, SpellObj.Damage, out InRedirect))
                SpellObj = InRedirect;

            if (target.Owner.Equipment.ShieldID != 0)
            {
                double Block = (target.Owner.Status.Block / 100);
                if (DateTime.Now < target.ShieldBlockEnd)
                    Block += ((target.ShieldBlockDamage) / 100);
                double Change = Math.Min(70.0, Block);

                if (Base.Success(Change))
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.Block;
                    SpellObj.Damage /= 2;
                }
            }

            if (player.SuperManMode)
            {
                SpellObj.Damage *= 100;
            }
        }
        public static void OnNpcs(Role.Player player, Role.SobNpc target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0, MsgAttackPacket.AttackEffect.None);

            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);

            Damage = (int)player.Owner.AjustAttack((uint)Damage);

            Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : ServerConfig.PhysicalDamage), 100);
            Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense2, player.Owner.GetDefense2());

            SpellObj.Damage = (uint)Math.Max(1, Damage);

            if (Base.GetRefinery())
            {
                if (player.Owner.Status.CriticalStrike > 0)
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                    SpellObj.Damage = Base.CalculateArtefactsDmg(SpellObj.Damage, player.Owner.Status.CriticalStrike, 0);
                }
            }
            //if (target.UID == 7811 || target.UID == 7812 || target.UID == 7813 || target.UID == 7814 || target.UID == 7815 || target.UID == 7816 || target.UID == 7817 || target.UID == 7818 || target.UID == 7819 || target.UID == 7820 || target.UID == 7821)
            //{
            //    player.Owner.OnAutoAttack = true;

            //}
            SpellObj.Damage = Calculate.Base.CalculateExtraAttack((uint)SpellObj.Damage, player.Owner.Status.PhysicalDamageIncrease, 0);
            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
                SpellObj.Damage = 100;


            if (player.SuperManMode)
            {
                SpellObj.Damage *= 100;
            }
        }

    }
}
