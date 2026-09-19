using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using System;
using static GameServer.Game.MsgServer.MsgMessage;

namespace GameServer
{
    public class NewItems
    {
        public static bool SwitchID(Client.GameClient client , Game.MsgServer.MsgGameItem ItemDat)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                switch (ItemDat.ITEM_ID)
                {
                    case 728895:
                        {
                            client.Inventory.Update(ItemDat, Instance.AddMode.REMOVE, stream);
                            client.Player.DonationPoints += 100;
                            client.Player.SendUpdate(stream, client.Player.DonationPoints, MsgUpdate.DataType.RaceShopPoints);
                            client.CreateBoxDialog("You received 100 DonationPoints.");
                            client.SendSysMesage("You received 100 DonationPoints.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            break;
                        }
                    case 729278:
                        {
                            client.Inventory.Update(ItemDat, Instance.AddMode.REMOVE, stream);
                            client.Player.DonationPoints += 50;
                            client.Player.SendUpdate(stream, client.Player.DonationPoints, MsgUpdate.DataType.RaceShopPoints);
                            client.CreateBoxDialog("You received 50 DonationPoints.");
                            client.SendSysMesage("You received 50 DonationPoints.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            break;
                        }
                    case 729461:
                        {
                            client.Inventory.Update(ItemDat, Instance.AddMode.REMOVE, stream);

                            uint id = StoneId(7);
                            client.Inventory.AddItemWitchStack(id, 7, 1, stream, false);
                            client.CreateBoxDialog("You've received Stone(+7).");
                            client.SendSysMesage("You've received Stone(+7).", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            break;
                        }
                    case 721300:
                        {
                            client.Inventory.Update(ItemDat, Instance.AddMode.REMOVE, stream);
                            uint id = StoneId(8);
                            client.Inventory.AddItemWitchStack(id, 8, 1, stream, false);
                            client.CreateBoxDialog("You've received Stone(+8).");
                            client.SendSysMesage("You've received Stone(+8).", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            break;
                        }
                    case 728352:
                        {
                            EventBag(stream, client, ItemDat);
                            return true;
                        }
                    case 724002://LotteryTickets
                        {
                            if (client.Inventory.HaveSpace(1))
                            {
                                client.Inventory.Update(ItemDat, Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(711504, 0, 3, stream);
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "lottery");

                                client.SendSysMesage("You opened the Small Lottery Ticket Packet and received 3 Small Lottery Tickets!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            }
                            else
                            {
                                client.SendSysMesage("Please make 1 more spaces in your inventory.");
                            }
                            break;
                        }
                }
                return false;
            }
        }
        public static uint StoneId(UInt16 Plus)
        {
            switch (Plus)
            {
                case 1: return 730001;
                case 2: return 730002;
                case 3: return 730003;
                case 4: return 730004;
                case 5: return 730005;
                case 6: return 730006;
                case 7: return 730007;
                case 8: return 730008;
            }
            return 0;
        }
        public static void EventBag(ServerSockets.Packet stream, Client.GameClient client, Game.MsgServer.MsgGameItem ItemDat)
        {
            if (!client.Inventory.HaveSpace(10))
                client.CreateBoxDialog("Please make 10 more spaces in your inventory.");
            else
            {
                client.Inventory.Update(ItemDat, Instance.AddMode.REMOVE, stream);
                
                if (Role.MyMath.Success(30.0))
                {
                    client.Inventory.AddItemWitchStack(711504, 0, 10, stream, false);
                    client.CreateBoxDialog("You've received 10 Small Lottery Tickets.");
                    MsgBags(stream, client, "10xSmallLotteryTickets", ((MsgStaticMessage.Messages)ItemDat.IDEvent).ToString());
                }
                else if (Role.MyMath.Success(5.2))
                {
                    byte plus = (byte)Role.Core.Random.Next(4, 7);
                    if (plus >= 7)
                        plus = 6;
                    uint id = StoneId(plus);
                    client.Inventory.AddItemWitchStack(id, plus, 1, stream, false);
                    client.CreateBoxDialog("You've received Stone(+" + plus + ").");
                    MsgBags(stream, client, "Stone(+" + plus + ")", ((MsgStaticMessage.Messages)ItemDat.IDEvent).ToString());
                }
                else
                {
                    uint value = EventsRewards.EventReward("LuckyBag").RewardValue;
                    client.Player.ConquerPoints += value;
                    client.CreateBoxDialog("You've received " + value + " ConquerPoints.");
                    MsgBags(stream, client, value + " ConquerPoints", ((MsgStaticMessage.Messages)ItemDat.IDEvent).ToString());
                }
            }
        }
        public static void MsgBags(ServerSockets.Packet stream, Client.GameClient client, string txt, string eventname)
        {
            string msg = $"{ client.Player.Name } has won " + txt + " when he opened [LuckyBag] from the hourly " + eventname + " Tournament!";
            string reward = "[EVENT]" + msg;
            if (txt != "")
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, MsgColor.red, ChatMode.System).GetArray(stream));
            Database.ServerDatabase.LoginQueue.Enqueue(reward);
        }
    }
}
