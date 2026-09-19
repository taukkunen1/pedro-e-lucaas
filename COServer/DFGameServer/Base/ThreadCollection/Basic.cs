using System;
using System.Threading;

namespace GameServer.Threading
{
    public delegate void Execute();
    public class Basic
    {
        public Basic(Action action, int dueTime)
        {
            Execute += new Threading.Execute(action);
            Start(dueTime);
        }
        public event Execute Execute;
        private Timer T;
        int interval;
        public static void ThreadInvoke(Action obj)
        {
            try
            {
                obj.Invoke();
            }
            catch (Exception e) { Console.SaveException(e); }
        }
        public void Start(int Interval, int dueTime = 2000)
        {
            interval = Interval;
            T = new Timer(Run, this, dueTime, Interval);
        }
        bool closed = false;
        public void Close()
        {
            if (!closed)
            {
                closed = true;
                T.Dispose();
                T = null;
                Execute = null;
            }
        }
        void Run(object ob)
        {
            if (closed)
            {
                return;
            }
            try
            {
                if (Execute != null)
                    Execute.Invoke();
                else
                    Console.WriteLine("Execute null!");
            }
            catch (Exception E) { Console.WriteLine(E.ToString()); }
        }
    }
}
