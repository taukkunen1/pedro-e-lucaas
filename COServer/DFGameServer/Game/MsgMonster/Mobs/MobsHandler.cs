using System;
using static GameServer.Pool;

namespace GameServer
{
    public enum IDMonster : uint
    {
        None = 0,
        SnowBanshee = 20070,
        TeratoDragon = 20060,
        ThrillingSpook = 20160,
        DarkmoonDemon = 4145,
        CornDevil = 3737,
        DarkSpearman = 3739,
        MummySkeleton = 3738,
        Ganoderma = 3130
    }
    public static class MobsHandler
    {
        public static Mobs.Base CallUp(Game.MsgMonster.MonsterFamily famil, IDMonster iDMonster)
        {
            try
            {
                var type = Type.GetType("GameServer.Mobs.MobID_" + famil.ID);
                return Activator.CreateInstance(type, famil) as Mobs.Base;
            }
            catch
            {
                return null;
            }
        }
        public static void Generate(IDMonster iDMonster)
        {
            try
            {
                
                Game.MsgMonster.MonsterFamily famil;
                if (MonsterFamilies.TryGetValue((uint)iDMonster, out famil))
                {
                    var type = Type.GetType("GameServer.Mobs.MobID_" + famil.ID);
                    if (type != null)
                    {
                        var mob = (Activator.CreateInstance(type, famil) as Mobs.Base);
                        if (mob != null)
                            mob.Run();
                        else Console.WriteLine("Could not load combat script for Boss " + iDMonster);
                    } else
                    {
                        Console.WriteLine("Not found type mob: GameServer.Mobs.MobID_" + famil.ID);
                    }
                }
                else Console.WriteLine("Could not load combat script for Boss " + iDMonster);
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
        }
    }
}
