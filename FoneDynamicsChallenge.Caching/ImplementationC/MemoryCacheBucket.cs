using System;
using FoneDynamicsChallenge.Caching.Interfaces;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace FoneDynamicsChallenge.Caching.ImplementationC
{
    /// <summary>
    /// Cache bucket implementation
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Cached data type</typeparam>
    class MemoryCacheBucket<TKey, TValue> : ICache<TKey, TValue>, ICountable
    {

        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        private readonly Dictionary<TKey, MemoryCacheBucketItem<TValue>> _cacheItems = new Dictionary<TKey, MemoryCacheBucketItem<TValue>>();

        private readonly IEqualityComparer<TValue> _comparer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">comparer to ensure the equality of 2 values</param>
        public MemoryCacheBucket(IEqualityComparer<TValue> comparer)
        {
            _comparer = comparer;
        }

        /// <inheritdoc />
        public int Count => _cacheItems.Count;

        /// <summary>
        /// Cleanup cache bucket by eviciting all items which live longer than <paramref name="maxObjectLifetime"/>
        /// </summary>
        /// <param name="maxObjectLifetime">max lifetime of an item in cache</param>
        public void Cleanup(long maxObjectLifetime)
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                List<TKey> keysToRemove = new List<TKey>();
                foreach(KeyValuePair<TKey, MemoryCacheBucketItem<TValue>> item in _cacheItems)
                {
                    if ((DateTime.Now - item.Value.LastUsed).TotalMilliseconds>maxObjectLifetime)
                    {
                        keysToRemove.Add(item.Key);
                    }
                }
                if (keysToRemove.Count>0)
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        foreach(TKey key in keysToRemove)
                        {
                            _cacheItems.Remove(key);
                        }
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <inheritdoc />
        public void AddOrUpdate(TKey key, TValue value)
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                //O(1) complexity
                if (_cacheItems.TryGetValue(key, out MemoryCacheBucketItem<TValue> result))
                {
                    TValue cachedValue = result.Value;

                    if (_comparer.Equals(cachedValue, value))
                    {
                        lock (result)
                        {
                            result.LastUsed = DateTime.Now;
                        }
                    }
                    else
                    {
                        lock (result)
                        {
                            result.Value = value;
                            result.LastUsed = DateTime.Now;
                        }
                    }
                }
                else
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        _cacheItems.Add(key, new MemoryCacheBucketItem<TValue> { Value = value, LastUsed = DateTime.Now });
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                //O(1) complexity
                if (_cacheItems.TryGetValue(key, out MemoryCacheBucketItem<TValue> result))
                {
                    value = result.Value;
                    lock (result)
                    {
                        result.LastUsed = DateTime.Now;
                    }
                    return true;
                }
                value = default(TValue);
                return false;
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}