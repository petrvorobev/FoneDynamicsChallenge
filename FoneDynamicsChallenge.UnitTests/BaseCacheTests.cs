using FoneDynamicsChallenge.Caching.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamicsChallenge.UnitTests
{
    public abstract class BaseCacheTests< TCache>
        where TCache : ICache<string, string>, ICountable,  new()
    {
        /// <summary>
        /// Insert data with a valid key
        /// </summary>
        /// <remarks>The number of items in cache should incarease by 1</remarks>
        [TestMethod]
        public void AddOrUpdate_InsertDataWithValidKey_CountIncreasedBy1()
        {
            //Arrange
            TCache cache = new TCache();
            int count = cache.Count;

            //Act
            cache.AddOrUpdate("key", "value");

            //Assert
            int newCount = cache.Count;
            Assert.AreEqual(1, newCount - count);
        }

        /// <summary>
        /// Insert data with NULL as key
        /// </summary>
        /// <remarks><see cref="ArgumentNullException"/> should be thrown</remarks>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOrUpdate_InsertDataWithNullKey_ArgumentNullException()
        {
            //Arrange
            TCache cache = new TCache();

            //Act
            cache.AddOrUpdate(null, "value");

        }

        /// <summary>
        /// Get data with NULL as key
        /// </summary>
        /// <remarks><see cref="ArgumentNullException"/> should be thrown</remarks>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryGetValue_GetDataWithNullKey_ArgumentNullException()
        {
            //Arrange
            TCache cache = new TCache();

            //Act
            cache.TryGetValue(null, out string data);

        }


        /// <summary>
        /// Get existing data with a valid key
        /// </summary>
        [TestMethod]
        public void TryGetValue_GetDataWithValidKey_DataExist()
        {
            //Arrange
            TCache cache = new TCache();
            string data = "value";
            string key = "key";
            cache.AddOrUpdate(key,data);

            //Act
            bool operationResult = cache.TryGetValue(key, out string dataFromCache);

            //Assert
            Assert.IsTrue(operationResult);
        }

        /// <summary>
        /// Get existing data with a valid key
        /// </summary>
        [TestMethod]
        public void TryGetValue_GetDataWithValidKey_DataEqualsTheOriginal()
        {
            //Arrange
            TCache cache = new TCache();
            string data = "value";
            string key = "key";
            cache.AddOrUpdate(key, data);

            //Act
            bool operationResult = cache.TryGetValue(key, out string dataFromCache);

            //Assert
            Assert.AreEqual(dataFromCache,data);
        }

        /// <summary>
        /// Get non-existing data with a valid key
        /// </summary>
        [TestMethod]
        public void TryGetValue_GetNonExistingDataWithValidKey_OperationResultIsFalse()
        {
            //Arrange
            TCache cache = new TCache();
            string key = "key";

            //Act
            bool operationResult = cache.TryGetValue(key, out string dataFromCache);

            //Assert
            Assert.IsFalse(operationResult);
        }
    }
}
