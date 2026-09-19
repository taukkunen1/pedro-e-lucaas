using System;

namespace Core
{
    public class SafeRandom
    {
        private Random Rand;
        public object SyncRoot;

        public SafeRandom(int seed = 0)
        {
            this.SyncRoot = new object();
            if (seed != 0)
                this.Rand = new Random(seed);
            else
                this.Rand = new Random();
        }

        public int Next(int minval, int maxval)
        {
            lock (this.SyncRoot)
                return this.Rand.Next(minval, maxval);
        }

        public int Next(int maxval)
        {
            lock (this.SyncRoot)
                return this.Rand.Next(maxval);
        }

        public int Next()
        {
            lock (this.SyncRoot)
                return this.Rand.Next();
        }

        public void SetSeed(int seed)
        {
            lock (this.SyncRoot)
                this.Rand = new Random();
        }
    }
}
