using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamicsChallenge.Caching.Common
{
    /// <summary>
    /// Default comparer for objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class DefaultEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <inheritdoc />
        public bool Equals(T x, T y)
        {
            return object.Equals(x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
