using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet GuildRanksCreate(this ServerSockets.Packet stream, MsgGuildRanks.RankTyp rank, ushort count, ushort Page)
        {
            stream.InitWriter();


            stream.Write((ushort)rank);
            stream.Write(count);
            stream.Write((ushort)20);//register count;
            stream.Write(Page);

            return stream;
        }
        public static unsafe ServerSockets.Packet AddItemGuildRanks(this ServerSockets.Packet stream, MsgGuildRanks.RankTyp rank, Role.Instance.Guild.Member member, long Donation, int RealRank)
        {
            stream.Write(member.UID);
            stream.Write((uint)member.Rank);
            stream.Write((uint)RealRank);
            stream.ZeroFill((ushort)(4 * (ushort)rank));
            stream.Write((uint)Donation);
            stream.ZeroFill(36 - (ushort)(4 * (ushort)rank));
            stream.Write(member.Name, 16);

            return stream;
        }
        public static unsafe ServerSockets.Packet GuildRanksFinalize(this ServerSockets.Packet stream)
        {

            stream.Finalize(GamePackets.GuildRanks);
            return stream;
        }
    }
    public unsafe struct MsgGuildRanks
    {
        public enum RankTyp : ushort
        {
            SilverRank = 0,
            CpRank = 1,
            GuideDonation = 2,
            PkRank = 3,
            ArsenalRank = 4,
            RosesRank = 5,
            OrchidRank = 6,
            LilyRank = 7,
            TulipRank = 8,
            TotalDonaion = 9,
            MaxCounts = 10
        }
        [PacketAttribute(GamePackets.GuildRanks)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.Player.MyGuild == null)
                return;
            RankTyp Rank = (RankTyp)stream.ReadUInt8();
            byte Page = stream.ReadUInt8();
            Page = (byte)Math.Min(2, (int)Page);
            switch (Rank)
            {
                case RankTyp.SilverRank:
                    {
                        var array = user.Player.MyGuild.RankSilversDonations.OrderByDescending(e => e.MoneyDonate).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));


                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;
                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.MoneyDonate, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.CpRank:
                    {
                        var array = user.Player.MyGuild.RankCPDonations.OrderByDescending(e => e.CpsDonate).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));


                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.CpsDonate, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.GuideDonation:
                    {
                        var array = user.Player.MyGuild.RankGuideDonations.OrderByDescending(e => e.VirtutePointes).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));


                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.VirtutePointes, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.PkRank:
                    {
                        var array = user.Player.MyGuild.RankPkDonations.OrderByDescending(e => e.PkDonation).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));


                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.PkDonation, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.ArsenalRank:
                    {
                        var array = user.Player.MyGuild.RankArsenalDonations.OrderByDescending(e => e.ArsenalDonation).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));


                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.ArsenalDonation, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.RosesRank:
                    {
                        var array = user.Player.MyGuild.RankRosseDonations.OrderByDescending(e => e.Rouses).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));

                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.Rouses, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.LilyRank:
                    {
                        var array = user.Player.MyGuild.RankLiliesDonations.OrderByDescending(e => e.Lilies).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));

                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.Lilies, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.OrchidRank:
                    {
                        var array = user.Player.MyGuild.RankOrchidsDonations.OrderByDescending(e => e.Orchids).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));


                        stream.GuildRanksCreate(Rank, (ushort)count, Page);
                        int RealRank = Page * 10;

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.Orchids, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.TulipRank:
                    {
                        var array = user.Player.MyGuild.RankTulipsDonations.OrderByDescending(e => e.Tulips).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));

                        int RealRank = Page * 10;

                        stream.GuildRanksCreate(Rank, (ushort)count, Page);

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.Tulips, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
                case RankTyp.TotalDonaion:
                    {
                        var array = user.Player.MyGuild.RankTotalDonations.OrderByDescending(e => e.TotalDonation).ToArray();

                        const int max = 10;
                        int offset = Page * max;
                        int count = Math.Min(max, Math.Max(0, array.Length - offset));

                        int RealRank = Page * 10;

                        stream.GuildRanksCreate(Rank, (ushort)count, Page);

                        for (int x = 0; x < count; x++)
                        {
                            if (array.Length > x + offset)
                            {
                                var element = array[x + offset];
                                stream.AddItemGuildRanks(Rank, element, element.TotalDonation, RealRank++);
                            }
                        }
                        user.Send(stream.GuildRanksFinalize());
                        break;
                    }
            }

        }

    }
}
