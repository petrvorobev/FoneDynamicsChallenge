using FoneDynamicsChallenge.Caching.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace PerformanceTests
{
    /// <summary>
    /// Performance tests
    /// </summary>
    class Program
    {
        /// <summary>
        /// number of concurrent tests
        /// </summary>
        private static int threadCount = 50;

        /// <summary>
        /// number of items the thread writes to cache
        /// </summary>
        private static int itemsPerThread = 100000;

        /// <summary>
        /// Cache limit for count based eviction
        /// </summary>
        private static int cacheLimit = itemsPerThread * 10;

        /// <summary>
        /// number of items the thread tries to read from cache per one write
        /// </summary>
        private static int readOperationPerWrite = 5;

        /// <summary>
        /// length of the string key
        /// </summary>
        private static int keyLength = 5;

        /// <summary>
        /// generate new data per each write or use static data sample each time
        /// </summary>
        private static bool generateTestData = true;

        /// <summary>
        /// static data sample
        /// </summary>
        private static TestData staticDataSample = new TestData();

        /// <summary>
        /// Maximum number of strings per sample data
        /// </summary>
        private static int numberOfStingsInSample = 50;

        /// <summary>
        /// Maximum number of chars in sample string
        /// </summary>
        private static int sampleStringLength = 200;

        /// <summary>
        /// Memory limit for concurrency-heavy test
        /// </summary>
        private static long memoryLimitForConcurrencyTest = 209715200; //200MB


        /// <summary>
        /// Memory limit for concurrency-heavy test
        /// </summary>
        private static long memoryLimitForMemoryUsageTest = 5368709120; //5GB


        /// <summary>
        /// Method to create data sample
        /// </summary>
        /// <param name="random">random number generator</param>
        /// <returns>test data</returns>
        private static TestData CreateTestData(Random random)
        {

            if (generateTestData)
            {
                int dataSize = random.Next(numberOfStingsInSample);
                TestData testData = new TestData()
                {
                    Data = new string[dataSize]
                };
                for (int i = 0; i < dataSize; i++)
                {
                    testData.Data[i] = new string('a', random.Next(sampleStringLength));
                }
                return testData;
            }
            else
            {
                return staticDataSample;
            }
        }

        /// <summary>
        /// Method to create cache key
        /// </summary>
        /// <param name="keyLength">key length</param>
        /// <returns>key</returns>
        private static string CreateKey(int keyLength)
        {
            return Guid.NewGuid().ToString().Substring(0, keyLength);
        }

        /// <summary>
        /// Cache test
        /// </summary>
        /// <param name="cache">cache instance</param>
        private static void Test( ICache<string, TestData> cache)
        {

            Console.WriteLine("Cache type: {0}.{1}", cache.GetType().Namespace, cache.GetType().Name);
            Console.WriteLine("Threads: {0}, Items per thread: {1}, Reads per thread: {2}", threadCount, itemsPerThread, itemsPerThread*readOperationPerWrite);
            Stopwatch stopwatch = new Stopwatch();
            ManualResetEvent mre = new ManualResetEvent(false);
            List<WaitHandle> threadHandles = new List<WaitHandle>();
            List<long> memoryUsageList = new List<long>();
            for (int i=0;i<threadCount;i++)
            {
                EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    mre.WaitOne();
                    Random random = new Random(DateTime.Now.Millisecond);
                    for (int j=0;j<itemsPerThread;j++)
                    {
                        string key = CreateKey(keyLength);
                        TestData data = CreateTestData(random);
                        cache.AddOrUpdate(key, data);
                        for (int k=0;k<readOperationPerWrite;k++)
                        {
                            string keyToSearch = CreateKey(keyLength);
                            cache.TryGetValue(keyToSearch, out TestData cachedData);
                        }
                    }

                    handle.Set();
                }));
                thread.Start();
                threadHandles.Add(handle);
            }
            Timer memoryMonitor = new Timer(new TimerCallback(s => memoryUsageList.Add(Process.GetCurrentProcess().WorkingSet64)), null, 100, 100);
            stopwatch.Start();
            mre.Set();

            WaitHandle.WaitAll(threadHandles.ToArray());
            stopwatch.Stop();
            memoryMonitor.Dispose();

            Console.WriteLine("Time Elapsed: {0}", stopwatch.Elapsed);
            Console.WriteLine("Total reads: {0}, total writes: {1}", itemsPerThread * readOperationPerWrite*threadCount, itemsPerThread  * threadCount);
            Console.WriteLine("Reads per second: {0}, writes per second: {1}", (long)((itemsPerThread * readOperationPerWrite * threadCount)/stopwatch.Elapsed.TotalSeconds),(long)(( itemsPerThread * threadCount) / stopwatch.Elapsed.TotalSeconds));
            Console.WriteLine("Max memory used (MB): {0}", (long)(memoryUsageList.Max() / 1048576));
            Console.WriteLine("Average memory used (MB): {0}",(long)( memoryUsageList.Average() / 1048576));
            GC.Collect();

        }

        /// <summary>
        /// Method to run test for cache for all instances
        /// </summary>
        /// <param name="memoryLimit">memory limit (used by ImplC only)</param>
        static void RunTests(long memoryLimit)
        {
            Test(new FoneDynamicsChallenge.Caching.ImplementationA.MemoryCache<string, TestData>(cacheLimit));
            Thread.Sleep(200);
            Test(new FoneDynamicsChallenge.Caching.ImplementationB.MemoryCache<string, TestData>(cacheLimit));
            Thread.Sleep(200);
            Test(new FoneDynamicsChallenge.Caching.ImplementationC.MemoryCache<string, TestData>(new FoneDynamicsChallenge.Caching.ImplementationC.CleanupSettings
            {
                CleanupInterval = 200,
                MaxMemorySize = memoryLimit,
                MaxObjectLifeTime = 2500,
                MinObjectLifetime = 500
            }));
            Thread.Sleep(200);
        }

        /// <summary>
        /// Entry method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //concurrency tests
            generateTestData = false;
            RunTests(memoryLimitForConcurrencyTest);
            //memory usage tests
            generateTestData = true;
            RunTests(memoryLimitForMemoryUsageTest);
            Console.ReadLine();
            Console.WriteLine("All test complete");
        }
    }
}