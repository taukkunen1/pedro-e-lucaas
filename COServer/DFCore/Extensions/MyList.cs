using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Core
{
    public class MyList<T>
    {
        private T[] objects = new T[0];
        public List<T> m_List = new List<T>();
        public object SyncRoot = new object();
        public bool Update;

        public void Add(T obj)
        {
            lock (this.SyncRoot)
            {
                if (!this.m_List.Contains(obj))
                    this.m_List.Add(obj);
                this.Update = true;
            }
        }

        public T[] GetValues()
        {
            if (this.Update)
            {
                lock (this.SyncRoot)
                {
                    this.objects = this.m_List.ToArray();
                    this.Update = false;
                }
            }
            return this.objects;
        }

        public void Remove(T obj)
        {
            lock (this.SyncRoot)
            {
                this.m_List.Remove(obj);
                this.Update = true;
            }
        }

        public int Count
        {
            get
            {
                return this.GetValues().Length;
            }
        }

        public void Clear()
        {
            lock (this.SyncRoot)
            {
                this.m_List.Clear();
                this.Update = true;
            }
        }

        public T this[int key]
        {
            get
            {
                var values = this.GetValues();
                if (key < 0 || key >= values.Length)
                {
                    return default(T);
                }
                return values[key];
            }
        }
    }
}
