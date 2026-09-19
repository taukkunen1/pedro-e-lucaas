namespace Core
{
    using System;
    using System.Threading;

    public class TimerRule<T>
    {
        public Action<T, int> _Action;
        public bool _Active;
        public int _period;
        public ThreadPriority _ThreadPriority;

        public TimerRule(Action<T, int> action, int period, ThreadPriority priority = ThreadPriority.Normal)
        {
            this._Action = action;
            this._period = period;
            this._Active = true;
            this._ThreadPriority = priority;
        }

        ~TimerRule()
        {
            this._Action = null;
        }
    }
}

