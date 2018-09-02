using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamicsChallenge.Caching.Interfaces
{
    /// <summary>
    /// Interface to get the number of some elements in the class
    /// </summary>
    public interface ICountable
    {
        /// <summary>
        /// Number of elements
        /// </summary>
        int Count { get; }
    }
}
