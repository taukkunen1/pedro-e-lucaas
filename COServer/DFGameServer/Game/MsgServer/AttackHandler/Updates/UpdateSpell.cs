using System;
using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler.Updates
{
    public class UpdateSpell
    {
        public unsafe static void CheckUpdate(ServerSockets.Packet stream, Client.GameClient client, InteractQuery Attack, uint Damage, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            if (Damage == 0 && Attack.SpellID != 30000)
                return;
            if (DBSpells != null)
            {
                MsgSpell ClientSpell;
                if (client.MySpells.ClientSpells.TryGetValue(Attack.SpellID, out ClientSpell))
                {
                    ushort firstlevel = ClientSpell.Level;
                    if (ClientSpell.Level < DBSpells.Count - 1)
                    {
                        if (client.Player.Level >= DBSpells[ClientSpell.Level].NeedLevel)
                        {
                            if (Attack.SpellID == 30000)
                                ClientSpell.Experience += (int)(10 * ServerConfig.ExpRateSpell);
                            else
                                ClientSpell.Experience += (int)(Damage * ServerConfig.ExpRateSpell);
                            if (ClientSpell.Experience > DBSpells[ClientSpell.Level].Experience)
                            {
                                ClientSpell.PreviousLevel = (byte)ClientSpell.Level;
                                ClientSpell.Level++;
                                ClientSpell.Experience = 0;
                            }
                            if (ClientSpell.PreviousLevel != 0 && ClientSpell.PreviousLevel >= ClientSpell.Level)
                            {
                                ClientSpell.Level = ClientSpell.PreviousLevel;
                            }
                            try
                            {
                                if (ClientSpell.Level > firstlevel)
                                    client.SendSysMesage("You have just leveled your skill " + DBSpells[0].Name + ".", MsgMessage.ChatMode.System);
                            }
                            catch (Exception e) { Console.WriteLine(e.ToString()); }
                            client.Send(stream.SpellCreate(ClientSpell));
                        }
                    }
                }
            }
            if (Attack.AtkType == MsgAttackPacket.AttackID.Physical || Attack.AtkType == MsgAttackPacket.AttackID.Archer || Attack.AtkType == MsgAttackPacket.AttackID.Magic)
            {
                uint ProfRightWeapon = client.Equipment.RightWeapon / 1000;
                uint PorfLeftWeapon = client.Equipment.LeftWeapon / 1000;
                if (ProfRightWeapon != 0)
                    client.MyProfs.CheckUpdate(ProfRightWeapon, ServerConfig.ExpRateProf, stream);
                if (PorfLeftWeapon != 0)
                    client.MyProfs.CheckUpdate(PorfLeftWeapon, ServerConfig.ExpRateProf, stream);
            }
        }
    }
}
