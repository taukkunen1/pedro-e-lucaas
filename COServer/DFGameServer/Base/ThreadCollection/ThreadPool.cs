using Core;
using GameServer.Client;
using GameServer.MadeByDaRkFox;
using System;
using System.Threading;
using static GameServer.Threading.Basic;

namespace GameServer.ServerSockets
{
    public class ThreadPool
    {
        public TimerRule<SecuritySocket> ConnectionReceive;
        public StaticPool SendPool;
        public static StaticPool GenericThreadPool;
        public TimerRule<GameClient> MainTimer, TimerCheckingExpiredItems;
        public TimerRule<Bot.AI> BotMainTimer;
        public ThreadPool()
        {
            GenericThreadPool = new StaticPool().Run();
            ConnectionReceive = new TimerRule<SecuritySocket>(connectionReceive, 1, ThreadPriority.Highest);
            MainTimer = new TimerRule<GameClient>(MainCallBack, 250);
            TimerCheckingExpiredItems = new TimerRule<GameClient>(ItemExpireSystem.CheckItemsTime, 1000);
            BotMainTimer = new TimerRule<Bot.AI>(BotMainCallBack, 1000);
        }
        public static void BotMainCallBack(Bot.AI bot, int time)
        {
            if (DateTime.Now >= bot.StampJumbCallback.AddMilliseconds(2500))
            {
                Bot.Dynamic.Jumb_DoWork(bot);
                bot.StampJumbCallback = DateTime.Now;
            }
            if (DateTime.Now >= bot.StampHitCallback.AddMilliseconds(1250))
            {
                Bot.Dynamic.Hit_DoWork(bot);
                bot.StampHitCallback = DateTime.Now;
            }
        }
        public static void MainCallBack(GameClient client, int time)
        {
            DateTime clock = DateTime.Now;

            try
            {
                if (client.Player.Class >= 41 && client.Player.Class <= 45)
                {
                    if (DateTime.Now >= client.StampAutoAttackCallback.AddMilliseconds(700))
                    {
                        ThreadInvoke(new Action(client.AutoAttackCallback));
                        client.StampAutoAttackCallback = DateTime.Now;
                    }
                }
                else if (DateTime.Now >= client.StampAutoAttackCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.AutoAttackCallback));
                    client.StampAutoAttackCallback = DateTime.Now;
                }
                if (DateTime.Now >= client.StampPlayer_BuffersCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.BufferCallback));
                    client.StampPlayer_BuffersCallback = DateTime.Now;
                }
                if (DateTime.Now >= client.StampItemsCallBack.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.ItemsCallBack));
                    client.StampItemsCallBack = DateTime.Now;
                }
                if (DateTime.Now >= client.StampMiningCallBack.AddMilliseconds(2500))
                {
                    ThreadInvoke(new Action(client.MiningCallBack));
                    client.StampMiningCallBack = DateTime.Now;
                }
                if (DateTime.Now >= client.StampSecondsCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.SecondsCallback));
                    client.StampSecondsCallback = DateTime.Now;
                }
                if (DateTime.Now >= client.StampAliveMonstersCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.AliveMonstersCallback));
                    client.StampAliveMonstersCallback = DateTime.Now;
                }
                if (DateTime.Now >= client.StampMonster_BuffersCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.BuffersCallback));
                    client.StampMonster_BuffersCallback = DateTime.Now;
                }
                if (DateTime.Now >= client.StampGuardsCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.GuardsCallback));
                    client.StampGuardsCallback = DateTime.Now;
                }
                if (DateTime.Now >= client.StampReviversCallback.AddMilliseconds(1000))
                {
                    ThreadInvoke(new Action(client.ReviversCallback));
                    client.StampReviversCallback = DateTime.Now;
                }
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
        }
        private static DateTime StampWorldTournaments = DateTime.Now;
        private static DateTime StampServer = DateTime.Now;
        public static void ServerCallBack()
        {
            if (DateTime.Now >= StampServer.AddMinutes(1))
            {
                Threading.Server.Handle(0);
                StampServer = DateTime.Now;
            }
            if (DateTime.Now >= StampWorldTournaments.AddMilliseconds(1000))
            {
                Threading.WorldTournaments.Handle(0);
                StampWorldTournaments = DateTime.Now;
            }
            foreach (var t in Poker.Database.Tables.Values)
                PokerHandler.PokerTablesCallback(t, 0);
            Threading.Qualifier.ArenaQualifier(0);
            Threading.Qualifier.TeamArenaQualifier(0);

        }
        public bool BotRegister(Bot.AI bot)
        {
            if (bot.BEntity.TimerSubscriptions == null)
            {
                bot.BEntity.TimerSubscriptions = new IDisposable[]
                {
                   Subscribe(BotMainTimer, bot)
                };
                return true;
            }
            return false;
        }
        public bool Register(GameClient client)
        {
            if (client.TimerSubscriptions == null)
            {
                client.TimerSubscriptions = new IDisposable[]
                {
                   Subscribe(MainTimer, client)
                };
                return true;
            }
            return false;
        }
        public static void Unregister(GameClient client)
        {
            lock (client.TimerSyncRoot)
            {
                if (client.TimerSubscriptions != null)
                {
                    foreach (var Now in client.TimerSubscriptions)
                        Now.Dispose();
                    client.TimerSubscriptions = null;
                }
            }
        }
        public static void connectionReceive(ServerSockets.SecuritySocket wrapper, int time)
        {
            if (wrapper.ReceiveBuffer())
            {
                wrapper.HandlerBuffer();
            }
        }
        #region Funcs
        //public static void Execute(Action<int> action, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        //{
        //    GenericThreadPool.Subscribe(new LazyDelegateAlt(action, timeOut, priority));
        //}
        //public static void Execute<T>(Action<T, int> action, T param, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        //{
        //    GenericThreadPool.Subscribe<T>(new LazyDelegate<T>(action, timeOut, priority), param);
        //}
        public static IDisposable Subscribe(Action<int> action, int period = 1, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe(new TimerRule(action, period, priority));
        }
        public static IDisposable Subscribe<T>(Action<T, int> action, T param, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe<T>(new TimerRule<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StandalonePool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StaticPool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param)
        {
            return GenericThreadPool.Subscribe<T>(rule, param);
        }
        #endregion
    }
}
