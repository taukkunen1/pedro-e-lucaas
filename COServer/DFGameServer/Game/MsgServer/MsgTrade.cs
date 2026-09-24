using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static class MsgTrade
    {
        public enum TradeID : uint
        {
            RequestNewTrade = 0x01,
            RequestCloseTrade = 0x02,
            RequestAddItemToTrade = 0x06,
            RequestAddMoneyToTrade = 0x07,
            RequestAddConquerPointsToTrade = 0x0D,
            RequestCompleteTrade = 0x0A,
            RemoveItem = 11,
            ShowTradeWindow = 0x03,
            CloseTradeWindow = 0x05,
            DisplayMoney = 0x08,
            DisplayConquerPoints = 0x0C
        }

        public static unsafe void GetTrade(this ServerSockets.Packet stream, out ulong dwParam, out TradeID ID)
        {
            dwParam = stream.ReadUInt32();
            ID = (TradeID)stream.ReadUInt16();
        }

        public static unsafe ServerSockets.Packet TradeCreate(this ServerSockets.Packet stream, ulong dwParam, TradeID ID)
        {
            stream.InitWriter();

            stream.Write((uint)dwParam);//4
  
            stream.Write((ushort)ID);//8


            stream.Finalize(GamePackets.Trade);
            return stream;
        }
     
        [PacketAttribute(GamePackets.Trade)]
        private static void HandlerTrade(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (!user.Player.IsCheckedPass)
                return;
            ulong dwParam;
            TradeID ID;
            stream.GetTrade(out dwParam, out ID);
            if (user.PokerPlayer != null)
            {
                return;
            }
            if (user.MyTrade != null)
            {
                if (user.MyTrade.Target != null)
                {
                    if (user.MyTrade.Target.PokerPlayer != null)
                    {
                        return;
                    }
                }
            }
            switch (ID)
            {

                case TradeID.RequestNewTrade:
                    {
                        if (user.MyTrade == null)
                            user.MyTrade = new Role.Instance.Trade(user);


                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue((uint)dwParam, out obj, Role.MapObjectType.Player))
                        {
                            Client.GameClient Partner = (obj as Role.Player).Owner;
                            if (Partner != null)
                            {
                                if (!Partner.InTrade && !user.InTrade)
                                {
                                    user.Player.targetTrade = (uint)Partner.Player.UID;
                                    user.MyTrade.Target = Partner;
                                    if (Partner.Player.targetTrade == user.Player.UID && user.Player.targetTrade == Partner.Player.UID)
                                    {
                                        if (user.MyTrade.Target.Player.UID == Partner.Player.UID && Partner.MyTrade.Target.Player.UID == user.Player.UID)
                                        {
                                            user.MyTrade.Target.MyTrade = new Role.Instance.Trade(user.MyTrade.Target);
                                            user.MyTrade.Target.MyTrade.Target = user;
                                            Partner.MyTrade.Target = user;
                                            Partner.MyTrade.WindowOpen = true;
                                            user.MyTrade.WindowOpen = true;
                                            Partner.MyTrade.Confirmed = false;
                                            user.MyTrade.Confirmed = false;
                                            user.Send(stream.TradeCreate(dwParam, TradeID.ShowTradeWindow));
                                            Partner.Send(stream.TradeCreate(user.Player.UID, TradeID.ShowTradeWindow));
                                        }
                                        else
                                        {
                                            user.SendSysMesage("Player already in a trade.");
                           
                                        }
                                    }
                                    else
                                    {

                                        Partner.Send(stream.PopupInfoCreate(user.Player.UID, Partner.Player.UID, user.Player.Level, user.Player.BattlePower));
                                        Partner.Send(stream.TradeCreate(user.Player.UID, TradeID.RequestNewTrade));

                                    }
                                }
                                else
                                {
                                    user.SendSysMesage("Player already in a trade.");
                                    
                                }
                            }
                        }
                        break;
                    }
                case TradeID.RequestCloseTrade:
                    {
                        if (user.InTrade)
                        {
                            user.MyTrade.CloseTrade();
                        }
                        break;
                    }
                case TradeID.RequestAddItemToTrade:
                    {
                        if (user.InTrade)
                        {
                            Game.MsgServer.MsgGameItem DataItem;
                            if (user.Inventory.TryGetItem((uint)dwParam, out DataItem))
                                user.MyTrade.AddItem(stream, (uint)dwParam, DataItem);
                        }
                        break;
                    }
                case TradeID.RequestAddConquerPointsToTrade:
                    {
                        if (user.InTrade)
                            user.MyTrade.AddConquerPoints((uint)dwParam, stream);
                        break;
                    }
                case TradeID.RequestAddMoneyToTrade:
                    {
                        if (user.InTrade)
                            user.MyTrade.AddMoney((uint)dwParam, stream);
                        break;
                    }
                case TradeID.RequestCompleteTrade:
                    {
                        if (!user.InTrade || user.MyTrade.Target == null)
                            break;

                        if (user.MyTrade.Target.Socket != null && user.MyTrade.Target.Socket.Alive)
                        {
                            // Economy V5: vinculo quebrado com o parceiro nunca descarta a
                            // escrow. Devolve so o lado deste jogador e fecha a janela.
                            var partnerTrade = user.MyTrade.Target.MyTrade;
                            bool linked = partnerTrade != null
                                && partnerTrade.Target != null
                                && partnerTrade.Target.Player.UID == user.Player.UID;
                            if (!linked)
                            {
                                user.MyTrade.AbortOwnSide(stream);
                                break;
                            }
                            if (user.InTrade)
                            {
                                if (user.MyTrade.Target.InTrade)
                                {
                                    user.MyTrade.Confirmed = true;
                                    if (!user.MyTrade.Target.MyTrade.Confirmed)
                                    {

                                        user.MyTrade.Target.Send(stream.TradeCreate(dwParam, TradeID.RequestCompleteTrade));
                                    }
                                    else
                                    {
                                        var userTrade = user.MyTrade;
                                        var target = userTrade.Target;
                                        var targetTrade = target.MyTrade;

                                        bool canReceiveCurrency =
                                            Game.Era1.Era1Faucets.CanAddCurrency(user.Player.ConquerPoints, targetTrade.ConquerPoints)
                                            && Game.Era1.Era1Faucets.CanAddCurrency(user.Player.Money, targetTrade.Money)
                                            && Game.Era1.Era1Faucets.CanAddCurrency(target.Player.ConquerPoints, userTrade.ConquerPoints)
                                            && Game.Era1.Era1Faucets.CanAddCurrency(target.Player.Money, userTrade.Money);

                                        bool accepted =
                                            canReceiveCurrency
                                            && user.Inventory.HaveSpace((byte)targetTrade.Items.Count)
                                            && targetTrade.ValidItems()
                                            && target.Inventory.HaveSpace((byte)userTrade.Items.Count)
                                            && userTrade.ValidItems();

                                        if (!accepted)
                                        {
                                            userTrade.Confirmed = false;
                                            targetTrade.Confirmed = false;
                                            user.SendSysMesage("There was an error with the trade. Currency and items remain in the trade window.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                            target.SendSysMesage("There was an error with the trade. Currency and items remain in the trade window.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                            break;
                                        }

                                        uint userEscrowCps;
                                        uint userEscrowMoney;
                                        uint targetEscrowCps;
                                        uint targetEscrowMoney;

                                        // Economy V5: atomically consume both escrow ledgers before
                                        // crediting either side. Concurrent close/disconnect/confirm
                                        // paths cannot settle or refund the same escrow twice.
                                        if (!Role.Instance.Trade.TryTakePair(
                                            userTrade,
                                            targetTrade,
                                            false,
                                            out userEscrowCps,
                                            out userEscrowMoney,
                                            out targetEscrowCps,
                                            out targetEscrowMoney))
                                        {
                                            userTrade.Confirmed = false;
                                            targetTrade.Confirmed = false;
                                            user.SendSysMesage("Trade settlement was cancelled because the escrow was already closed.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                            target.SendSysMesage("Trade settlement was cancelled because the escrow was already closed.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                            break;
                                        }

                                        user.Player.ConquerPoints += targetEscrowCps;
                                        user.Player.Money += targetEscrowMoney;
                                        user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);

                                        target.Player.ConquerPoints += userEscrowCps;
                                        target.Player.Money += userEscrowMoney;
                                        target.Player.SendUpdate(stream, target.Player.Money, MsgUpdate.DataType.Money);

                                        foreach (var item in targetTrade.Items.Values)
                                        {
                                            user.Inventory.Update(item, Instance.AddMode.MOVE, stream);
                                            target.Inventory.Update(item, Instance.AddMode.REMOVE, stream, true);
                                        }
                                        foreach (var item in userTrade.Items.Values)
                                        {
                                            target.Inventory.Update(item, Instance.AddMode.MOVE, stream);
                                            user.Inventory.Update(item, Instance.AddMode.REMOVE, stream, true);
                                        }

                                        user.Player.targetTrade = 0;
                                        target.Player.targetTrade = 0;
                                        user.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));
                                        target.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));

                                        target.MyTrade = null;
                                        user.MyTrade = null;

                                    }
                                }
                            }
                        } else
                        {
                            user.MyTrade.Owner.Player.MessageBox("the player you are trying to trade with is not online", null, null);
                            user.MyTrade.CloseTrade();
                        }
                        break;
                    }
            }
        }
    }
}
