using GameServer.Client;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.MsgServer
{
    public class SurpriseBox
    {
        static List<uint> High = new List<uint>()
        {
            720611,723860,720027,723094,1200000,723727,
            181955, 182635, 191305,200005,754999,753999,753003,753001,751999,751001,751003
        };
        static List<uint> Mid = new List<uint>()
        {
            720049,723342,723342,753999,753099,754999,720027,723094,1200000,1200001,1200002,1100009,1100006,200312,200311,200310,
            181955, 182635, 191305,200005,754999,753999,753003,753001,751999,751001,751003
        };
        static uint GenerateSoulsItems()
        {
            var level = (ushort)Role.Core.Random.Next(3, 5);
            if (Database.ItemType.PurificationItems.ContainsKey(level))
            {
                var array = Database.ItemType.PurificationItems[level].Values.ToArray();
                int position = Pool.GetRandom.Next(0, array.Length);
                return array[position].ID;
            }

            return 0;
        }
        public static void GetReward(GameClient client, ServerSockets.Packet stream)
        {
            if (Role.MyMath.Success(0.5))
            {
                var reward = GenerateSoulsItems();
                client.Inventory.Add(stream, reward);
                client.SendSysMesage("You got a nice reward check your inventory");
            }
            else if (Role.MyMath.Success(10))
            {
                var reward = Mid[Role.Core.Random.Next(0, Mid.Count)];
                client.Inventory.Add(stream, reward, 1);
                client.SendSysMesage("You got a nice reward check your inventory");
            }
            else
            {
                var reward = High[Role.Core.Random.Next(0, High.Count)];
                client.Inventory.Add(stream, reward, 1);
                client.SendSysMesage("You got a nice reward check your inventory");
            }
        }
    }
}
