using System.Collections.Generic;
using System.Linq;
namespace GameServer.Game.MsgMonster
{
    public class MonsterView
    {
        public const int ViewThreshold = 18;
        private MonsterRole _Role;
        public MonsterView(MonsterRole _role)
        {
            _Role = _role;

        }
        /// <summary>
        /// make sure the collection
        /// if all its in screen ( -> ViewThreshold )
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Role.IMapObj> Roles(Role.GameMap map, Role.MapObjectType typ)
        {
            return map.View.Roles(typ, _Role.X, _Role.Y, p => CanSee(p));
        }
        public Role.IMapObj GetTarget(Role.GameMap map, Role.MapObjectType typ, uint oldtarget = 0)
        {
            var array = map.View.Roles(typ, _Role.X, _Role.Y, p => CanSee(p) && p.ObjType == Role.MapObjectType.Player && p.UID != oldtarget && p.Alive);
            if (array.Count() > 0)
                return array.OrderByDescending(p => p.IndexInScreen).FirstOrDefault();
            else
                return null;
        }
        public unsafe void SendScreen(ServerSockets.Packet msg, Role.GameMap map)
        {
            foreach (var obj in Roles(map, Role.MapObjectType.Player))
            {
                obj.Send(msg);
            }
        }
        public bool CanSee(Role.IMapObj obj)
        {
            if (obj.Map != _Role.Map)
                return false;
            if (obj.DynamicID != _Role.DynamicID)
                return false;
            if (obj.UID == _Role.UID)
                return false;
            return GetDistance(obj.X, obj.Y, _Role.X, _Role.Y) <= ViewThreshold;
        }

        public static short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            short x = 0;
            short y = 0;
            if (X >= X2) x = (short)(X - X2);
            else if (X2 >= X) x = (short)(X2 - X);
            if (Y >= Y2) y = (short)(Y - Y2);
            else if (Y2 >= Y) y = (short)(Y2 - Y);
            if (x > y) return x;
            else return y;
        }
    }
}
