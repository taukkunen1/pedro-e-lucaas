using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public static class MsgSecondaryPassword
    {
        public enum ActionID : uint
        {
            VerifiedPassword = 1,
            ForgetPassword = 2,
            SetNewPass = 3,
            SendInformation = 4,
            PasswordCorrect = 5,
            PasswordWrong = 6
        }

        public static unsafe ServerSockets.Packet SecondaryPasswordCreate(this ServerSockets.Packet stream, ActionID Type, uint OldPassowrd, uint NewPassword)
        {
            stream.InitWriter();
            stream.Write((uint)Type);
            stream.Write(OldPassowrd);
            stream.Write(NewPassword);

            stream.Finalize(GamePackets.SecondaryPassword);
            return stream;
        }
        public static unsafe void GetSecondaryPassword(this ServerSockets.Packet stream, out ActionID Type, out uint OldPassowrd, out uint NewPassword)
        {
            Type = (ActionID)stream.ReadUInt32();
            OldPassowrd = stream.ReadUInt32();
            NewPassword = stream.ReadUInt32();
        }
        [PacketAttribute(GamePackets.SecondaryPassword)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            ActionID Type;
            uint OldPassoword;
            uint NewPassoword;
            stream.GetSecondaryPassword(out Type, out OldPassoword, out NewPassoword);

            switch (Type)
            {
                case ActionID.SetNewPass:
                    {
                        if (user.Player.SecurityPassword == 0)
                        {
                            user.Player.SecurityPassword = NewPassoword;
                            user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordCorrect, 0x0101, 0));
                            user.SendSysMesage("Successfully set! Please remember your secondary password", MsgMessage.ChatMode.System);
                        }
                        break;
                    }
                case ActionID.SendInformation:
                    {
                        if (user.Player.SecurityPassword == 0)
                        {
                            user.Player.SecurityPassword = NewPassoword;
                            user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordCorrect, 0x0101, 0));
                            user.SendSysMesage("Successfully set! Please remember your secondary password", MsgMessage.ChatMode.System);
                        }
                        else
                        {
                            if (OldPassoword == user.Player.SecurityPassword)
                            {
                                user.Player.OnReset = 0;
                                user.Player.SecurityPassword = NewPassoword;
                                user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordCorrect, 0x0101, 0));
                                user.SendSysMesage("Successfully modified! Please remember your secondary password", MsgMessage.ChatMode.System);
                            }
                            else
                            {
                                user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordWrong, 1, 0));
                            }
                        }

                        break;
                    }
                case ActionID.VerifiedPassword:
                    {
                        if (user.Player.SecurityPassword != 0)
                        {
                            if (user.Player.SecurityPassword == NewPassoword)
                            {
                                user.Player.IsCheckedPass = true;
                                user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordCorrect, 0x0101, 0));
                                user.SendSysMesage("Secondary verified!", MsgMessage.ChatMode.System);
                            }
                            else
                            {
                                user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordWrong, 0, 0));
                            }
                        }
                        break;
                    }
                case ActionID.ForgetPassword:
                    {
                        if (user.Player.SecurityPassword != 0)
                        {
                            user.Player.OnReset = 1;
                            user.Player.ResetSecurityPassowrd = DateTime.Now.AddDays(7);
                            user.Send(stream.SecondaryPasswordCreate(ActionID.PasswordCorrect, 0x0101, 0));
                            user.SendSysMesage("Your secondary password will be removed on " + user.Player.ResetSecurityPassowrd.ToString("d/M/yyyy (H:mm)") + ".", MsgMessage.ChatMode.System);
                        }
                        break;
                    }
            }
        }
    }
}
