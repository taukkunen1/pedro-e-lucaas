using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer.AttackHandler.Updates
{
   public class IncreaseExperience
    {
       public unsafe static void Up(ServerSockets.Packet stream, Client.GameClient user, uint Damage)
       {
           if (Damage == 0)
               return;
           if (user.Player.ContainFlag(MsgUpdate.Flags.Oblivion))
           {
               user.ExpOblivion += Damage * 4;
           }
           else
           {
               if (user.AutoHunting != null && user.AutoHunting.Enable &&
                   user.AutoHunting.ExpDeliveryMode == AutoHunting.AutoHuntExpDelivery.OnStop)
               {
                   // Freeze all EXP multipliers at the moment this damage/kill EXP is earned.
                   // Stop/logout only transfers this final value and never recalculates it.
                   user.AutoHunting.AddPendingExperience(user.CalculateFinalExperience(Damage));
               }
               else
                   user.IncreaseExperience(stream, Damage);
           }

           if (user.Player.HeavenBlessing > 0)
           {
               user.Player.HuntingBlessing += Damage / 10;
           }
       }
       

    }
}
