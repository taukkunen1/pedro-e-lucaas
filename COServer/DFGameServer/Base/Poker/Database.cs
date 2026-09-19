using GameServer;
using Poker.Structures;
using System;
using System.Collections.Generic;
using System.IO;
namespace Poker
{
    public class Database
    {
        public static Dictionary<uint, PokerTable> Tables;
        public static void Load()
        {
            Tables = new Dictionary<uint, PokerTable>();
            var lines = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "PokerTables.ini"));
            foreach (var vLine in lines)
            {
                var line = vLine.Split(' ');
                var table = new PokerTable(Convert.ToUInt32(line[0]));
                table.X = Convert.ToUInt16(line[1]);
                table.Y = Convert.ToUInt16(line[2]);
                table.Mesh = Convert.ToUInt32(line[3]);
                table.Number = Convert.ToUInt32(line[4]);
                int I = Convert.ToByte(line[5]);
                if (I == 1)
                    table.UnLimited = true;
                I = Convert.ToByte(line[6]);
                if (I == 1)
                {
                    table.IsCPs = true;
                    table.MapId = 1860;
                }
                table.MinBet = Convert.ToUInt32(line[7]);
                table.TableType = (Enums.TableType)Convert.ToUInt32(line[8]);
                Tables.Add(table.Id, table);
            }

            if (Tables != null)
            {
                GameServer.Console.WriteLine("Loaded " + Tables.Count + " Poker Tables");
            }
        }
    }
}