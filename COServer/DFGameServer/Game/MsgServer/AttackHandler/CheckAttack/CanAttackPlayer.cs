using GameServer.Client;
using GameServer.Game.MsgTournaments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CanAttackPlayer
    {
        public static bool Verified(Client.GameClient client, Role.Player attacked
      , Database.MagicType.Magic DBSpell, bool Archer = false)
        {
            if (client.Map.ID == 10137)
            {
                if (client.Player.X < 145 || attacked.X < 145)
                {
                    client.SendSysMesage("You can't attack in the safe area", MsgMessage.ChatMode.Whisper, MsgMessage.MsgColor.red);
                    return false;
                }
            }
            if (client.Map.ID == 10166)
            {
                if (Calculate.Base.GetDistance(client.Player.X, client.Player.Y, 60, 128) < 10)
                {
                    client.SendSysMesage("You can't attack in the safe area", MsgMessage.ChatMode.Whisper, MsgMessage.MsgColor.red);
                    return false;
                }
                if (DateTime.Now.Minute <= 15)
                {
                    client.SendSysMesage("You can't attack in the first 15 minute of each hour", MsgMessage.ChatMode.Whisper, MsgMessage.MsgColor.red);
                    return false;
                }
            }
            if (client.Player.OnTransform)
                return false;
            if (attacked.Map == 700 && attacked.DynamicID == 0)
                return false;
            if (!attacked.Alive)
                return false;
            if (attacked.Invisible)
                return false;
            if (client.Player.PkMode == Role.Flags.PKMode.Peace)
                return false;


            if (client.Player.Map == 3935)
            {
                foreach (var server in Database.GroupServerList.GroupServers.Values)
                {
                    if (Role.Core.GetDistance((ushort)server.X, (ushort)server.Y, attacked.X, attacked.Y) <= 8)
                        return false;
                }
            }

            if (client.Player.Map == 1002)
            {
                if (client.Player.OnMyOwnServer)
                {
                    if (attacked.OnMyOwnServer == false)
                        return true;
                }
                else
                {
                    return false;
                }
            }
            if (attacked.ContainFlag(MsgUpdate.Flags.Fly) && !Archer)
            {
                if (DBSpell == null)
                    return false;
                else if (!DBSpell.AttackInFly)
                    return false;
            }
            if (attacked.Owner.IsWatching())
            {
                return false;
            }
            if (client.IsWatching())
            {
                return false;
            }
            if (!attacked.AllowAttack())
                return false;
            if (client.InTeamQualifier())
            {
                if (client.Team != null && client.Player.Map == 700)
                {
                    if (!client.Team.Members.ContainsKey(attacked.UID))
                        return true;
                    else
                        return false;
                }
            }
            if (client.Player.Map == MsgTournaments.MsgCaptureTheFlag.MapID)
            {
                if (!MsgTournaments.MsgSchedules.CaptureTheFlag.Attackable(attacked))
                    return false;
            }
            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FrozenSky)
            {
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                {
                    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process != MsgTournaments.ProcesType.Alive)
                        return false;
                }
            }
            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FiveNOut)
            {
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                {
                    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process != MsgTournaments.ProcesType.Alive)
                        return false;
                }
            }
            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.DragonWar)
            {
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                {
                    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process != MsgTournaments.ProcesType.Alive)
                        return false;

                    if (!client.Player.DragonKing && !attacked.DragonKing && DragonWar.HasDragonKing)
                        return false;
                }
            }
            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.KingOfTheHill)
            {
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                {
                    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process != MsgTournaments.ProcesType.Alive)
                        return false;
                }
            }
            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.PassTheBomb)
            {
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                {
                    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process != MsgTournaments.ProcesType.Alive)
                        return false;
                }
            }
            if (client.Player.Map == 4000 || client.Player.Map == 4003 || client.Player.Map == 4006 || client.Player.Map == 4008 || client.Player.Map == 4009
                || client.Player.Map == 4020)
                return false;
            if (Pool.BlockAttackMap.Contains(client.Player.Map) && client.Player.DynamicID == 0)
                return false;
            else if (client.Player.DynamicID != 0)
            {
                if (Pool.BlockAttackMap.Contains(client.Player.DynamicID))
                    return false;
            }
            if (client.Player.PkMode == Role.Flags.PKMode.PK)
            {
                if (client.Player.Map == 1080)
                {
                    if (client.Player.RedTeam && attacked.RedTeam)
                        return false;
                    else if (client.Player.BlueTeam && attacked.BlueTeam)
                        return false;
                }
                
            }
            if (client.Player.PkMode == Role.Flags.PKMode.Team)
            {
                if (client.Player.Map == 1080)
                {
                    if (client.Player.RedTeam && attacked.RedTeam)
                        return false;
                    else if (client.Player.BlueTeam && attacked.BlueTeam)
                        return false;
                }
               
                if (client.Team != null)
                {
                    if (client.Team.Members.ContainsKey(attacked.UID))
                        return false;
                }

                if (client.Player.Associate.Contain(Role.Instance.AssociateGS.Friends, attacked.UID))
                    return false;

                if (client.Player.MyGuild != null)
                {
                    if (client.Player.GuildID == attacked.GuildID)
                        return false;

                    if (attacked.MyGuild != null)
                    {
                        if (client.Player.MyGuild.Ally.ContainsKey(attacked.GuildID))
                            return false;
                    }
                }
                if (client.Player.MyClan != null)
                {
                    if (client.Player.ClanUID == attacked.ClanUID)
                        return false;

                    if (attacked.MyClan != null)
                    {
                        if (client.Player.MyClan.Ally.ContainsKey(attacked.ClanUID))
                            return false;
                    }
                }
            }
            if (ServerConfig.IsInterServer == false)
            {
                if (client.Player.PkMode != Role.Flags.PKMode.Capture && client.Player.PkMode != Role.Flags.PKMode.Peace)
                {
                    if (!attacked.ContainFlag(MsgUpdate.Flags.FlashingName) && !attacked.ContainFlag(MsgUpdate.Flags.RedName) && !attacked.ContainFlag(MsgUpdate.Flags.BlackName))
                    {
                        if (!Pool.FreePkMap.Contains(attacked.Map) && attacked.DynamicID == 0)
                            if (!(client.Player.Map == 1020 && MsgTournaments.MsgSchedules.PoleDomination.Proces == MsgTournaments.ProcesType.Alive))
                                if (!(client.Player.Map == 1015 && MsgTournaments.MsgSchedules.PoleDominationBI.Proces == MsgTournaments.ProcesType.Alive))
                                    if (!(client.Player.Map == 1000 && MsgTournaments.MsgSchedules.PoleDominationDC.Proces == MsgTournaments.ProcesType.Alive))
                                        if (!(client.Player.Map == 1011 && MsgTournaments.MsgSchedules.PoleDominationPC.Proces == MsgTournaments.ProcesType.Alive))
                                            if (!(client.Player.Map == 1011 && client.Player.DynamicID != 0))
                                                client.Player.AddFlag(MsgUpdate.Flags.FlashingName, 30, true);

                    }
                    return true;
                }
            }
            if (client.Player.PkMode == Role.Flags.PKMode.CS)
            {
                if (client.Player.ServerID == attacked.ServerID)
                    return false;
            }

            if (client.Player.PkMode == Role.Flags.PKMode.Capture)
            {
                if (attacked.ContainFlag(MsgUpdate.Flags.FlashingName) || attacked.ContainFlag(MsgUpdate.Flags.BlackName))
                    return true;
                else
                    return false;
            }

            return true;
        }
    }
}
