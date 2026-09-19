namespace Core
{
    public class BitVector32
    {
        public uint[] bits;

        public int Size
        {
            get
            {
                return 32 * this.bits.Length;
            }
        }

        public BitVector32(int BitCount)
        {
            int length = BitCount / 32;
            if (BitCount % 32 != 0)
                ++length;
            this.bits = new uint[length];
        }

        public void Add(int index)
        {
            if (index >= this.Size)
                return;
            this.bits[index / 32] |= (uint)(1 << index % 32);
        }

        public void Remove(int index)
        {
            if (index >= this.Size)
                return;
            this.bits[index / 32] &= ~(uint)(1 << index % 32);
        }

        public bool Contain(int index)
        {
            if (index > this.Size)
                return false;
            int index1 = index / 32;
            uint num = (uint)(1 << index % 32);
            return ((int)this.bits[index1] & (int)num) == (int)num;
        }

        public int Count()
        {
            int num = 0;
            for (int index1 = 0; index1 < this.Size / 32; ++index1)
            {
                for (int index2 = 0; index2 < 32; ++index2)
                {
                    if (((long)this.bits[index1] & (long)(1 << index2)) == (long)(1 << index2))
                        ++num;
                }
            }
            return num;
        }

        public void Clear()
        {
            ushort num = (ushort)(byte)(this.Size / 32);
            for (byte index = 0; (int)index < (int)num; ++index)
                this.bits[(int)index] = 0U;
        }
    }
}
