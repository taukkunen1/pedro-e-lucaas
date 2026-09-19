namespace System.Threading.Generic
{
    using System;
    using System.Threading;

    public class LazyDelegate<T> : TimerRule<T>
    {
        public LazyDelegate(Action<T, int> action, int dueTime, ThreadPriority priority = (ThreadPriority)2)
            : base(action, dueTime, priority)
        {
            base._Active = false;
        }
    }
}

