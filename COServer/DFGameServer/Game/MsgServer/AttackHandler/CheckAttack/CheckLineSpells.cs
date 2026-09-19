using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CheckLineSpells
    {
        public static bool CheckUp(Client.GameClient user, ushort spellid)
        {
            if ((
               //|| MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.TeamDeathmatch
                MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.DragonWar
               || MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.PassTheBomb)
            && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
            {
                if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                {
                    if (spellid != 1045 && spellid != 1046 && spellid != 11005 && spellid != 11000)
                    {
                        user.SendSysMesage("You have to use manual linear skills(fastblade/scentsword/viperfang/dragontail/DragonTail/ViperFang)");
                        return false;
                    }
                }
            }
            //if (user.EventBase != null)
            //{
            //    if (!user.EventBase.MagicAllowed && user.EventBase?.Stage == MsgEvents.EventStage.Fighting)
            //    {
            //        if (user.EventBase?.AllowedSkills != null)
            //        {

            //            if (user.EventBase?.AllowedSkills.Count > 0)
            //            {
            //                if (!user.EventBase.AllowedSkills.Contains(spellid))
            //                {
            //                    user.SendSysMesage("This skill cannot be used in this event!");
            //                    return false;
            //                }
            //            }
            //            else
            //                return false;
            //        }
            //        else
            //        {
            //            user.SendSysMesage("This skill cannot be used in this event!");
            //            return false;
            //        }
            //    }
            //}
            //if (user.Arena != null)
            //{
            //    if (user.Arena?.AllowedSkills != null)
            //    {

            //        if (user.Arena?.AllowedSkills.Count > 0)
            //        {
            //            if (!user.Arena.AllowedSkills.Contains(spellid))
            //            {
            //                user.SendSysMesage("This skill cannot be used in this event!");
            //                return false;
            //            }
            //        }
            //        else
            //            return false;
            //    }
            //    else
            //    {
            //        user.SendSysMesage("This skill cannot be used in this event!");
            //        return false;
            //    }
            //}
            return true;
        }
    }
}
