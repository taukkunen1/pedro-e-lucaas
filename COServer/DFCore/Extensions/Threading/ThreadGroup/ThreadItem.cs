using System;
using System.Threading;

namespace Core
{
    public class ThreadItem : ThreadBase
    {
        private const uint INFINITE = 4294967295;
        private const uint WAIT_ABANDONED = 128;
        private const uint WAIT_OBJECT_0 = 0;
        private const uint WAIT_TIMEOUT = 258;
        private int Max_ProcessInterval;
        private Mutex ThreadMutex;
        private Action Event;
        public Action<int> EventSleep;

        public ThreadItem(int interval, string MutexName, Action _Event, Action<int> Sleep = null)
        {
            this.Max_ProcessInterval = interval;
            this.ThreadMutex = new Mutex(false, MutexName);
            this.Event = _Event;
            this.EventSleep = Sleep;
        }

        protected override void OnInit()
        {
        }

        protected override bool OnProcess()
        {
            try
            {
                DateTime now = DateTime.Now;
                try
                {
                    if (this.Event != null)
                        this.Event();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    int millisecondsTimeout = this.Max_ProcessInterval - (DateTime.Now.AllMilliseconds() - now.AllMilliseconds());
                    if (millisecondsTimeout >= 0 && millisecondsTimeout <= this.Max_ProcessInterval)
                    {
                        if (this.EventSleep != null)
                            this.EventSleep(millisecondsTimeout);
                        else
                            Thread.Sleep(millisecondsTimeout);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return true;
        }
    }
}
