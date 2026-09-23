using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public static class MsgReincarnation
    {

        public static void GetReincarnation(this ServerSockets.Packet msg, out uint ToClass, out uint ToBody)
        {
            //int timerstamp = msg.ReadInt32();

            ToClass = msg.ReadUInt32();
            ToBody = msg.ReadUInt32();
        }
        [PacketAttribute(Game.GamePackets.Reincarnation)]
        public unsafe static void Proces(Client.GameClient user, ServerSockets.Packet stream)
        {

            uint ToClass;
            uint ToBody;

            stream.GetReincarnation(out ToClass, out ToBody);

            // Era 1 has no reincarnation/third-life system. The 5017 target keeps
            // only first and second rebirth, so packets for the later system are rejected.
            user.CreateBoxDialog("Reincarnation is not available in Era 1.");
            return;

#pragma warning disable CS0162
            if (user.Inventory.HaveSpace(2))
            {
                if (ToClass == 11 || ToClass == 21 || ToClass == 41 || ToClass == 132 || ToClass == 142)
                {
                    if (user.Inventory.Contain(711083, 1) || user.Inventory.Contain(711083, 1, 1))
                    {
                        if (user.Player.Level >= 110 && user.Player.Reborn == 2)
                        {
                            if (Role.Core.IsBoy(user.Player.Body))
                            {
                                switch (ToBody)
                                {
                                    case 1:
                                        user.Player.Body = 1003;
                                        break;
                                    case 2:
                                        user.Player.Body = 1004;
                                        break;
                                }
                            }
                            else
                            {
                                switch (ToBody)
                                {
                                    case 1:
                                        user.Player.Body = 2001;
                                        break;
                                    case 2:
                                        user.Player.Body = 2002;
                                        break;
                                }
                            }
                            user.Inventory.Remove(711083, 1, stream);
                            Pool.RebornInfo.Reborn(user.Player, (byte)ToClass, stream);
                        }
                        else
                            user.CreateBoxDialog("You have not been Reborn twice or you are not level 110 ++");
                    }
                }
            }
            else user.CreateBoxDialog("You need 2 free spaces in your inventory.");
#pragma warning restore CS0162
        }
    }
}