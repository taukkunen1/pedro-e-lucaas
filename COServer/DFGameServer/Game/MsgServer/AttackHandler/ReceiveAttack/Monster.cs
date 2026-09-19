using System;

namespace GameServer.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Monster
    {
        public static uint Execute(ServerSockets.Packet stream, MsgSpellAnimation.SpellObj obj, Client.GameClient client, MsgMonster.MonsterRole monster, bool CounterKill = false)
        {
            if (monster.HitPoints <= obj.Damage)
            {

                client.Map.SetMonsterOnTile(monster.X, monster.Y, false);
                monster.Dead(stream, client, client.Player.UID, client.Map, CounterKill);

                if (client.Team != null)
                    client.Team.ShareExperience(stream, client, monster);
            }
            else
            {
                monster.HitPoints -= obj.Damage;

                if (monster.Boss == 1)
                {
                    Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(stream, monster.UID, 2);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, monster.Family.MaxHealth);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, monster.HitPoints);
                    stream = Upd.GetArray(stream);
                    client.Player.View.SendView(stream, true);
                    monster.SendScores(stream);
                    monster.UpdateScores(client.Player, obj.Damage);
                }
            }

            if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) != MsgMonster.MonsterSettings.Guard)
            {
                if (obj.Damage > monster.Family.MaxHealth)
                {
                    if (monster.Family.ID == 3130 || monster.Family.ID == 4145 || monster.Family.ID == 3737)
                    {
                        if (client.Player.Level < 100)
                        {
                            uint exp1 = (uint)(AdjustExp(monster.Family.MaxHealth, client.Player.Level, monster.Level) * 5) / 100;
                            return exp1 / 2;
                        }
                    }
                    return (uint)AdjustExp(monster.Family.MaxHealth, client.Player.Level, monster.Level);
                }
                else
                {
                    if (monster.Family.ID == 3130 || monster.Family.ID == 4145 || monster.Family.ID == 3737)
                    {
                        if (client.Player.Level < 100)
                        {
                            uint exp2 = (uint)(AdjustExp((int)obj.Damage, client.Player.Level, monster.Level) * 3) / 100;
                            return exp2 / 4;
                        }
                    }
                    return (uint)AdjustExp((int)obj.Damage, client.Player.Level, monster.Level);
                }
            }
            else
                return 0;
        }
        public static int AdjustExp(int nDamage, int nAtkLev, int nDefLev)
        {

            //   return nDamage;
            //if (nAtkLev > 120)
            //    nAtkLev = 120;

            int nExp = nDamage;

            int nNameType = Calculate.Base.GetNameType(nAtkLev, nDefLev);
            int nDeltaLev = nAtkLev - nDefLev;
            if (nNameType == Calculate.Base.StatusConstants.NAME_GREEN)
            {
                if (nDeltaLev >= 3 && nDeltaLev <= 5)
                    nExp = nExp * 70 / 100;
                else if (nDeltaLev > 5 && nDeltaLev <= 10)
                    nExp = nExp * 20 / 100;
                else if (nDeltaLev > 10 && nDeltaLev <= 20)
                    nExp = nExp * 10 / 100;
                else if (nDeltaLev > 20)
                    nExp = nExp * 5 / 100;
            }
            else if (nNameType == Calculate.Base.StatusConstants.NAME_RED)
            {
                nExp = (int)(nExp * 1.3);
            }
            else if (nNameType == Calculate.Base.StatusConstants.NAME_BLACK)
            {
                if (nDeltaLev >= -10 && nDeltaLev < -5)
                    nExp = (int)(nExp * 1.5);
                else if (nDeltaLev >= -20 && nDeltaLev < -10)
                    nExp = (int)(nExp * 1.8);
                else if (nDeltaLev < -20)
                    nExp = (int)(nExp * 2.3);
            }

            return Math.Max(10, nExp);
        }
    }
}
