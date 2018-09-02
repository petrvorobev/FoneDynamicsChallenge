using FoneDynamicsChallenge.Caching.Common;
using FoneDynamicsChallenge.Caching.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FoneDynamicsChallenge.Caching.ImplementationA
{
    /// <summary>
    /// "Academic" cache implementation
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Data type</typeparam>
    /// <remarks>
    /// Implementation is based on the following articles:
    /// https://docs.microsoft.com/ru-ru/dotnet/api/system.threading.readerwriterlockslim?view=netcore-2.1
    /// https://www.codeproject.com/Articles/1095822/Choosing-The-Right-Collection
    /// </remarks>
    public class MemoryCache<TKey, TValue> : ICache<TKey, TValue>, ICountable
    {
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        private readonly LinkedList<MemoryCacheItem<TKey, TValue>> _accessChain = new LinkedList<MemoryCacheItem<TKey, TValue>>();

        private readonly Dictionary<TKey, LinkedListNode<MemoryCacheItem<TKey, TValue>>> _cacheItems = new Dictionary<TKey, LinkedListNode<MemoryCacheItem<TKey, TValue>>>();

        private readonly int _limit;

        private readonly IEqualityComparer<TValue> _comparer;

        /// <summary>
        /// Number od items in cache
        /// </summary>
        /// <remarks>snapshot use only</remarks>
        public int Count
        {
            get
            {
                return _cacheItems.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">Max number of items in cache</param>
        /// <param name="comparer">Comparer for cache objects</param>
        public MemoryCache(int limit, IEqualityComparer<TValue> comparer)
        {
            _limit = limit;
            _comparer = comparer;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">Max number of items in cache</param>
        public MemoryCache(int limit)
            :this(limit,new DefaultEqualityComparer<TValue>())
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryCache()
            : this(100000)
        {

        }

        /// <summary>
        /// Make current cache item most recently used
        /// </summary>
        /// <param name="accessChainItem">cache item wrapped into node of type <see cref="LinkedListNode"/> </param>
        /// <remarks>Both operation in the method have O(1) complexity</remarks>
        /// 
        private void UpdateAccessChain(LinkedListNode<MemoryCacheItem<TKey, TValue>> accessChainItem)
        {
            _accessChain.Remove(accessChainItem);
            _accessChain.AddFirst(accessChainItem);
        }

        /// <summary>
        /// ake current cache item most recently used
        /// </summary>
        /// <param name="cacheItem">cache item</param>
        /// <returns>cache item wrapped into node of type <see cref="LinkedListNode"/></returns>
        /// <remarks>Both operation in the method have O(1) complexity</remarks>
        private LinkedListNode<MemoryCacheItem<TKey, TValue>> UpdateAccessChain(MemoryCacheItem<TKey, TValue> cacheItem)
        {
            return _accessChain.AddFirst(cacheItem);
        }

        /// <inheritdoc />
        public void AddOrUpdate(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                //O(1) complexity
                if (_cacheItems.TryGetValue(key, out LinkedListNode<MemoryCacheItem<TKey, TValue>> result))
                {
                    TValue cachedValue = result.Value.Value;

                    if (_comparer.Equals(cachedValue, value))
                    {
                        _cacheLock.EnterWriteLock();
                        try
                        {
                            //O(1) complexity
                            UpdateAccessChain(result);
                        }
                        finally
                        {
                            _cacheLock.ExitWriteLock();
                        }
                    }
                    else
                    {
                        _cacheLock.EnterWriteLock();
                        try
                        {
                            result.Value.Value = value;
                            //O(1) complexity
                            UpdateAccessChain(result);
                        }
                        finally
                        {
                            _cacheLock.ExitWriteLock();
                        }
                    }
                }
                else
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        //O(1) complexity
                        LinkedListNode<MemoryCacheItem<TKey, TValue>> newItem = UpdateAccessChain(new MemoryCacheItem<TKey, TValue>() { Key = key, Value = value });
                        //O(1) complexity
                        _cacheItems.Add(key, newItem);
                        if (_cacheItems.Count>_limit)
                        {
                            LinkedListNode<MemoryCacheItem<TKey, TValue>> oldestItem = _accessChain.Last;
                            //O(1) complexity
                            _cacheItems.Remove(oldestItem.Value.Key);
                            //O(1) complexity
                            _accessChain.Remove(oldestItem);
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
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                //O(1) complexity
                if (_cacheItems.TryGetValue(key, out LinkedListNode<MemoryCacheItem<TKey, TValue>> result))
                {
                    value = result.Value.Value;
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        //O(1) complexity
                        UpdateAccessChain(result);
                        return true;
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
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
