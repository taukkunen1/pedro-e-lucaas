using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{

    public static unsafe partial class MsgBuilder
    {

        public static unsafe void GetFlower(this ServerSockets.Packet stream, out MsgFlower.FlowerAction action
            , out uint UID, out uint ItemUID, out string SenderName, out string ReceiverName, out uint SendAmount)
        {
            action = (MsgFlower.FlowerAction)stream.ReadUInt32();
            UID = stream.ReadUInt32();
            ItemUID = stream.ReadUInt32();
            SenderName = stream.ReadCString(16);
            ReceiverName = stream.ReadCString(16);
            SendAmount = stream.ReadUInt32();
        }

        public static unsafe ServerSockets.Packet FlowerCreate(this ServerSockets.Packet stream, MsgFlower.FlowerAction action
            , uint UID, uint ItemUID, string SenderName, string ReceiverName, uint SendAmount, MsgFlower.FlowersType FlowerTyp
            , MsgFlower.FlowerEffect Effect)
        {
            stream.InitWriter();

            stream.Write((uint)action);
            stream.Write(UID);
            stream.Write(ItemUID);
            stream.Write(SenderName, 16);
            stream.Write(ReceiverName, 16);
            stream.Write(SendAmount);
            stream.Write((uint)FlowerTyp);
            stream.Write((uint)Effect);

            stream.Finalize(GamePackets.FlowerPacket);

            return stream;
        }

        public static unsafe ServerSockets.Packet FlowerCreate(this ServerSockets.Packet stream, MsgFlower.FlowerAction action
         , uint UID = 0, uint ItemUID = 0, uint RedRoses = 0, uint RedRoses2day = 0, uint Lilies = 0, uint Lilies2day = 0
            , uint Orchids = 0, uint Orchids2day = 0, uint Tulips = 0, uint Tulips2day = 0
            , uint SendAmount = 0, MsgFlower.FlowersType FlowerTyp = MsgFlower.FlowersType.Rouse
            , MsgFlower.FlowerEffect Effect = MsgFlower.FlowerEffect.None)
        {
            stream.InitWriter();

            stream.Write((uint)action);
            stream.Write(0);
            stream.Write(0);
            stream.Write(RedRoses);//16
            stream.Write(RedRoses2day);//20
            stream.Write(Lilies);//24
            stream.Write(Lilies2day);//28
            stream.Write(Orchids);//32
            stream.Write(Orchids2day);//36
            stream.Write(Tulips);//40
            stream.Write(Tulips2day);//44
            stream.Finalize(GamePackets.FlowerPacket);

            return stream;
        }
    }
    public unsafe struct MsgFlower
    {
        public enum FlowerAction
        {
            None = 0,
            GirlSend = 1,
            FlowerSender = 2, 
            Flower = 3
        }
        public enum FlowersType : uint
        {
            Rouse = 0,
            Lilies = 1,
            Orchids = 2,
            Tulips = 3,

            Kiss = 4,
            love = 5,
            Tins = 6,
            Jade = 7,
        }
        public enum FlowerEffect : uint
        {
            None = 0,

            Rouse = 1,
            Lilies = 2,
            Orchids = 3,
            Tulips = 4,

            Kiss = 1,
            love = 2,
            Tins = 3,
            Jade = 4,
        }
   
        [PacketAttribute(GamePackets.FlowerPacket)]
        public unsafe static void Handler(Client.GameClient user, ServerSockets.Packet packet)
        {
            if (!user.Player.OnMyOwnServer)
                return;
             MsgFlower.FlowerAction action;
             uint UID;
             uint ItemUID; 
            string SenderName;
             string ReceiverName; 
            uint SendAmount;

            packet.GetFlower(out action, out UID, out ItemUID, out SenderName, out ReceiverName, out SendAmount);


            if (Role.Core.IsBoy(user.Player.Body) && action == FlowerAction.None)
            {
                switch (ItemUID)
                {
                    case 0:
                        {
                            if (user.Player.Flowers.FreeFlowers > 0)
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Role.Player Target = obj as Role.Player;
                                    if (Role.Core.IsGirl(Target.Body))
                                    {
                                        if (!Role.Instance.Flowers.ClientPoll.ContainsKey(Target.UID))
                                            Role.Instance.Flowers.ClientPoll.TryAdd(Target.UID, Target.Flowers);

                                        Target.Flowers.RedRoses += user.Player.Flowers.FreeFlowers;

                                        Target.View.SendView(packet.FlowerCreate(action, UID, ItemUID, user.Player.Name, Target.Name, user.Player.Flowers.FreeFlowers, FlowersType.Rouse, FlowerEffect.Rouse), true);

                                        user.Player.Flowers.FreeFlowers = 0;
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        {
                            MsgGameItem GameItem;
                            if (user.Inventory.TryGetItem(ItemUID, out GameItem))
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Role.Player Target = obj as Role.Player;
                                    if (Role.Core.IsGirl(Target.Body))
                                    {
                                        if (!Role.Instance.Flowers.ClientPoll.ContainsKey(Target.UID))
                                            Role.Instance.Flowers.ClientPoll.TryAdd(Target.UID, Target.Flowers);

                                        SendAmount = (uint)(GameItem.ITEM_ID % 1000);

                                        if (SendAmount != Pool.ItemsBase[GameItem.ITEM_ID].Durability)
                                            break;

                                        var FlowerTyp = GetFlowerTyp(GameItem.ITEM_ID);
                                        var Flowers = Target.Flowers.SingleOrDefault(p => p.Type == FlowerTyp);
                                        if (Flowers != null)
                                        {
                                            Flowers += SendAmount;

                                            Pool.GirlsFlowersRanking.UpdateRank(Flowers, FlowerTyp);

                                            uint FlowersToday = Target.Flowers.AllFlowersToday();
                                            Pool.FlowersRankToday.UpdateRank(Target.UID, FlowersToday);


                                            Target.View.SendView(packet.FlowerCreate(action, UID, ItemUID, user.Player.Name, Target.Name, SendAmount, FlowerTyp, (FlowerEffect)(FlowerTyp + 1)), true);

                                            user.Inventory.Update(GameItem, Instance.AddMode.REMOVE,packet);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            else if (Role.Core.IsGirl(user.Player.Body) &&action == FlowerAction.GirlSend)
            {
                switch (ItemUID)
                {
                    case 0:
                        {
                            if (user.Player.Flowers.FreeFlowers > 0)
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Role.Player Target = obj as Role.Player;
                                    if (Role.Core.IsBoy(Target.Body))
                                    {
                                        if (!Role.Instance.Flowers.ClientPoll.ContainsKey(Target.UID))
                                            Role.Instance.Flowers.ClientPoll.TryAdd(Target.UID, Target.Flowers);

                                        Target.Flowers.RedRoses += user.Player.Flowers.FreeFlowers;

                                        Target.View.SendView(packet.FlowerCreate(action, UID, ItemUID, user.Player.Name, Target.Name, user.Player.Flowers.FreeFlowers, FlowersType.Kiss, FlowerEffect.Kiss), true);

                                        user.Player.Flowers.FreeFlowers = 0;
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        {
                            MsgGameItem GameItem;
                            if (user.Inventory.TryGetItem(ItemUID, out GameItem))
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Role.Player Target = obj as Role.Player;
                                    if (Role.Core.IsBoy(Target.Body))
                                    {
                                        if (!Role.Instance.Flowers.ClientPoll.ContainsKey(Target.UID))
                                            Role.Instance.Flowers.ClientPoll.TryAdd(Target.UID, Target.Flowers);

                                       SendAmount = (uint)(GameItem.ITEM_ID % 1000);

                                        if (SendAmount != Pool.ItemsBase[GameItem.ITEM_ID].Durability)
                                            break;

                                        var FlowerTyp = GetFlowerTyp(GameItem.ITEM_ID);
                                        var Flowers = Target.Flowers.SingleOrDefault(p => p.Type == FlowerTyp);
                                        if (Flowers != null)
                                        {
                                            Flowers += SendAmount;

                                            Pool.BoysFlowersRanking.UpdateRank(Flowers, FlowerTyp);

                                            Target.View.SendView(packet.FlowerCreate(action, UID, ItemUID, user.Player.Name, Target.Name, SendAmount, (FlowersType)(FlowerTyp + 4), (FlowerEffect)(FlowerTyp + 1)), true);

                                            user.Inventory.Update(GameItem, Instance.AddMode.REMOVE,packet);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }

        public static FlowersType GetFlowerTyp(uint ID)
        {
            if (ID >= 751001 && ID <= 751999 || ID >= 755001 && ID <= 755999)
                return FlowersType.Rouse;
            if (ID >= 752001 && ID <= 752999 || ID >= 756001 && ID <= 756999)
                return FlowersType.Lilies;
            if (ID >= 753001 && ID <= 753999 || ID >= 757001 && ID <= 757999)
                return FlowersType.Orchids;
            if (ID >= 754001 && ID <= 754999 || ID >= 758001 && ID <= 758999)
                return FlowersType.Tulips;
            return 0;
        }

    }
}
