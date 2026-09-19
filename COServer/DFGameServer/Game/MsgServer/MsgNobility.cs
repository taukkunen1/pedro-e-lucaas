using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetNobility(this ServerSockets.Packet stream, out MsgNobility.NobilityAction mode, out ulong UID, out MsgNobility.DonationTyp donationtyp)
        {
            mode = (MsgNobility.NobilityAction)stream.ReadInt32();//4
            UID = stream.ReadUInt64();//8
            stream.ZeroFill(88);
            donationtyp = (MsgNobility.DonationTyp)stream.ReadUInt16();
        }
        public static unsafe ServerSockets.Packet NobilityIconCreate(this ServerSockets.Packet stream, Role.Instance.Nobility nobility)
        {
            stream.InitWriter();

            stream.Write((uint)MsgNobility.NobilityAction.Icon);//4
            stream.Write(nobility.UID);//8

            string StrList = "" + nobility.UID + " " + nobility.Donation + " " + (byte)nobility.Rank + " " + nobility.Position + "";

            stream.ZeroFill(108);

            stream.Write(StrList);

            stream.Finalize(GamePackets.Nobility);

            return stream;
        }
    }

    public unsafe struct MsgNobility
    {


        public enum NobilityAction : uint
        {
            Donate = 1,
            RankListen = 2,
            Icon = 3,
            NobilityInformation = 4,
        }
        public enum DonationTyp : byte
        {
            Money = 0,
            ConquerPoints = 1
        }


        [PacketAttribute(GamePackets.Nobility)]
        public static void HandlerNobility(Client.GameClient user, ServerSockets.Packet stream)
        {
            NobilityAction Action;
            ulong UID;
            DonationTyp donationtyp;
            stream.GetNobility(out Action, out UID, out donationtyp);

            switch (Action)
            {
                case NobilityAction.Donate:
                    {
                        if (!user.Player.OnMyOwnServer)
                            return;
                        if (user.InTrade)
                            return;
                        if (user.PokerPlayer != null)
                            return;
                        if (user.Player.Map == 1767)
                            return;
                        switch (donationtyp)
                        {
                            #region NobilitySystem Donation
                            case DonationTyp.Money:
                                {
                                    if (user.Player.Money >= UID)
                                    {
                                        user.Player.Money -= (uint)(UID);
                                        user.Player.Nobility.Donation += UID; 
                                        user.Send(stream.NobilityIconCreate(user.Player.Nobility));
                                        Pool.NobilityRanking.UpdateRank(user.Player.Nobility);
                                    }
                                    break;
                                }
                            case DonationTyp.ConquerPoints:
                                {
                                    ulong conquerPointsCost = UID / 50000;
                                    if (user.Player.ConquerPoints >= conquerPointsCost)
                                    {
                                        user.Player.ConquerPoints -= (uint)conquerPointsCost;
                                        user.Player.Nobility.Donation += conquerPointsCost * 50000;
                                        user.Send(stream.NobilityIconCreate(user.Player.Nobility));
                                        Pool.NobilityRanking.UpdateRank(user.Player.Nobility);
                                    }
                                    break;
                                }
                                #endregion
                        }
                        break;
                    }
                case NobilityAction.RankListen:
                    {
                        int displyPage = (int)UID;
                        var info = Pool.NobilityRanking.GetArray();
                        try
                        {
                            const int max = 10;
                            int offset = displyPage * max;
                            int count = Math.Min(max, Math.Max(0, info.Length - offset));

                            stream.InitWriter();

                            stream.Write((uint)NobilityAction.RankListen);
                            stream.Write((ushort)displyPage);//8
                            int max_show = (int)Math.Ceiling((info.Length * 1.0) / max);
                            stream.Write((ushort)(max_show));//10
                            int count_show = 50;
                            if (info.Length < 50)
                            {
                                int current = info.Length / 10;
                                if (current == displyPage)
                                    count_show = current;

                            }
                            if (info.Length < 10)
                                count_show = info.Length;
                            else
                                count_show = info.Length - offset;
                            stream.Write((ushort)count_show);//12

                            stream.ZeroFill(106);//14

                            for (int x = 0; x < count; x++)
                            {
                                if (info.Length > offset + x)
                                {
                                    var element = info[offset + x];
                                    if (element.Position < 50)
                                    {
                                        stream.Write(element.UID);
                                        stream.Write((uint)element.Gender);
                                        stream.Write(element.Mesh);
                                        stream.Write(element.Name, 16);
                                        stream.Write((uint)0);
                                        stream.Write(element.Donation);
                                        stream.Write((uint)element.Rank);
                                        stream.Write(element.Position);
                                    }
                                }
                            }
                            stream.Finalize(Game.GamePackets.Nobility);

                            user.Send(stream);

                        }
                        catch (Exception e) { Console.WriteLine(e.ToString()); }
                        break;
                    }
                case NobilityAction.NobilityInformation:
                    {
                        stream.InitWriter();

                        stream.Write((uint)NobilityAction.NobilityInformation);

                        stream.Write(Pool.NobilityRanking.KnightDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Earl);

                        stream.Write(Pool.NobilityRanking.KnightDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Earl);

                        stream.Write(Pool.NobilityRanking.EarlDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Earl);

                        stream.Write(Pool.NobilityRanking.DukeDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Duke);

                        stream.Write(Pool.NobilityRanking.PrinceDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Prince);

                        stream.Write(Pool.NobilityRanking.KingDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.King);

                        stream.Finalize(GamePackets.Nobility);

                        user.Send(stream);


                        break;
                    }

            }

        }
    }
}
