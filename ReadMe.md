
How to Use:
-------------------------------------------------

Reference FoneDynamicsChallenge.Caching project in your solution.

Create instances of ICache<TKey, TValue> implementations directly or via Dependency Injection.

Sample usage is shown in project FoneDynamicsChallenge.Sample.

Solution Structure:
-------------------------------------------------

Module FoneDynamicsChallenge.Caching

This project is a class library which contains 3 implementations for interface ICache<TKey, TValue>.

Implementations A and B comply the following requirements:
- thread safety
- O(1) complexity on insert/get/update/evict operations
- will not contain more items than a number specified as a limit (in case the limit is reached least recently added/updated/retrieved item is evicted from the cache)

The only difference between A and B is that A uses ReaderWriterLockSlim and B uses SpinLock.

Implementation C complies the following requirements:
- thread safety
- O(1) complexity on insert/get/update operations

The eviction of cache elements is not triggered by reaching the limit of items but instead by process memory limit.

Module FoneDynamicsChallenge.UnitTests

This project is a collection of unit tests to ensure that all implementations of ICache<TKey, TValue> support basic cache management scenarios.

Code coverage is specified in Appendix A.

Module FoneDynamicsChallenge.PerformanceTests.

This project contains performance tests to measure the implementation of each implementation.
The tests run in two modes - concurrency maximization and memory utilization.
In the first mode all cache management operations use the same data instance. This decreases the object construction  payload and increases the number of operations with cache per second.
In the second mode every write operation executes with the randomly generated data instance. Though, this is a synthetic case it is still much closer to real-world scenario.

The performance results for all three implementations are in Appendix B.

Module FoneDynamicsChallenge.Sample

This project contain the example of FoneDynamicsChallenge.Caching utilization.

Appendixes:
-------------------------------------------------

A. Code Coverage Calculation

1) Install OpenCover https://github.com/OpenCover/opencover
2) Make full rebuild 
dotnet build "FoneDynamicsChallenge.sln" /p:DebugType=Full
3) Calculate code coverage
OpenCover.Console.exe -target:"C:/Program Files/dotnet/dotnet.exe" -targetargs:"test \"[Path to solution]\FoneDynamicsChallenge.UnitTests\FoneDynamicsChallenge.UnitTests.csproj\" --configuration Debug --no-build" -filter:"+[*]* -[*.Tests*]*" -oldStyle -register:user -output:"OpenCover.xml"

Current code coverage:

Visited Classes 9 of 12 (75)

Visited Methods 49 of 58 (84.48)

Visited Points 275 of 412 (66.75)

Visited Branches 78 of 108 (72.22)

==== Alternative Results (includes all methods including those without corresponding source) ====

Alternative Visited Classes 13 of 17 (76.47)

Alternative Visited Methods 59 of 71 (83.1)

B. Performance Test Results

Machine: Core i7 + 16GB RAM 

Summary

Max Concurrency Mode

Implementation		A		B		C

Reads/s			377310	504528	2098720

Writes/s		75462	100905	 419744


Memory Utilization Mode

Implementation		A		B		C

Reads/s			98468    114192  316652

Writes/s		19693     22838   63330

Max Memory(MB)  11369     11307   11175

Avg Memory(MB)   7373      8588    7085

Details

-------------Max Concurrency Mode----------------

Cache type: FoneDynamicsChallenge.Caching.ImplementationA.MemoryCache`2
Threads: 50, Items per thread: 100000, Reads per thread: 500000
Time Elapsed: 00:01:06.2583872
Total reads: 25000000, total writes: 5000000
Reads per second: 377310, writes per second: 75462
Max memory used (MB): 215
Average memory used (MB): 179

Cache type: FoneDynamicsChallenge.Caching.ImplementationB.MemoryCache`2
Threads: 50, Items per thread: 100000, Reads per thread: 500000
Time Elapsed: 00:00:49.5511862
Total reads: 25000000, total writes: 5000000
Reads per second: 504528, writes per second: 100905
Max memory used (MB): 351
Average memory used (MB): 260

Cache type: FoneDynamicsChallenge.Caching.ImplementationC.MemoryCache`2
Threads: 50, Items per thread: 100000, Reads per thread: 500000
Time Elapsed: 00:00:11.9120179
Total reads: 25000000, total writes: 5000000
Reads per second: 2098720, writes per second: 419744
Max memory used (MB): 304
Average memory used (MB): 279

-------------Memory Utilization Mode-------------

Cache type: FoneDynamicsChallenge.Caching.ImplementationA.MemoryCache`2
Threads: 50, Items per thread: 100000, Reads per thread: 500000
Time Elapsed: 00:04:13.8871843
Total reads: 25000000, total writes: 5000000
Reads per second: 98468, writes per second: 19693
Max memory used (MB): 11369
Average memory used (MB): 7373

Cache type: FoneDynamicsChallenge.Caching.ImplementationB.MemoryCache`2
Threads: 50, Items per thread: 100000, Reads per thread: 500000
Time Elapsed: 00:03:38.9286977
Total reads: 25000000, total writes: 5000000
Reads per second: 114192, writes per second: 22838
Max memory used (MB): 11307
Average memory used (MB): 8588

Cache type: FoneDynamicsChallenge.Caching.ImplementationC.MemoryCache`2
Threads: 50, Items per thread: 100000, Reads per thread: 500000
Time Elapsed: 00:01:18.9508052
Total reads: 25000000, total writes: 5000000
Reads per second: 316652, writes per second: 63330
Max memory used (MB): 11175
Average memory used (MB): 7085

