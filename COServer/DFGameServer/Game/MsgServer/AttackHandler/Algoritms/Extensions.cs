using GameServer.Game.MsgServer.AttackHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class Extensions23
    {
        public static void Add(this List<coords> Coords, int x, int y)
        {
            coords add = new coords((ushort)x, (ushort)y);
            if (!Coords.Contains(add))
                Coords.Add(add);
        }
        public static bool Contains(this List<coords> Coords, ushort X, ushort Y)
        {
            if (Coords.Where(e => e.X == X && e.Y == Y).Count() > 0)
                return true;
            return false;
        }
    }
}
