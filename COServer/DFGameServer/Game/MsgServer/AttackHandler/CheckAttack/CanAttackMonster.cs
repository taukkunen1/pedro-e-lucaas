namespace GameServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CanAttackMonster
    {
        public static bool Verified(Client.GameClient client, MsgMonster.MonsterRole attacked
            , Database.MagicType.Magic DBSpell)
        {
            if ((attacked.Family.Settings & MsgMonster.MonsterSettings.Reviver) == MsgMonster.MonsterSettings.Reviver)
                return false;
            //if (client.Player.OnTransform)
            //    return false;
            if (!attacked.Alive)
                return false;

            if ((attacked.Family.Settings & MsgMonster.MonsterSettings.Guard) == MsgMonster.MonsterSettings.Guard)
            {
                if (client.Player.PkMode != Role.Flags.PKMode.PK)
                    return false;
                else
                {
                    client.Player.AddFlag(MsgUpdate.Flags.FlashingName, 30, true);
                }
            }
            if (DBSpell != null && attacked.Family.ID == 4145 && !Pool.RebornInfo.StaticSpells.Contains(DBSpell.ID) && DBSpell.ID != 1045 && DBSpell.ID != 1046 && DBSpell.ID != 11000&& DBSpell.ID != 11005)
            {
                client.SendSysMesage("You can`t use any magic spells on the TwinCity Boss bitch!");
                return false;
            }
            return true;

        }
    }
}
