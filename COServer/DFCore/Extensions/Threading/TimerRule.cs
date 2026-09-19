namespace Core
{
    using System;
    using System.Threading;

    public class TimerRule
    {
        public Action<int> _action;
        public bool _Active;
        public int _period;
        public ThreadPriority _ThreadPriority;

        public TimerRule(Action<int> action, int period, ThreadPriority priority = ThreadPriority.Normal)
        {
            this._action = action;
            this._period = period;
            this._Active = true;
            this._ThreadPriority = priority;
        }

        ~TimerRule()
        {
            this._action = null;
        }
    }
}

