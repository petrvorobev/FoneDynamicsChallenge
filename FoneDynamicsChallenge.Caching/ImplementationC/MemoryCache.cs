using FoneDynamicsChallenge.Caching.Common;
using FoneDynamicsChallenge.Caching.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoneDynamicsChallenge.Caching.ImplementationC
{
    /// <summary>
    /// Implementation of cache optimized for multithreading
    /// </summary>
    /// <typeparam name="TKey">Cache key type</typeparam>
    /// <typeparam name="TValue">Cache data type</typeparam>
    /// <remarks>
    /// The default cache parameters are:
    /// number of buckets - 64
    /// CleanupInterval - 200 ms
    /// MaxMemorySize - 5 GB
    /// MaxObjectLifeTime - 5000 ms
    /// MinObjectLifetime - 500 ms
    /// </remarks>
    public class MemoryCache<TKey, TValue> : ICache<TKey, TValue>, ICountable
    {
        private readonly MemoryCacheBucket<TKey, TValue>[] _buckets;
        private readonly int _bucketCount;
        private readonly KeyMappingDelegate<TKey> _bucketMapper;
        private readonly IEqualityComparer<TValue> _comparer;
        private readonly Timer _cleanupTimer;
        private readonly CleanupSettings _cleanupSettings;

        private bool _cleanupStarted = false;

        private static readonly CleanupSettings _defaultCleanupSettings = new CleanupSettings
        {
            CleanupInterval =200,
            MaxMemorySize = 5368709120, //5GB
            MaxObjectLifeTime =5000,
            MinObjectLifetime =500
        };

        private static int DefaultKeyMapping(TKey key)
        {
            Int32 keyHash = key.GetHashCode();
            return keyHash & 0x0000003f; //((1 << 6) - 1)
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Sets all parameters by default</remarks>
        public MemoryCache()
           : this( _defaultCleanupSettings)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cleanupSettings">Cache eviction settings</param>
        public MemoryCache(CleanupSettings cleanupSettings)
            : this(new DefaultEqualityComparer<TValue>(),cleanupSettings)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">comparer to ensure the equality of 2 values</param>
        /// <param name="cleanupSettings">Cache eviction settings</param>
        public MemoryCache(IEqualityComparer<TValue> comparer, CleanupSettings cleanupSettings)
            :this(64,DefaultKeyMapping, comparer,cleanupSettings)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketCount">number of cache buckets</param>
        /// <param name="bucketMapper">bucket mapper</param>
        /// <param name="comparer">comparer to ensure the equality of 2 values</param>
        /// <param name="cleanupSettings">Cache eviction settings</param>
        public MemoryCache(int bucketCount, KeyMappingDelegate<TKey> bucketMapper, IEqualityComparer<TValue> comparer, CleanupSettings cleanupSettings)
        {
            _bucketCount = bucketCount;
            _buckets = new MemoryCacheBucket<TKey, TValue>[_bucketCount];
            _bucketMapper = bucketMapper;
            _comparer = comparer;

            for (int i=0;i<_bucketCount;i++)
            {
                _buckets[i] = new MemoryCacheBucket<TKey, TValue>(_comparer);
            }
            _cleanupSettings = cleanupSettings;
            _cleanupTimer = new Timer(new TimerCallback(CacheCleanup), this, _cleanupSettings.CleanupInterval, _cleanupSettings.CleanupInterval);
        }

        private void CacheCleanup(object state)
        {
            if (_cleanupStarted)
            {
                return;
            }
            long memorySize = Process.GetCurrentProcess().WorkingSet64;
            
            if (memorySize>_cleanupSettings.MaxMemorySize)
            {
                _cleanupStarted = true;
                try
                {
                    long cleanupLifetime = _cleanupSettings.MaxObjectLifeTime;
                    while (cleanupLifetime>_cleanupSettings.MinObjectLifetime || Process.GetCurrentProcess().WorkingSet64>_cleanupSettings.MaxMemorySize)
                    {
                        List<Task> cleanupTasks = new List<Task>();
                        foreach (MemoryCacheBucket<TKey, TValue> bucket in _buckets)
                        {
                            cleanupTasks.Add(Task.Run(() => { bucket.Cleanup(cleanupLifetime); }));
                        }
                        Task.WhenAll(cleanupTasks);
                        cleanupLifetime = cleanupLifetime / 2;
                    }
                }
                finally
                {
                    _cleanupStarted = false;
                }
            }
        }

        /// <summary>
        /// Number of items in cache
        /// </summary>
        /// <remarks>snapshot use only</remarks>
        public int Count =>_buckets.Sum(b=>b.Count);

        /// <inheritdoc />
        public void AddOrUpdate(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            MemoryCacheBucket<TKey, TValue> bucket = _buckets[_bucketMapper(key)];
            bucket.AddOrUpdate(key, value);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            MemoryCacheBucket<TKey, TValue> bucket = _buckets[_bucketMapper(key)];
            return bucket.TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// The delegate for a method to map a cache key to a paticular bucket
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <param name="key">cache key</param>
    /// <returns>Number of a bucket</returns>
    public delegate int KeyMappingDelegate<TKey>(TKey key);
}
