namespace GameServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CanAttackNpc
    {
        public static bool Verified(Client.GameClient client, Role.SobNpc attacked
     , Database.MagicType.Magic DBSpell)
        {
            if (attacked.HitPoints == 0)
            {
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.LeftGate].UID)
                    return true;
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.RightGate].UID)
                    return true;
                return false;
            }
            #region Check LeftGate or RightGate
            if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.LeftGate].UID || attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.RightGate].UID)
            {
                if (DBSpell != null && DBSpell.ID == 11070)
                {
                    return false;
                }
            }
            #endregion
            if (client.Player.OnTransform)
                return false;
            if (attacked.IsStatue)
            {
                if (attacked.HitPoints == 0)
                    return false;
                if (client.Player.PkMode == Role.Flags.PKMode.PK)
                    return true;
                else
                    return false;
            }
            if (attacked.UID == 890)
            {
                if (client.Player.MyClan == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.ClassicClanWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.ClanUID == Game.MsgTournaments.MsgSchedules.ClassicClanWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.ClassicClanWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.ClassicClanWar.Proces == MsgTournaments.ProcesType.Idle)
                    return false;
            }
            if (attacked.UID == MsgTournaments.MsgFortressWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (MsgTournaments.MsgFortressWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.Name == MsgTournaments.MsgFortressWar.Furnitures[Role.SobNpc.StaticMesh.Pole].Name)
                    return false;
                if (MsgTournaments.MsgFortressWar.Proces != MsgTournaments.ProcesType.Alive)
                    return false;
            }
            if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Idle)
                    return false;
            }
           
            if (attacked.UID >= 821 && attacked.UID <= 825 && Game.MsgTournaments.MsgSchedules.CityWar.Furnitures.ContainsKey(attacked.UID))
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.CityWar.Furnitures[attacked.UID].HitPoints == 0)
                    return false;
                if (attacked.UID == 821 && client.Player.GuildID == Game.MsgTournaments.MsgSchedules.CityWar.WinnerTC.GuildID)
                    return false;
                if (attacked.UID == 822 && client.Player.GuildID == Game.MsgTournaments.MsgSchedules.CityWar.WinnerPC.GuildID)
                    return false;
                if (attacked.UID == 823 && client.Player.GuildID == Game.MsgTournaments.MsgSchedules.CityWar.WinnerAC.GuildID)
                    return false;
                if (attacked.UID == 824 && client.Player.GuildID == Game.MsgTournaments.MsgSchedules.CityWar.WinnerDC.GuildID)
                    return false;
                if (attacked.UID == 825 && client.Player.GuildID == Game.MsgTournaments.MsgSchedules.CityWar.WinnerBI.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.CityWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.CityWar.Proces == MsgTournaments.ProcesType.Idle)
                    return false;
            }
            #region Poles
            if (Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures.ContainsKey(Role.SobNpc.StaticMesh.Pole) && Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures[Role.SobNpc.StaticMesh.Pole] != null)
            {
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    if (client.Player.MyGuild == null)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDomination.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                        return false;
                    if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.PoleDomination.Winner.GuildID)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDomination.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.PoleDomination.Proces == MsgTournaments.ProcesType.Idle)
                        return false;
                }
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDominationBI.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    if (client.Player.MyGuild == null)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDominationBI.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                        return false;
                    if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.PoleDominationBI.Winner.GuildID)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDominationBI.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.PoleDominationBI.Proces == MsgTournaments.ProcesType.Idle)
                        return false;
                }
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDominationDC.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    if (client.Player.MyGuild == null)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDominationDC.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                        return false;
                    if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.PoleDominationDC.Winner.GuildID)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDominationDC.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.PoleDominationDC.Proces == MsgTournaments.ProcesType.Idle)
                        return false;
                    //if (client.Player.MyGuild == null)
                    //    return false;
                    //if (Game.MsgTournaments.MsgSchedules.PoleDominationDC.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    //    return false;
                    //if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.PoleDominationDC.Winner.GuildID)
                    //    return false;
                    //if (Game.MsgTournaments.MsgSchedules.PoleDominationDC.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.PoleDominationDC.Proces == MsgTournaments.ProcesType.Idle)
                    //    return false;
                }
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.PoleDominationPC.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                {
                    if (client.Player.MyGuild == null)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDominationPC.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                        return false;
                    if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.PoleDominationPC.Winner.GuildID)
                        return false;
                    if (Game.MsgTournaments.MsgSchedules.PoleDominationPC.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.PoleDominationPC.Proces == MsgTournaments.ProcesType.Idle)
                        return false;
                }
            }
            #endregion
            if (attacked.UID == Game.MsgTournaments.MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.EliteGuildWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces == MsgTournaments.ProcesType.Idle)
                    return false;
            }
           
            
            MsgTournaments.MsgCaptureTheFlag.Basse Bas;
            if (MsgTournaments.MsgSchedules.CaptureTheFlag.Bases.TryGetValue(attacked.UID, out Bas))
            {
                if (MsgTournaments.MsgSchedules.CaptureTheFlag.Proces != MsgTournaments.ProcesType.Alive)
                    return false;
                if (client.Player.MyGuild == null)
                    return false;
                if (Bas.Npc.HitPoints == 0)
                    return false;
                if (Bas.CapturerID == client.Player.GuildID)
                    return false;

            }
            return true;
        }
    }
}
