using Core;
using System;

namespace GameServer.Game.MsgMonster
{
    public class MobCollection
    {
        public const byte Multiple = 3;

        public string LocationSpawn = "";

        public object SyncRoot = new object();
        public static Counter GenerateUid = new Counter(400000);
        public Role.GameMap DMap = null;

        private uint DmapID = 0;
        public MobCollection(uint Map)
        {
            DmapID = Map;
            if (Pool.ServerMaps != null)
            {
                if (Pool.ServerMaps.TryGetValue(Map, out DMap))
                    DMap.MonstersCollection = this;
            }
        }
        public bool ReadMap()
        {
            if (Pool.ServerMaps != null)
            {
                var mapId = DmapID;
                if (Pool.ServerMaps.TryGetValue(mapId, out DMap))
                    DMap.MonstersCollection = this;
            }

            return DMap != null;
        }
        public MonsterRole Add(MonsterFamily Famili, bool RemoveOnDead = false, uint dinamicid = 0, bool justone = false)
        {
            if (DMap == null)
                ReadMap();
            MonsterRole monsterr = null;
            int count = (int)((Famili.Boss > 0) ? 1 : (int)(Math.Max(1, (int)Famili.SpawnCount) * Multiple));
            if (justone)
                count = Math.Max(1, (int)Famili.SpawnCount);
            for (int x = 0; x < count; x++)
            {
                MonsterRole Mob = new MonsterRole(Famili.Copy(), GenerateUid.Next, LocationSpawn, DMap);
                Mob.RemoveOnDead = RemoveOnDead;
                TryObtainSpawnXY(Famili, out ushort _x, out ushort _y);
                if (!DMap.ValidLocation(_x, _y) || (DMap.MonsterOnTile(_x, _y) && Mob.Boss == 0))
                    continue;
                DMap.SetMonsterOnTile(_x, _y, true);
                Mob.X = _x;
                Mob.Y = _y;
                Mob.RespawnX = _x;
                Mob.RespawnY = _y;
                Mob.Map = DMap.ID;
                Mob.DynamicID = dinamicid;
                monsterr = Mob;
                DMap.View.EnterMap<MonsterRole>(Mob);
            }
            return monsterr;
        }

        /// <summary>
        /// Attemps to obtain a point where the monster can be re-spawned.
        /// </summary>
        /// <param name="X">The x-coordinate point.</param>
        /// <param name="Y">The y-coordinate point.</param>
        public void TryObtainSpawnXY(MonsterFamily Monster, out ushort X, out ushort Y)
        {

            X = (ushort)Pool.GetRandom.Next(Monster.SpawnX, Monster.MaxSpawnX);
            Y = (ushort)Pool.GetRandom.Next(Monster.SpawnY, Monster.MaxSpawnY);

            for (byte i = 0; i < 10; i++)
            {
                if (DMap == null)
                    break;
                if (DMap.ValidLocation(X, Y) && !DMap.MonsterOnTile(X, Y))
                    break;

                X = (ushort)Pool.GetRandom.Next(Monster.SpawnX, Monster.MaxSpawnX);
                Y = (ushort)Pool.GetRandom.Next(Monster.SpawnY, Monster.MaxSpawnY);
            }
        }

        public static void TryObtainSpawnXY(Role.GameMap DMap, 
            ushort x, ushort y, 
            ushort x_max, ushort y_max, 
            out ushort X, out ushort Y)
        {
            ushort x2 = (ushort)(x + x_max);
            ushort y2 = (ushort)(y + y_max);

            X = (ushort)Pool.GetRandom.Next(x, x2);
            Y = (ushort)Pool.GetRandom.Next(y, y2);

            for (byte i = 0; i < 10; i++)
            {
                if (DMap == null)
                    break;
                if (DMap.ValidLocation(X, Y) && !DMap.MonsterOnTile(X, Y))
                    break;

                X = (ushort)Pool.GetRandom.Next(x, x2);
                Y = (ushort)Pool.GetRandom.Next(y, y2);
            }
        }
    }
}
