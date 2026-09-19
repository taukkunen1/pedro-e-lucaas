using GameServer.Game.MsgTournaments;

namespace GameServer.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Player
    {
        public unsafe static void Execute(MsgSpellAnimation.SpellObj obj, Client.GameClient client, Role.Player attacked)
        {
            CheckAttack.CheckItems.RespouseDurability(client);
            if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.DragonWar)
            {
                if (MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                {
                    if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                    {
                        if (attacked.DragonWarHits <= 1)
                        {
                            if (attacked.DragonKing || !DragonWar.HasDragonKing)
                            {
                                attacked.DragonKing = false;
                                client.Player.DragonKing = true;
                                client.Player.DragonWarScore += 15;
                            }
                            else if (client.Player.DragonKing)
                                client.Player.DragonWarScore += 5;
                            attacked.DragonWarHits = 0;
                            attacked.Dead(client.Player, attacked.X, attacked.Y, attacked.UID);
                        }
                        else attacked.DragonWarHits--;
                        return;
                    }
                }
            }
            if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.PassTheBomb)
            {
                if (MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                {
                    if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                    {
                        if (client.Player.HasTheBomb == true)
                        {
                            attacked.HasTheBomb = true;
                            client.Player.HasTheBomb = false;
                            MsgTournaments.PassTheBomb.Hunted = attacked.Name;
                            ((MsgTournaments.PassTheBomb)MsgTournaments.MsgSchedules.CurrentTournament).SetTime();
                        }
                        return;
                    }
                }
            }
            ////////////////
           
            ////////////////
            ushort X = attacked.X;
            ushort Y = attacked.Y;
            using (var rec = new ServerSockets.RecycledPacket())
            {

                var stream = rec.GetStream();
                ActionQuery Gui = new ActionQuery()
                {
                    Type = (ActionType)158,
                    ObjId = client.Player.UID,
                    wParam1 = client.Player.X,
                    wParam2 = client.Player.Y
                };
                client.Send(stream.ActionCreate(&Gui));
                ActionQuery action = new ActionQuery()
                {
                    Type = (ActionType)158,
                    ObjId = attacked.UID,
                    wParam1 = attacked.X,
                    wParam2 = attacked.Y
                };
                attacked.Owner.Send(stream.ActionCreate(&action));
            }
            if (attacked.HitPoints <= obj.Damage)
            {
                attacked.DeadState = true;
                if (client.Player.OnTransform)
                {
                    if (client.Player.TransformInfo != null)
                    {
                        client.Player.TransformInfo.FinishTransform();
                    }
                }
                attacked.Dead(client.Player, X, Y, 0);
            }
            else
            {
                CheckAttack.CheckGemEffects.CheckRespouseDamage(attacked.Owner);
                client.UpdateQualifier(client, attacked.Owner, obj.Damage);
                attacked.HitPoints -= (int)obj.Damage;
               
            }
        }
    }
}
