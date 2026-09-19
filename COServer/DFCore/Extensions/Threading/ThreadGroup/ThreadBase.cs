using System.Threading;

namespace Core
{
    public abstract class ThreadBase
    {
        private bool Alive = false;
        private Thread _thread;

        public ThreadBase()
        {
            this._thread = new Thread(new ThreadStart(this.ThreadProc));
            this._thread.Priority = ThreadPriority.Highest;
        }

        public void Open()
        {
            if (this.Alive)
                return;
            this.Alive = true;
            this._thread.Start();
        }

        protected abstract void OnInit();

        protected abstract bool OnProcess();

        public void ThreadProc()
        {
            this.OnInit();
            while (this.Alive)
            {
                try
                {
                    if (!this.OnProcess())
                    {
                        this.Close();
                        break;
                    }
                }
                catch
                {
                }
            }
        }

        public void Close()
        {
            if (!this.Alive)
                return;
            this.Alive = false;
            this._thread.Interrupt();
        }
    }
}
