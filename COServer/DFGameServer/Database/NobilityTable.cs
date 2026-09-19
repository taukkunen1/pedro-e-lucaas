using Core;
using Core.Models.GameServer;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class NobilityTable
    {
        public static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper ini = new();
                foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                {
                    ini.LoadFile(fname);
                    ushort Body = ini.ReadUInt16("Character", "Body", 1002);
                    ushort Face = ini.ReadUInt16("Character", "Face", 0);
                    uint UID = ini.ReadUInt32("Character", "UID", 0);
                    string Name = ini.ReadString("Character", "Name", "None");
                    byte Gender = 0;
                    if ((byte)(Body % 10) >= 3)
                        Gender = 0;
                    else
                        Gender = 1;
                    uint Mesh = (uint)(Face * 10000 + Body);
                    ulong donation = ini.ReadUInt64("Character", "DonationNobility", 0);
                    Role.Instance.Nobility nobility = new Role.Instance.Nobility(UID, Name, donation, Mesh, Gender);
                    Pool.NobilityRanking.UpdateRank(nobility);
                }
            } else
            {
                List<Player> players = RestApiHelper.GetPlayers();
                foreach (Player player in players)
                {
                    ushort Body = player.Body;
                    ushort Face = player.Face;
                    uint UID = player.UID;
                    string Name = player.Name;
                    byte Gender = 0;
                    if ((byte)(Body % 10) >= 3)
                    {
                        Gender = 0;
                    }
                    else
                    {
                        Gender = 1;
                    }
                    uint Mesh = (uint)(Face * 10000 + Body);
                    ulong donation = player.NobilityDonation;
                    Role.Instance.Nobility nobility = new Role.Instance.Nobility(UID, Name, donation, Mesh, Gender);
                    Pool.NobilityRanking.UpdateRank(nobility);
                }
            }
        }

        public static void Reset()
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper ini = new();
                foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                {
                    ini.LoadFile(fname);
                    ini.Write("Character", "DonationNobility", 0);
                }
            }
            else
            {
                List<Player> players = RestApiHelper.GetPlayers();
                foreach (Player player in players)
                {
                    player.NobilityDonation = 0;
                }
                RestApiHelper.UpdatePlayers(players);
            }
            Load();
        }
    }
}
