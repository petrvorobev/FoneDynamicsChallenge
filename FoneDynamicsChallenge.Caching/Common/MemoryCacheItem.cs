using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamicsChallenge.Caching.Common
{
    /// <summary>
    /// Item to store cache data with key
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Data type</typeparam>
    class MemoryCacheItem<TKey, TValue>
    {
        /// <summary>
        /// Cache key
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Cache value
        /// </summary>
        public TValue Value { get; set; }
    }
}
