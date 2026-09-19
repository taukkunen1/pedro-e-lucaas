using GameServer.Game.MsgNpc;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.MadeByDaRkFox
{
    public static class SmartNPCManager
    {
        private static List<NpcAttribute> GetAllNpcAttributes()
        {
            return Procesor.invoker.GetAll().Values.Where(x => x.Item1 != null && x.Item1.MapId > 0).Select(tuple => tuple.Item1).ToList();
        }

        public static void Load()
        {
            uint Count = 0;
            List<NpcAttribute> npcs = GetAllNpcAttributes();
            foreach (var npcAttr in npcs)
            {
                Npc gameNpc = Npc.Create();
                gameNpc.UID = (uint)npcAttr.Type;
                gameNpc.NpcType = npcAttr.ActionType;
                gameNpc.Mesh = npcAttr.Mesh;
                gameNpc.Map = npcAttr.MapId;
                gameNpc.X = npcAttr.MapX;
                gameNpc.Y = npcAttr.MapY;
                gameNpc.Name = npcAttr.Name;
                if (npcAttr.Name.Length > 0)
                {
                    gameNpc.Name = npcAttr.Name;
                }
                if (Pool.ServerMaps.ContainsKey(gameNpc.Map))
                {
                    Count++;
                    Pool.ServerMaps[gameNpc.Map].AddNpc(gameNpc);
                }
            }
            Console.WriteLine("[SmartNPCManager] Loading " + Count + " Npcs", System.ConsoleColor.DarkGreen);
        }
    }
}
