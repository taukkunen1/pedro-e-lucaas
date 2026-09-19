using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetAdvertiseGui(this ServerSockets.Packet stream, out uint GuildID, out string bulletin, out long Donation, out ushort AutoJoin
            , out ushort Level, out ushort Reborn, out ushort Flag, out byte Grade)
        {
            GuildID = stream.ReadUInt32();
            bulletin = stream.ReadCString(255);
            sbyte unknow = stream.ReadInt8();
            Donation = stream.ReadInt64();
            AutoJoin = stream.ReadUInt16();
            Level = stream.ReadUInt16();
            Reborn = stream.ReadUInt16();
            Flag = stream.ReadUInt16();
            Grade = stream.ReadUInt8();
        }

        public static unsafe ServerSockets.Packet AdvertiseGuiCreate(this ServerSockets.Packet stream, uint GuildID = 0, string bulletin = "", long Donation = 0, ushort AutoJoin = 0
            , ushort Level = 0, ushort Reborn = 0, ushort Flag = 0, byte Grade = 0)
        {
            stream.InitWriter();

            stream.Write(GuildID);
            stream.Write(bulletin, 255);
            stream.Write((byte)0);
            stream.Write(Donation);
            stream.Write(AutoJoin);
            stream.Write(Level);
            stream.Write(Reborn);
            stream.Write(Flag);
            stream.Write(Grade);

            stream.Finalize(GamePackets.AdvertiseGui);

            return stream;
        }
    }
    public unsafe class MsgAdvertiseGui
    {
        [PacketAttribute(GamePackets.AdvertiseGui)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {

            uint GuildID;
            string bulletin;
            long Donation;
            ushort AutoJoin;
            ushort Level;
            ushort Reborn;
            ushort Flag;
            byte Grade;
            stream.GetAdvertiseGui(out GuildID, out bulletin, out Donation, out AutoJoin, out Level, out Reborn, out Flag, out Grade);

            if (user.Player.MyGuild == null) return;
            if (user.Player.MyGuildMember == null) return;
            if (user.Player.GuildRank != Role.Flags.GuildMemberRank.GuildLeader) return;

            if (bulletin.Contains('/'))
            {
                user.SendSysMesage("You buletin contain invalid char (/)! Please change this, to use my services");              
                return;
            }
            if (user.Player.MyGuild.Info.SilverFund >= Donation)
            {
                user.Player.MyGuild.Info.SilverFund -= Donation;
                user.Player.MyGuild.AdvertiseRecruit.Buletin = bulletin;
                user.Player.MyGuild.AdvertiseRecruit.AutoJoin = AutoJoin == 1;
                user.Player.MyGuild.AdvertiseRecruit.Level = (byte)Level;
                user.Player.MyGuild.AdvertiseRecruit.Reborn = (byte)Reborn;
                user.Player.MyGuild.AdvertiseRecruit.SetFlag((int)Flag, Role.Instance.Guild.Recruitment.Mode.Recruit);
                user.Player.MyGuild.AdvertiseRecruit.Donations += Donation;

                Role.Instance.Guild.Advertise.Add(user.Player.MyGuild);
            }
            else
            {
                user.SendSysMesage("You guild use a small donation!");              
            }
        }

    }
}
