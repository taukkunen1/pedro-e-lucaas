using System;
using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CanUseSpell
    {
        public unsafe static bool Verified(InteractQuery Attack, Client.GameClient client, Dictionary<ushort, Database.MagicType.Magic> DBSpells
            , out MsgSpell ClientSpell, out Database.MagicType.Magic Spell)
        {
            try
            {
                //anti proxy --------------------------

                if (Database.MagicType.RandomSpells.Contains((Role.Flags.SpellID)Attack.SpellID))
                {
                    if (client.Player.RandomSpell != Attack.SpellID)
                    {
                        ClientSpell = default(MsgSpell);
                        Spell = default(Database.MagicType.Magic);
                        return false;
                    }
                    client.Player.RandomSpell = 0;
                }
                //-------------------------------------
                if (client.MySpells.ClientSpells.TryGetValue(Attack.SpellID, out ClientSpell))
                {
                    if (DBSpells.TryGetValue(ClientSpell.Level, out Spell))
                    {

                        if (Spell.Type == Database.MagicType.MagicSort.DirectAttack || Spell.Type == Database.MagicType.MagicSort.Attack)
                        {

                            if (!client.IsInSpellRange(Attack.OpponentUID, Spell.Range))
                            {
                                ClientSpell = default(MsgSpell);
                                Spell = default(Database.MagicType.Magic);
                                return false;
                            }
                        }

                        uint IncreaseSpellStamina = 0;//constant
                        if (client.Player.ContainFlag(MsgUpdate.Flags.ScurvyBomb))
                            IncreaseSpellStamina = (uint)(client.Player.UseStamina + 5);
                        //var sobnpc = client.Player.View.Roles(Role.MapObjectType.SobNpc).Where(p => (p.UID > 7810 && p.UID < 7822));
                        if (client.Player.Map != 1039)
                        {
                            if (Spell.UseStamina + IncreaseSpellStamina > client.Player.Stamina)
                                return false;//try
                            else
                            {
                                if ((ushort)(Spell.UseStamina + IncreaseSpellStamina) > 0)
                                {
                                    client.Player.Stamina -= (ushort)(Spell.UseStamina + IncreaseSpellStamina);
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Stamina, MsgUpdate.DataType.Stamina);
                                    }
                                }
                            }
                            if (Spell.UseMana > client.Player.Mana)
                                return false;
                            else
                            {
                                if (Spell.UseMana > 0)
                                {
                                    client.Player.Mana -= Spell.UseMana;
                                }
                            }
                        }
                        if (Spell.IsSpellWithColdTime)
                        {
                            DateTime now = DateTime.Now;
                            if (ClientSpell.ColdTime > now)
                                return false;
                            else
                            {
                                ClientSpell.IsSpellWithColdTime = true;
                                ClientSpell.ColdTime = now.AddMilliseconds(Spell.ColdTime);
                            }

                        }
                        else// if(ClientSpell.ID == 6000 || ClientSpell.ID == 10381)
                        {
                            if (ClientSpell.ID == 10381)
                            {
                                if (client.Player.WhirlWind || DateTime.Now > ClientSpell.LastUse.AddMilliseconds(600))
                                {
                                    ClientSpell.LastUse = DateTime.Now;
                                    client.Player.WhirlWind = false;
                                    return true;
                                }
                            }
                            else if (DateTime.Now > ClientSpell.LastUse.AddMilliseconds(Spell.CustomCoolDown) || Pool.RebornInfo.StaticSpells.Contains(Spell.ID))
                            {
                                ClientSpell.LastUse = DateTime.Now;
                                return true;
                            }
                            return false;
                        }
                        return true;
                    }
                }

                ClientSpell = default(MsgSpell);
                Spell = default(Database.MagicType.Magic);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ClientSpell = default(MsgSpell);
                Spell = default(Database.MagicType.Magic);
                return false;
            }
            return false;
        }
    }
}
