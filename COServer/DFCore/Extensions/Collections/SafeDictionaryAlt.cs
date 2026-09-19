using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core
{
    public class SafeDictionaryAlt<T1, T2> : Dictionary<T1, T2>
    {
        public object SyncRoot = (object)null;
        public bool Update = false;
        public T2[] MyArray = new T2[0];

        public SafeDictionaryAlt()
        {
            this.MyArray = new T2[0];
            this.SyncRoot = new object();
        }

        public SafeDictionaryAlt(int cap)
          : base(cap)
        {
            this.SyncRoot = new object();
        }

        public new T2 this[T1 key]
        {
            get
            {
                if (this.ContainsKey(key))
                    return base[key];
                return default(T2);
            }
            set
            {
                base[key] = value;
            }
        }

        public new void Add(T1 key, T2 value)
        {
            try
            {
                Monitor.Enter(this.SyncRoot);
                base[key] = value;
                this.Update = true;
            }
            finally
            {
                Monitor.Exit(this.SyncRoot);
            }
        }

        public new void Remove(T1 key)
        {
            try
            {
                Monitor.Enter(this.SyncRoot);
                base.Remove(key);
                this.Update = true;
            }
            finally
            {
                Monitor.Exit(this.SyncRoot);
            }
        }

        public T2[] GetValues()
        {
            if (!this.Update)
                return this.MyArray;
            try
            {
                Monitor.Enter(this.SyncRoot);
                this.Update = false;
                this.MyArray = this.Values.ToArray<T2>();
            }
            finally
            {
                Monitor.Exit(this.SyncRoot);
            }
            return this.MyArray;
        }

        public new void Clear()
        {
            lock (this.SyncRoot)
                base.Clear();
            this.Update = true;
        }
    }
}
