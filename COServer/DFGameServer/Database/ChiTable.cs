using Core;
using Core.Models.GameServer;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class ChiTable
    {
        public static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper ini = new();
                foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                {
                    ini.LoadFile(fname);
                    uint UID = ini.ReadUInt32("Character", "UID", 0);
                    Role.Instance.Chi playerchi = new Role.Instance.Chi(UID);
                    string Name = ini.ReadString("Character", "Name", "None");
                    playerchi.Name = Name;
                    playerchi.ChiPoints = ini.ReadInt32("Character", "ChiPoints", 0);
                    playerchi.Dragon.Load(ini.ReadString("Character", "Dragon", ""), UID, Name);
                    playerchi.Phoenix.Load(ini.ReadString("Character", "Pheonix", ""), UID, Name);
                    playerchi.Turtle.Load(ini.ReadString("Character", "Turtle", ""), UID, Name);
                    playerchi.Tiger.Load(ini.ReadString("Character", "Tiger", ""), UID, Name);
                    if (playerchi.Dragon.UnLocked)
                    {
                        Role.Instance.Chi.ChiPool.TryAdd(playerchi.UID, playerchi);
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Dragon, playerchi.Dragon);
                    }
                    if (playerchi.Phoenix.UnLocked)
                    {
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Phoenix, playerchi.Phoenix);
                    }
                    if (playerchi.Tiger.UnLocked)
                    {
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Tiger, playerchi.Tiger);
                    }
                    if (playerchi.Turtle.UnLocked)
                    {
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Turtle, playerchi.Turtle);
                    }
                }
            } else
            {
                List<Player> players = RestApiHelper.GetPlayers();
                foreach(Player player in players)
                {
                    Role.Instance.Chi playerchi = new Role.Instance.Chi(player.UID);
                    playerchi.Name = player.Name;
                    playerchi.ChiPoints = (int)player.ChiPoints;
                    playerchi.Dragon.Load(player.ChiDragon, player.UID, player.Name);
                    playerchi.Phoenix.Load(player.ChiPhoenix, player.UID, player.Name);
                    playerchi.Turtle.Load(player.ChiTurtle, player.UID, player.Name);
                    playerchi.Tiger.Load(player.ChiTiger, player.UID, player.Name);
                    if (playerchi.Dragon.UnLocked)
                    {
                        Role.Instance.Chi.ChiPool.TryAdd(playerchi.UID, playerchi);
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Dragon, playerchi.Dragon);
                    }
                    if (playerchi.Phoenix.UnLocked)
                    {
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Phoenix, playerchi.Phoenix);
                    }
                    if (playerchi.Tiger.UnLocked)
                    {
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Tiger, playerchi.Tiger);
                    }
                    if (playerchi.Turtle.UnLocked)
                    {
                        Pool.ChiRanking.Upadte(Pool.ChiRanking.Turtle, playerchi.Turtle);
                    }
                }
            }
        }
    }
}
