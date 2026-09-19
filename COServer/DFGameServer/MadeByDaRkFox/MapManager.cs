using GameServer.Client;
using GameServer.Game.MsgMonster;
using System;
using System.Linq;
using System.Threading;

namespace GameServer.MadeByDaRkFox
{
    public class MapManager
    {
        private readonly Role.GameMap _map;
        private readonly Timer _logicTimer;

        public MapManager(Role.GameMap map)
        {
            _map = map;
            _logicTimer = new Timer(Tick, null, 0, 500);
        }

        private void Tick(object? state)
        {
            ProcessMonsterAI();
        }

        private void ProcessMonsterAI()
        {
            foreach (var kvP in Pool.GamePoll)
            {
                try
                {
                    GameClient client = kvP.Value;

                    if (client == null)
                        continue;
                    if (client.Map == null)
                        continue;

                    DateTime timer = DateTime.Now;

                    // Get all the monsters but only for the specified map
                    var monsters = client.Player.View.Roles(Role.MapObjectType.Monster).Where(x => x.Map == _map.ID).ToList();

                    foreach (var map_mob in monsters)
                    {
                        var monster = (map_mob as MonsterRole);
                        if (monster == null)
                            continue;

                        if (!monster.Alive)
                        {
                            if (monster.State == Game.MsgMonster.MobStatus.Respawning)
                            {
                                if (monster.Family.ID == 8500)
                                    continue;
                                if (MonsterRole.SpecialMonsters.Contains(monster.Family.ID))
                                    continue;
                                if (timer > monster.RespawnStamp)
                                {
                                    if (!client.Map.MonsterOnTile(monster.RespawnX, monster.RespawnY))
                                    {
                                        monster.Respawn();
                                        client.Map.SetMonsterOnTile(monster.X, monster.Y, true);
                                    }
                                }
                            }
                        }

                        if ((monster.Family.Settings & MonsterSettings.Guard) != MonsterSettings.Guard
                            && (monster.Family.Settings & MonsterSettings.Reviver) != MonsterSettings.Reviver
                            && (monster.Family.Settings & MonsterSettings.Lottus) != MonsterSettings.Lottus)
                        {
                            if (monster.Family.ID == 20211)
                                continue;

                            client.Player.View.MobActions.ExecuteAction(client.Player.View.GetPlayer(), monster);

                            if (!monster.Alive)
                            {
                                var now = DateTime.Now;
                                monster.AddFadeAway(now.Ticks, client.Map);
                                monster.RemoveView(now.Ticks, client.Map);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                }
            }
        }
    }
}
