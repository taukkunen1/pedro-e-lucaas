using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetVipHandler(this ServerSockets.Packet stream, out MsgVipHandler.VIPTeleportTypes TeleportType, out MsgVipHandler.VIPTeleportLocations Locations, out uint Countdown
            , out string name)
        {
            TeleportType = (MsgVipHandler.VIPTeleportTypes)stream.ReadUInt32();
            Locations = (MsgVipHandler.VIPTeleportLocations)stream.ReadUInt32();
            Countdown = stream.ReadUInt32();

            byte size = stream.ReadUInt8();
            name = stream.ReadCString(size);
        }
        public static unsafe ServerSockets.Packet VipHandlerCreate(this ServerSockets.Packet stream, MsgVipHandler.VIPTeleportTypes TeleportType, MsgVipHandler.VIPTeleportLocations Locations, uint Countdown
         , string name)
        {

            stream.InitWriter();

            stream.Write((uint)TeleportType);
            stream.Write((uint)Locations);
            stream.Write(Countdown);

            stream.Write(name.Length);
            stream.Write(name, name.Length);

            stream.Finalize(Game.GamePackets.MsgVipHandler);

            return stream;
        }
    }
    public unsafe struct MsgVipHandler
    {
        public enum VIPTeleportTypes : uint
        {
            SelfTeleport = 0,
            TeamTeleport = 1,
            TeammateConfirmation = 2,
            TeammateTeleport = 3
        }
        public enum VIPTeleportLocations : uint
        {
            TwinCity = 1,
            PhoenixCastle = 2,
            ApeCity = 3,
            DesertCity = 4,
            BirdIland = 5,
            TCSquare = 6,
            WPFarm = 7,
            WPBridge = 8,
            WPAltar = 9,
            WPApparation = 10,
            WPPoltergiest = 11,
            WPTurtledove = 12,
            PCSqaure = 13,
            MFWaterCave = 14,
            MFVillage = 15,
            MFLake = 16,
            MFMineCave = 17,
            MFBridge = 18,
            MFToApeCity = 19,
            ACSquare = 20,
            ACSouth = 21,
            ACEast = 22,
            ACNorth = 23,
            ACWest = 24,
            BISquare = 25,
            BICenter = 26,
            BISouthWest = 27,
            BINorthWest = 28,
            BINorthEast = 29,
            DCSquare = 30,
            DCSouth = 31,
            DCVillage = 32,
            DCMoonSpring = 33,
            DCAncientMaze = 34
        }

        [PacketAttribute(GamePackets.MsgVipHandler)]
        public unsafe static void Handler(Client.GameClient user, ServerSockets.Packet packet)
        {

            VIPTeleportTypes TeleportType;
            VIPTeleportLocations Locations;
            uint Countdown;
            string name;
            if (user.PokerPlayer != null)
                return;

            packet.GetVipHandler(out TeleportType, out Locations, out Countdown, out name);


            switch (TeleportType)
            {
                case VIPTeleportTypes.SelfTeleport:
                    {
                        if (!user.Player.Alive)
                            return;
                        if (DateTime.Now > user.LastVIPTeleport.AddSeconds(30))
                        {
                            Teleport(user, Locations);
                            user.LastVIPTeleport = DateTime.Now;
                        }
                        else
                        {
                            user.SendSysMesage("You have to wait " + (user.LastVIPTeleport.AddSeconds(30) - DateTime.Now).TotalSeconds.ToString() + " more seconds to use the VIP Teleport.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                        }
                        break;
                    }
                case VIPTeleportTypes.TeamTeleport:
                    {

                        if (DateTime.Now > user.LastVIPTeamTeleport.AddSeconds(30))
                        {
                            if (user.Team != null)
                            {
                                packet = packet.VipHandlerCreate(VIPTeleportTypes.TeammateConfirmation, Locations, 15, user.Player.Name);

                                foreach (var member in user.Team.Temates)
                                {
                                    if (member.client.Player.UID != user.Player.UID)
                                    {
                                        if (member.client.Player.Alive)
                                            member.client.Send(packet);
                                    }
                                }
                            }
                            if (user.Player.Alive)
                                Teleport(user, Locations);
                            user.LastVIPTeamTeleport = DateTime.Now;
                        }
                        else
                        {
                            user.SendSysMesage("You have to wait " + (user.LastVIPTeamTeleport.AddSeconds(30) - DateTime.Now).TotalSeconds.ToString() + " more seconds to use the VIP Teleport.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                        }
                        break;
                    }
                case VIPTeleportTypes.TeammateTeleport:
                    {
                        Teleport(user, Locations);
                        break;
                    }
            }
        }

        public static void Teleport(Client.GameClient user, VIPTeleportLocations Location)
        {
            switch (Location)
            {
                case VIPTeleportLocations.TwinCity:
                case VIPTeleportLocations.TCSquare: user.Teleport(428, 379, 1002); break;
                case VIPTeleportLocations.WPFarm: user.Teleport(506, 262, 1002); break;
                case VIPTeleportLocations.WPBridge: user.Teleport(638, 679, 1002); break;
                case VIPTeleportLocations.WPAltar: user.Teleport(556, 955, 1002); break;
                case VIPTeleportLocations.WPApparation: user.Teleport(468, 750, 1002); break;
                case VIPTeleportLocations.WPPoltergiest: user.Teleport(358, 488, 1002); break;
                case VIPTeleportLocations.WPTurtledove: user.Teleport(755, 574, 1002); break;

                case VIPTeleportLocations.PhoenixCastle:
                case VIPTeleportLocations.PCSqaure: user.Teleport(188, 264, 1011); break;
                case VIPTeleportLocations.MFWaterCave: user.Teleport(380, 31, 1011); break;
                case VIPTeleportLocations.MFVillage: user.Teleport(785, 472, 1011); break;
                case VIPTeleportLocations.MFLake: user.Teleport(369, 568, 1011); break;
                case VIPTeleportLocations.MFMineCave: user.Teleport(924, 560, 1011); break;
                case VIPTeleportLocations.MFBridge: user.Teleport(648, 567, 1011); break;
                case VIPTeleportLocations.MFToApeCity: user.Teleport(475, 841, 1011); break;

                case VIPTeleportLocations.ApeCity:
                case VIPTeleportLocations.ACSquare: user.Teleport(565, 562, 1020); break;
                case VIPTeleportLocations.ACSouth: user.Teleport(699, 640, 1020); break;
                case VIPTeleportLocations.ACEast: user.Teleport(624, 337, 1020); break;
                case VIPTeleportLocations.ACNorth: user.Teleport(200, 224, 1020); break;
                case VIPTeleportLocations.ACWest: user.Teleport(322, 621, 1020); break;

                case VIPTeleportLocations.DesertCity:
                case VIPTeleportLocations.DCSquare: user.Teleport(500, 650, 1000); break;
                case VIPTeleportLocations.DCSouth: user.Teleport(758, 750, 1000); break;
                case VIPTeleportLocations.DCVillage: user.Teleport(480, 271, 1000); break;
                case VIPTeleportLocations.DCMoonSpring: user.Teleport(291, 450, 1000); break;
                case VIPTeleportLocations.DCAncientMaze: user.Teleport(87, 321, 1000); break;

                case VIPTeleportLocations.BirdIland:
                case VIPTeleportLocations.BISquare: user.Teleport(717, 571, 1015); break;
                case VIPTeleportLocations.BICenter: user.Teleport(585, 593, 1015); break;
                case VIPTeleportLocations.BISouthWest: user.Teleport(562, 786, 1015); break;
                case VIPTeleportLocations.BINorthWest: user.Teleport(125, 323, 1015); break;
                case VIPTeleportLocations.BINorthEast: user.Teleport(125, 323, 1015); break;
            }
        }


    }
}
