using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LruCache
{
    public class LruCache<TKey, TValue> : IDictionary<TKey, TValue>
    {
        // Dictionary that will hold the data to be cached
        private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

        // List of keys for maintaining the age of each item in the cache
        // Keys are ordered from oldest to newest
        private LinkedList<TKey> list = new LinkedList<TKey>();

        private object locker = new object();

        private int maxSize = 0;
        private int currentSize = 0;
        private int itemSize = 1;
        private int hitCount = 0;
        private int missCount = 0;
        private int discardedCount = 0;

        /// <summary>
        /// Initializes a new instance of LruCache<TKey, TValue>.
        /// </summary>
        /// <param name="maxSize">The maximum size of the cache.</param>
        /// <param name="itemSize">The size of each entry in the cache.</param>
        public LruCache(int maxSize, int itemSize)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maxSize cannot be <= 0");
            }

            if (itemSize < 0)
            {
                throw new ArgumentOutOfRangeException("itemSize cannot be < 0");
            }

            if (maxSize < itemSize)
            {
                throw new ArgumentOutOfRangeException("maxSize cannot be < itemSize");
            }

            this.maxSize = maxSize;
            this.itemSize = itemSize;
        }

        /// <summary>
        /// Initializes a new instance of LruCache<TKey, TValue>.
        /// </summary>
        /// <param name="maxSize">The maximum number of entries in the cache.</param>
        public LruCache(int maxSize) : this(maxSize, 1) {}

        public void Add(TKey key, TValue value)
        {
            lock (this.locker)
            {
                this.dict.Add(key, value);
                this.list.AddLast(key);

                this.currentSize += this.itemSize;

                TrimToSize(this.maxSize);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (this.locker)
            {
                this.Add(item.Key, item.Value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (this.locker)
            {
                if (this.ContainsKey(key))
                {
                    if (this.list.Remove(key) && this.dict.Remove(key))
                    {
                        this.currentSize -= this.itemSize;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (this.locker)
            {
                return this.Remove(item.Key);
            }
        }

        public void Clear()
        {
            lock (this.locker)
            {
                this.list.Clear();
                this.dict.Clear();

                this.currentSize = 0;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (this.locker)
                {
                    if (this.ContainsKey(key))
                    {
                        hitCount++;
                        RenewKey(key);
                        return this.dict[key];
                    }
                    else
                    {
                        missCount++;
                        return default(TValue);
                    }
                }
            }
            set
            {
                lock (this.locker)
                {
                    if (!this.ContainsKey(key))
                    {
                        throw new KeyNotFoundException();
                    }

                    this.dict[key] = value;
                    RenewKey(key);
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (this.locker)
            {
                if (this.dict.TryGetValue(key, out value))
                {
                    hitCount++;
                    RenewKey(key);
                    return true;
                }
                else
                {
                    missCount++;
                    return false;
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (this.locker)
            {
                return (this.list.Contains(key) && this.dict.ContainsKey(key));
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (this.locker)
            {
                return this.ContainsKey(item.Key);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (this.locker)
                {
                    return this.dict.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (this.locker)
                {
                    return this.dict.Values;
                }
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (this.locker)
            {
                IDictionary<TKey, TValue> dict = this.dict;
                dict.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (this.locker)
            {
                return this.dict.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.locker)
            {
                return this.dict.GetEnumerator();
            }
        }

        public TValue OldestValue
        {
            get
            {
                lock (this.locker)
                {
                    if (this.Count > 0)
                    {
                        return this.dict[this.list.First.Value];
                    }

                    return default(TValue);
                }
            }
        }

        public TKey OldestKey
        {
            get
            {
                lock (this.locker)
                {
                    if (this.Count > 0)
                    {
                        return this.list.First.Value;
                    }

                    return default(TKey);
                }
            }
        }

        public TValue NewestValue
        {
            get
            {
                lock (this.locker)
                {
                    if (this.Count > 0)
                    {
                        return this.dict[this.list.Last.Value];
                    }

                    return default(TValue);
                }
            }
        }

        public TKey NewestKey
        {
            get
            {
                lock (this.locker)
                {
                    if (this.Count > 0)
                    {
                        return this.list.Last.Value;
                    }

                    return default(TKey);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (this.locker)
                {
                    return this.dict.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int MaxSize
        {
            get
            {
                lock (this.locker)
                {
                    return this.maxSize;
                }
            }
        }

        public int CurrentSize
        {
            get
            {
                lock (this.locker)
                {
                    return this.currentSize;
                }
            }
        }

        public int ItemSize
        {
            get
            {
                lock (this.locker)
                {
                    return this.itemSize;
                }
            }
        }

        public int HitCount
        {
            get
            {
                lock (this.locker)
                {
                    return this.hitCount;
                }
            }
        }

        public int MissCount
        {
            get
            {
                lock (this.locker)
                {
                    return this.missCount;
                }
            }
        }

        public int DiscardedCount
        {
            get
            {
                lock (this.locker)
                {
                    return this.discardedCount;
                }
            }
        }

        public void Resize(int maxSize)
        {
            lock (this.locker)
            {
                if (maxSize <= 0)
                {
                    throw new ArgumentOutOfRangeException("maxSize cannot be <= 0");
                }

                if (maxSize < this.itemSize)
                {
                    throw new ArgumentOutOfRangeException("maxSize cannot be < ItemSize");
                }

                this.maxSize = maxSize;
                TrimToSize(this.maxSize);
            }
        }

        public override string ToString()
        {
            lock (this.locker)
            {
                return string.Format("LruCache<{0}, {1}> Max Size: {2} Current Size: {3} Item Size {4} Hits: {5} Misses: {6} Discarded: {7}",
                    typeof(TKey).Name, typeof(TValue).Name, this.maxSize, this.currentSize, this.itemSize, this.hitCount, this.missCount, this.discardedCount);
            }
        }

        private void TrimToSize(int maxSize)
        {
            lock (this.locker)
            {
                while (this.currentSize > maxSize)
                {
                    this.Remove(this.OldestKey);
                    discardedCount++;
                }
            }
        }

        private void RenewKey(TKey key)
        {
            lock (this.locker)
            {
                if (this.list.Contains(key))
                {
                    this.list.Remove(key);
                    this.list.AddLast(key);
                }
            }
        }
    }
}