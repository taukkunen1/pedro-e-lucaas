namespace Core
{
    public class Counter
    {
        uint Start = 0;
        uint finish = uint.MaxValue;

        public uint Finish
        {
            get
            {
                return finish;
            }
            set
            {
                finish = value;
            }
        }

        // Get current value
        public uint Now
        {
            get;
            set;
        }
        // The same as Now attribute
        public uint Count
        {
            get
            {
                return Now;
            }
        }

        public uint Next
        {
            get
            {
                Now++;
                if (Now == Finish)
                    Now = Start;
                return Now;
            }
        }
        public Counter()
        {
            Now = Start;
        }
        public Counter(uint startFrom)
        {
            Start = startFrom;
            Now = startFrom;
        }

        public void Set(uint start)
        {
            this.Now = start;
        }
    }
}
