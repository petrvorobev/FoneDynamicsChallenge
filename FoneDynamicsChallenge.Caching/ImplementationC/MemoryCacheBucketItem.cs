using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamicsChallenge.Caching.ImplementationC
{
    /// <summary>
    /// Cache item container
    /// </summary>
    /// <typeparam name="TValue">Cache data type</typeparam>
    class MemoryCacheBucketItem<TValue>
    {
        /// <summary>
        /// Cached data
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Last time data was used (vai add/get or update)
        /// </summary>
        public DateTime LastUsed { get; set; }
    }
}
