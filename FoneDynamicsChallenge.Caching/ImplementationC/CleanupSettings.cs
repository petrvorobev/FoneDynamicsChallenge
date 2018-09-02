using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamicsChallenge.Caching.ImplementationC
{
    /// <summary>
    /// Cache eviction configuration
    /// </summary>
    public class CleanupSettings
    {
        /// <summary>
        /// Cache cleanup interval (milliseconds)
        /// </summary>
        public int CleanupInterval { get; set; }

        /// <summary>
        /// In case current process exceeds this size (in bytes) the oldest cache items must be removed
        /// </summary>
        /// <remarks>Process.WorkingSet64 is used to calculate the process memory size</remarks>
        public long MaxMemorySize { get; set; }

        /// <summary>
        /// Objects which reside in cache longer that this value (in ticks) are first candidates for removal
        /// </summary>
        public long MaxObjectLifeTime { get; set; }

        /// <summary>
        /// Objects which in cache less that this value (in ticks) will not be removed
        /// </summary>
        public long MinObjectLifetime { get; set; }

    }
}
