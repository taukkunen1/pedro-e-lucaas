using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class MyRandom
    {
        int ProbTime = 0;
        double before;
        Random rand;
        public MyRandom()
        {
            rand = new Random();
            before = 0;
        }
        public MyRandom(int seed)
        {
            rand = new Random(seed);
            before = 0;
        }

        int verify()
        {
            int next = rand.Next();
            if (before == next)
            {
                rand = new Random();
                ProbTime++;
            }
            return 0;
        }

        public int Next()
        {
            verify();
            before = rand.Next();
            return (int)before;
        }

        public int Next(int max)
        {
            lock (rand)
            {
                verify();
                before = rand.Next(max);
                return (int)before;
            }
        }

        public int Next(int min, int max)
        {
            lock (rand)
            {
                verify();
                before = rand.Next(min, max);
                return (int)before;
            }
        }

        public void NextBytes(byte[] buffer)
        {
            verify();
            rand.NextBytes(buffer);
        }

        public double NextDouble()
        {
            lock (rand)
            {
                verify();
                before = rand.NextDouble();
                return before;
            }
        }
    }
}