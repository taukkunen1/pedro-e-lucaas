using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetLottery(this ServerSockets.Packet stream, out MsgLottery.Action Mode, out Role.Flags.Gem SocketOne
            , out Role.Flags.Gem SocketTwo, out byte Plus, out byte Color, out byte JadesAddeds, out uint ItemID)
        {
            Mode = (MsgLottery.Action)stream.ReadUInt16();
            stream.ReadUInt8();//unknow
            SocketOne = (Role.Flags.Gem)stream.ReadUInt8();
            SocketTwo = (Role.Flags.Gem)stream.ReadUInt8();
            Plus = stream.ReadUInt8();
            Color = stream.ReadUInt8();
            JadesAddeds = stream.ReadUInt8();
            ItemID = stream.ReadUInt32();
        }

        public static unsafe ServerSockets.Packet LotteryCreate(this ServerSockets.Packet stream, MsgLottery.Action Mode, Role.Flags.Gem SocketOne
            , Role.Flags.Gem SocketTwo, byte Plus, byte Color, byte JadesAddeds, uint ItemID)
        {
            stream.InitWriter();
            stream.Write((ushort)Mode);//4
            stream.Write((byte)2);
            stream.Write((byte)SocketOne);
            stream.Write((byte)SocketTwo);
            stream.Write(Plus);
            stream.Write(Color);
            stream.Write(JadesAddeds);
            stream.Write(ItemID);
            stream.Finalize(GamePackets.MsgLottery);
            return stream;
        }


    }
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public unsafe struct MsgLottery
    {
        public enum Action : uint
        {
            Accept = 0,
            AddJade = 1,
            Continue = 2,
            Show = 515// 263171

        }
        [PacketAttribute(GamePackets.MsgLottery)]
        private static void Procesor(Client.GameClient user, ServerSockets.Packet stream)
        {

            MsgLottery.Action Mode;
            Role.Flags.Gem SocketOne;
            Role.Flags.Gem SocketTwo;
            byte Plus;
            byte Color;
            byte JadesAddeds;
            uint ItemID;

            stream.GetLottery(out Mode, out SocketOne, out SocketTwo, out Plus, out Color, out JadesAddeds, out ItemID);

            switch (Mode)
            {
                case Action.Accept:
                    {
                        if (user.Player.LotteryItem != null)
                        {
                            user.Player.AddJade = 0;

                            user.Inventory.Update(Pool.NewLottery.CreateGameItem(user.Player.LotteryItem), Instance.AddMode.ADD, stream);
                            if (user.Player.LotteryItem.Rank < 5)
                            {
#if Arabic
                                      Program.SendGlobalPackets.Enqueue(new MsgMessage("Congratulations! " + user.Player.Name + " Won " + user.Player.LotteryItem.Name + " in Lottery.", MsgMessage.MsgColor.white, MsgMessage.ChatMode.System).GetArray(stream));
                         
#else
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("Congratulations! " + user.Player.Name + " Won " + user.Player.LotteryItem.Name + " in Lottery.", MsgMessage.MsgColor.white, MsgMessage.ChatMode.System).GetArray(stream));

#endif
                                user.SendSysMesage("You won a " + user.Player.LotteryItem.Name + " from the lottery!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            }
                            user.Player.LotteryItem = null;

                            user.Send(stream.LotteryCreate(Mode, SocketOne, SocketTwo, Plus, Color, JadesAddeds, ItemID));
                        }
                        break;
                    }
                case Action.AddJade:
                    {
                        if (user.Inventory.Contain(711504, 1))
                        {
                            user.Inventory.Remove(711504, 1, stream);
                            user.Player.AddJade++;
                            user.Player.LotteryItem = Pool.NewLottery.GenerateLotteryItem();

                            user.Send(stream.LotteryCreate(Action.Show
                                , (user.Player.LotteryItem.Sockets > 0) ? Role.Flags.Gem.EmptySocket : Role.Flags.Gem.NoSocket
                                , (user.Player.LotteryItem.Sockets > 1) ? Role.Flags.Gem.EmptySocket : Role.Flags.Gem.NoSocket
                                , user.Player.LotteryItem.Plus
                                , user.Player.LotteryItem.Color
                                , user.Player.AddJade
                                , user.Player.LotteryItem.ID));
                        }
                        break;
                    }
                case Action.Continue:
                    {
                        if (user.Player.LotteryEntries < Pool.NewLottery.LotteryEntry(user.Player.VipLevel))
                        {
                            if (user.Inventory.Contain(711504, 3) && user.Player.AddJade < 3)
                            {
                                user.Inventory.Remove(711504, 3, stream);

                                user.Player.LotteryEntries++;
                                //reset lottery jade 
                                user.Player.AddJade = 0;

                                user.Player.LotteryItem = Pool.NewLottery.GenerateLotteryItem();

                                user.Send(stream.LotteryCreate(Action.Show
                              , (user.Player.LotteryItem.Sockets > 0) ? Role.Flags.Gem.EmptySocket : Role.Flags.Gem.NoSocket
                              , (user.Player.LotteryItem.Sockets > 1) ? Role.Flags.Gem.EmptySocket : Role.Flags.Gem.NoSocket
                              , user.Player.LotteryItem.Plus
                              , user.Player.LotteryItem.Color
                              , user.Player.AddJade
                              , user.Player.LotteryItem.ID));
                            }
                            else
                            {
#if Arabic
                                    user.SendSysMesage("You need to pay 3 Small Lottery Tickets to draw from the lottery in the lottery Center.");
#else
                                user.SendSysMesage("You need to pay 3 Small Lottery Tickets to draw from the lottery in the lottery Center.");
#endif

                            }
                        }
                        else
                        {
#if Arabic
                              user.SendSysMesage("Sorry, your LotteryEntries has finished come tomorrow.");
#else
                            user.SendSysMesage("Sorry, your LotteryEntries has finished come tomorrow.");
#endif

                        }
                        break;
                    }
                default:
                    {
                        Console.WriteLine("MsgLottery not found ->" + Mode);
                        break;
                    }
            }
        }
    }
}
