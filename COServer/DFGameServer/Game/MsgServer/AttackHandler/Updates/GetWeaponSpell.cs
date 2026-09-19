using Core;
using System;
using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler.Updates
{
    public class GetWeaponSpell
    {
        public static void CheckExtraEffects(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (client.Equipment.RingEffect != Role.Flags.ItemEffect.None)
            {
                if (Calculate.Base.Success(20))
                {
                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Stigma))
                    {
                        MsgSpellAnimation MsgSpell = new MsgSpellAnimation(client.Player.UID
                   , 0, client.Player.X, client.Player.Y, (ushort)Role.Flags.SpellID.Stigma
                   , 4, 0);


                        client.Player.AddSpellFlag(MsgUpdate.Flags.Stigma, 20, true);
                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(client.Player.UID, 0, MsgAttackPacket.AttackEffect.None));


                        MsgSpell.SetStream(stream);
                        MsgSpell.Send(client);
                    }
                }
            }
            if (client.Equipment.NecklaceEffect != Role.Flags.ItemEffect.None || client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Shield)
            {
                if (Calculate.Base.Success(20))
                {
                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Shield))
                    {
                        MsgSpellAnimation MsgSpell = new MsgSpellAnimation(client.Player.UID
                   , 0, client.Player.X, client.Player.Y, (ushort)Role.Flags.SpellID.Shield
                   , 4, 0);


                        client.Player.AddSpellFlag(MsgUpdate.Flags.Shield, 15, true);
                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(client.Player.UID, 0, MsgAttackPacket.AttackEffect.None));


                        MsgSpell.SetStream(stream);
                        MsgSpell.Send(client);
                    }
                }
            }
            //if (client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Shield)
            //{
            //    if (Calculate.Base.Success(20))
            //    {
            //        MsgSpellAnimation MsgSpell = new MsgSpellAnimation(client.Player.UID
            //               , 0, client.Player.X, client.Player.Y, 1090
            //               , 4, 0);


            //        if (!client.Player.ContainFlag(MsgUpdate.Flags.Shield))
            //        {
            //            client.Player.AddSpellFlag(MsgUpdate.Flags.Shield, (int)80, true);
            //            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(client.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
            //            MsgSpell.SetStream(stream);
            //            MsgSpell.Send(client);
            //        }
            //    }
            //}
        }
        public unsafe static bool Check(InteractQuery Attack, ServerSockets.Packet stream, Client.GameClient client, Role.IMapObj Target)
        {
           
            if (Calculate.Base.Success(35))
            {
                if (client.Equipment.RightWeapon != 0)
                {
                    if (client.Equipment.UseMonkEpicWeapon)
                    {
                        if (Calculate.Base.Success(20))
                        {
                            List<ushort> CanUse = new List<ushort>();
                            if (client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleAttack))
                                CanUse.Add((ushort)Role.Flags.SpellID.TripleAttack);
                            client.Player.AttackStamp = new DateTime();

                            if (CanUse.Count > 0)
                            {
                                InteractQuery AttackPaket = new InteractQuery();
                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                AttackPaket.UID = Attack.UID;
                                AttackPaket.X = Target.X;
                                AttackPaket.Y = Target.Y;
                                AttackPaket.SpellID = (ushort)CanUse[Pool.GetRandom.Next(0, CanUse.Count)];
                                client.Player.RandomSpell = (ushort)CanUse[Pool.GetRandom.Next(0, CanUse.Count)];

                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                MsgServer.MsgAttackPacket.ProcessMagic(client, stream, AttackPaket);
                                return true;
                            }
                            else
                                return false;
                        }
                    }


                    if ((client.Equipment.RightWeaponEffect != Role.Flags.ItemEffect.None || client.Equipment.LeftWeaponEffect != Role.Flags.ItemEffect.None) && Target.ObjType != Role.MapObjectType.SobNpc)
                    {
                        if (Calculate.Base.Success(20) && !client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                        {
                            client.Player.AttackStamp = new DateTime();

                            InteractQuery AttackPaket = new InteractQuery();

                            AttackPaket.OpponentUID = Attack.OpponentUID;
                            AttackPaket.UID = Attack.UID;
                            AttackPaket.X = Target.X;
                            AttackPaket.Y = Target.Y;

                            if (client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Poison || client.Equipment.LeftWeaponEffect == Role.Flags.ItemEffect.Poison)
                                AttackPaket.SpellID = (ushort)Role.Flags.SpellID.Poison;
                            else if (client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.MP)
                                AttackPaket.SpellID = (ushort)Role.Flags.SpellID.EffectMP;

                            client.Player.RandomSpell = AttackPaket.SpellID;

                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                            MsgServer.MsgAttackPacket.ProcessMagic(client, stream, AttackPaket);

                            return true;
                        }

                    }

                    uint rightWeapon = client.Equipment.RightWeapon;
                    ushort wep1subyte = (ushort)(rightWeapon / 1000), wep2subyte = 0;
                    bool doWep1Spell = false, doWep2Spell = false;
                    ushort wep1spellid = 0, wep2spellid = 0;
                    if (wep1subyte == 421)
                        wep1subyte--;
                    if (Pool.WeaponSpells.ContainsKey(wep1subyte))
                        wep1spellid = Pool.WeaponSpells[wep1subyte];
                    if (client.MySpells.ClientSpells.ContainsKey(wep1spellid))
                        doWep1Spell = Calculate.Base.Success(50);


                    if (!doWep1Spell)
                    {
                        if (client.Equipment.LeftWeapon != 0)
                        {
                            uint leftWeapon = client.Equipment.LeftWeapon;
                            wep2subyte = (ushort)(leftWeapon / 1000);
                            if (Pool.WeaponSpells.ContainsKey(wep2subyte))
                                wep2spellid = Pool.WeaponSpells[wep2subyte];
                            if (client.MySpells.ClientSpells.ContainsKey(wep2spellid))
                                doWep2Spell = true;

                            if (doWep2Spell)
                            {
                                if (!client.MySpells.ClientSpells.ContainsKey(wep2spellid))
                                    return false;

                                if (client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                {
                                    if (Target.ObjType == Role.MapObjectType.Monster)
                                        client.Shift(Target.X, Target.Y, stream);
                                }
                                if (Target.ObjType != Role.MapObjectType.Monster)
                                    MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);
                                else
                                {
                                    MsgMonster.MonsterRole attacked = Target as MsgMonster.MonsterRole;
                                    if (attacked.Family.ID != 4145)
                                        MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);

                                }
                                InteractQuery AttackPaket = new InteractQuery();
                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                AttackPaket.UID = Attack.UID;
                                AttackPaket.X = Target.X;
                                AttackPaket.Y = Target.Y;
                                AttackPaket.SpellID = wep2spellid;
                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                client.Player.RandomSpell = wep2spellid;
                                MsgServer.MsgAttackPacket.ProcessMagic(client, stream, AttackPaket, true);

                                return true;
                            }
                            else
                            {
                                doWep1Spell = true;
                            }
                        }
                        else
                        {
                            doWep1Spell = true;
                        }

                    }
                    if (doWep1Spell)
                    {
                        if (!client.MySpells.ClientSpells.ContainsKey(wep1spellid))
                            return false;

                        if (client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                        {
                            client.Shift(Target.X, Target.Y, stream);
                        }

                        if (Target.ObjType != Role.MapObjectType.Monster)
                            MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);
                        else
                        {
                            MsgMonster.MonsterRole attacked = Target as MsgMonster.MonsterRole;
                            if (attacked.Family.ID != 4145)
                                MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);

                        }
                        InteractQuery AttackPaket = new InteractQuery();
                        AttackPaket.OpponentUID = Attack.OpponentUID;
                        AttackPaket.UID = Attack.UID;
                        AttackPaket.X = Target.X;
                        AttackPaket.Y = Target.Y;
                        AttackPaket.SpellID = wep1spellid;
                        client.Player.RandomSpell = wep1spellid;
                        AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                        MsgServer.MsgAttackPacket.ProcessMagic(client, stream, AttackPaket, true);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
