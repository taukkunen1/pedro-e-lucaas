using System;
using System.Threading;

namespace TrinityConquerLoader
{
    public class ProcessFactory
    {
        private const int REST = 2;

        private Action[] actions;
        private Thread[] threads;
        private int pos;

        public ProcessFactory(int workers)
        {
            actions = new Action[workers];
            threads = new Thread[workers];
            for (int i = 0; i < workers; i++)
                threads[i] = new Thread(Process);
        }
        public void Process(object operationObj)
        {
            var idx = (int)operationObj;
            for (; ; )
            {
                if (actions[idx] != null)
                    actions[idx]();
                Thread.Sleep(REST);
            }
        }
        public void Start()
        {
            for (int i = 0; i < threads.Length; i++)
                threads[i].Start(i);
        }
        public void AddProcess(Action process)
        {
            actions[pos] += process;
            pos = (pos + 1) % actions.Length;
        }
    }
}
