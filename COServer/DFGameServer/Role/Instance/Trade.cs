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
           if (Target.InTrade)
           {
               if (dwParam > 0
                   && Owner.Player.ConquerPoints >= dwParam
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
           if (Target.InTrade)
           {
               if (dwParam > 0
                   && Owner.Player.Money >= dwParam
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

       public unsafe void CloseTrade()
       {
           using (var rec = new ServerSockets.RecycledPacket())
           {
               var msg = rec.GetStream();

               if (Target.InTrade)
               {
                   Owner.Send(msg.TradeCreate(Owner.Player.UID, MsgTrade.TradeID.CloseTradeWindow));
                   Target.Send(msg.TradeCreate(Owner.Player.UID, MsgTrade.TradeID.CloseTradeWindow));

                   if (!Target.MyTrade.CanRefundCurrency() || !Owner.MyTrade.CanRefundCurrency())
                   {
                       Owner.SendSysMesage("Trade cannot be closed while a currency refund would exceed the balance limit.");
                       Target.SendSysMesage("Trade cannot be closed while a currency refund would exceed the balance limit.");
                       return;
                   }

                   Owner.Player.targetTrade = 0;
                   Target.Player.targetTrade = 0;

                   Target.MyTrade.DestroyItems(msg);
                   Target.MyTrade = null;

                   Owner.MyTrade.DestroyItems(msg);
                   Owner.MyTrade = null;
               }
           }
       }

       public bool CanRefundCurrency()
       {
           return Game.Era1.Era1Faucets.CanAddCurrency(Owner.Player.ConquerPoints, ConquerPoints)
               && Game.Era1.Era1Faucets.CanAddCurrency(Owner.Player.Money, Money);
       }

       public void DestroyItems(ServerSockets.Packet stream)
       {
           if (!CanRefundCurrency())
           {
               Owner.SendSysMesage("Trade refund blocked because the currency balance limit would be exceeded.");
               return;
           }

           uint cps = ConquerPoints;
           uint money = Money;
           ConquerPoints = 0;
           Money = 0;

           Owner.Player.ConquerPoints += cps;
           Owner.Player.Money += money;
          
           Owner.Player.SendUpdate(stream,Owner.Player.Money, Game.MsgServer.MsgUpdate.DataType.Money);

           foreach (var item in Items.Values)
           {
               item.Mode = Flags.ItemMode.AddItem;
               item.Send(Owner,stream);
           }
       }
      
    }
}
