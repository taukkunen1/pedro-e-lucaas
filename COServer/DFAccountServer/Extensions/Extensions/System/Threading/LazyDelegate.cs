namespace System.Threading
{
    using System;

    public class LazyDelegate : TimerRule
    {
        public LazyDelegate(Action<int> action, int dueTime, ThreadPriority priority = (ThreadPriority)2)
            : base(action, dueTime, priority)
        {
            base._Active = false;
        }
    }
}

