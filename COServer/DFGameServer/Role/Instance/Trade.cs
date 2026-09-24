using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using GameServer.Game.MsgServer;

namespace GameServer.Role.Instance
{
    public class Trade
    {
        public Client.GameClient Owner;
        public Client.GameClient Target;
        public uint ConquerPoints;
        public uint Money;
        public bool WindowOpen;
        public bool Confirmed;

        public ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem> Items;

        private readonly object CurrencySync = new object();
        private bool EscrowClosed;

        public Trade(Client.GameClient _owner)
        {
            Owner = _owner;
            Items = new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>();
        }

        public bool ItemInTrade(Game.MsgServer.MsgGameItem Dataitem)
        {
            return Items.ContainsKey(Dataitem.ITEM_ID);
        }

        public unsafe void AddConquerPoints(uint dwParam, ServerSockets.Packet stream)
        {
            if (!Target.InTrade || dwParam == 0)
                return;

            lock (CurrencySync)
            {
                if (EscrowClosed)
                    return;

                if (Owner.Player.ConquerPoints >= dwParam
                    && Game.Era1.Era1Faucets.CanAddCurrency(ConquerPoints, dwParam))
                {
                    Owner.Player.ConquerPoints -= dwParam;
                    ConquerPoints += dwParam;
                    Target.Send(stream.TradeCreate((uint)ConquerPoints, MsgTrade.TradeID.DisplayConquerPoints));
                }
            }
        }

        public unsafe void AddMoney(uint dwParam, ServerSockets.Packet stream)
        {
            if (!Target.InTrade || dwParam == 0)
                return;

            lock (CurrencySync)
            {
                if (EscrowClosed)
                    return;

                if (Owner.Player.Money >= dwParam
                    && Game.Era1.Era1Faucets.CanAddCurrency(Money, dwParam))
                {
                    Owner.Player.Money -= dwParam;
                    Money += dwParam;
                    Target.Send(stream.TradeCreate((ulong)Money, MsgTrade.TradeID.DisplayMoney));
                }
            }
        }

        public bool ValidItems()
        {
            foreach (var item in Items.Values)
                if (!Owner.Inventory.ClientItems.ContainsKey(item.UID))
                    return false;
            return true;
        }

        public unsafe void AddItem(ServerSockets.Packet stream, uint dwparam, Game.MsgServer.MsgGameItem DataItem)
        {
            if (Target.InTrade)
            {
                if (DataItem.Locked != 0)
                {
                    ConcurrentDictionary<uint, Role.Instance.AssociateGS.Member> src;
                    if (!Owner.Player.Associate.Associat.TryGetValue(Role.Instance.AssociateGS.Partener, out src))
                    {
                        Owner.Send(stream.TradeCreate(dwparam, MsgTrade.TradeID.RemoveItem));
                        Owner.SendSysMesage("unable to trade this item.");
                        return;
                    }
                    else if (!src.ContainsKey(Target.Player.UID))
                    {
                        Owner.Send(stream.TradeCreate(dwparam, MsgTrade.TradeID.RemoveItem));
                        Owner.SendSysMesage("unable to trade this item.");
                        return;
                    }
                }

                if (DataItem.Bound >= 1 || DataItem.Inscribed == 1 || Database.ItemType.unabletradeitem.Contains(DataItem.ITEM_ID))
                {
                    Owner.Send(stream.TradeCreate(dwparam, MsgTrade.TradeID.RemoveItem));
                    Owner.SendSysMesage("unable to trade this item.");
                    return;
                }

                if (Target.Inventory.HaveSpace((byte)(Items.Count + 1)))
                {
                    DataItem.Mode = Flags.ItemMode.Trade;
                    DataItem.Send(Target, stream);
                    DataItem.Mode = Flags.ItemMode.AddItem;
                    Items.TryAdd(DataItem.UID, DataItem);
                }
                else
                {
                    Owner.Send(stream.TradeCreate(dwparam, MsgTrade.TradeID.RemoveItem));
                    Owner.SendSysMesage("There is not enough room in your partner inventory.");
                }
            }
        }

        private bool CanRefundCurrencyUnsafe()
        {
            return !EscrowClosed
                && Game.Era1.Era1Faucets.CanAddCurrency(Owner.Player.ConquerPoints, ConquerPoints)
                && Game.Era1.Era1Faucets.CanAddCurrency(Owner.Player.Money, Money);
        }

        private void TakeEscrowUnsafe(out uint cps, out uint money)
        {
            cps = ConquerPoints;
            money = Money;
            ConquerPoints = 0;
            Money = 0;
            EscrowClosed = true;
        }

        public bool CanRefundCurrency()
        {
            lock (CurrencySync)
                return CanRefundCurrencyUnsafe();
        }

        private bool TryTakeRefund(out uint cps, out uint money)
        {
            lock (CurrencySync)
            {
                if (!CanRefundCurrencyUnsafe())
                {
                    cps = 0;
                    money = 0;
                    return false;
                }

                TakeEscrowUnsafe(out cps, out money);
                return true;
            }
        }

        public static bool TryTakePair(
            Trade left,
            Trade right,
            bool requireRefundCapacity,
            out uint leftCps,
            out uint leftMoney,
            out uint rightCps,
            out uint rightMoney)
        {
            leftCps = leftMoney = rightCps = rightMoney = 0;
            if (left == null || right == null)
                return false;

            Trade first = left.Owner.Player.UID <= right.Owner.Player.UID ? left : right;
            Trade second = object.ReferenceEquals(first, left) ? right : left;

            lock (first.CurrencySync)
            {
                lock (second.CurrencySync)
                {
                    if (left.EscrowClosed || right.EscrowClosed)
                        return false;

                    if (requireRefundCapacity
                        && (!left.CanRefundCurrencyUnsafe() || !right.CanRefundCurrencyUnsafe()))
                        return false;

                    left.TakeEscrowUnsafe(out leftCps, out leftMoney);
                    right.TakeEscrowUnsafe(out rightCps, out rightMoney);
                    return true;
                }
            }
        }

        private void ApplyRefund(uint cps, uint money, ServerSockets.Packet stream)
        {
            Owner.Player.ConquerPoints += cps;
            Owner.Player.Money += money;
            Owner.Player.SendUpdate(stream, Owner.Player.Money, Game.MsgServer.MsgUpdate.DataType.Money);

            foreach (var item in Items.Values)
            {
                item.Mode = Flags.ItemMode.AddItem;
                item.Send(Owner, stream);
            }
        }

        public unsafe void CloseTrade()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var msg = rec.GetStream();
                var targetTrade = Target != null ? Target.MyTrade : null;

                uint ownerCps;
                uint ownerMoney;
                uint targetCps = 0;
                uint targetMoney = 0;

                bool refunded;
                if (targetTrade != null && !object.ReferenceEquals(targetTrade, this))
                {
                    refunded = TryTakePair(
                        this,
                        targetTrade,
                        true,
                        out ownerCps,
                        out ownerMoney,
                        out targetCps,
                        out targetMoney);
                }
                else
                {
                    refunded = TryTakeRefund(out ownerCps, out ownerMoney);
                }

                if (!refunded)
                {
                    Owner.SendSysMesage("Trade cannot be closed while the escrow cannot be refunded safely.");
                    if (Target != null)
                        Target.SendSysMesage("Trade cannot be closed while the escrow cannot be refunded safely.");
                    return;
                }

                // Refund first, then close UI/references. This guarantees that a
                // disconnect or broken peer link cannot silently discard escrow.
                ApplyRefund(ownerCps, ownerMoney, msg);
                if (targetTrade != null && !object.ReferenceEquals(targetTrade, this))
                    targetTrade.ApplyRefund(targetCps, targetMoney, msg);

                Owner.Player.targetTrade = 0;
                if (Target != null)
                    Target.Player.targetTrade = 0;

                if (Owner.Socket != null && Owner.Socket.Alive)
                    Owner.Send(msg.TradeCreate(Owner.Player.UID, MsgTrade.TradeID.CloseTradeWindow));
                if (Target != null && Target.Socket != null && Target.Socket.Alive)
                    Target.Send(msg.TradeCreate(Owner.Player.UID, MsgTrade.TradeID.CloseTradeWindow));

                if (Target != null && object.ReferenceEquals(Target.MyTrade, targetTrade))
                    Target.MyTrade = null;
                if (object.ReferenceEquals(Owner.MyTrade, this))
                    Owner.MyTrade = null;
            }
        }

        /// <summary>
        /// Economy V5: encerra apenas o lado deste jogador, devolvendo a propria escrow.
        /// Usado quando o vinculo com o parceiro esta quebrado: nunca toca no ledger do
        /// parceiro, que pode pertencer a outro trade. Retorna false se o refund nao for
        /// seguro; nesse caso o trade continua aberto e nada e' descartado.
        /// </summary>
        public bool AbortOwnSide(ServerSockets.Packet stream)
        {
            uint cps;
            uint money;
            if (!TryTakeRefund(out cps, out money))
            {
                Owner.SendSysMesage("Trade cannot be closed while the escrow cannot be refunded safely.");
                return false;
            }

            ApplyRefund(cps, money, stream);
            Owner.Player.targetTrade = 0;
            if (Owner.Socket != null && Owner.Socket.Alive)
                Owner.Send(stream.TradeCreate(Owner.Player.UID, MsgTrade.TradeID.CloseTradeWindow));
            if (object.ReferenceEquals(Owner.MyTrade, this))
                Owner.MyTrade = null;
            return true;
        }

        public void DestroyItems(ServerSockets.Packet stream)
        {
            uint cps;
            uint money;
            if (!TryTakeRefund(out cps, out money))
            {
                Owner.SendSysMesage("Trade refund blocked because the escrow cannot be refunded safely.");
                return;
            }

            ApplyRefund(cps, money, stream);
        }
    }
}
