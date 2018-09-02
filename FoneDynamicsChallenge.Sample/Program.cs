using FoneDynamicsChallenge.Caching.ImplementationA;
using FoneDynamicsChallenge.Caching.Interfaces;
using System;

namespace FoneDynamicsChallenge.Sample
{
    class Program
    {
        private static readonly ICache<string, string> _cache = new MemoryCache<string, string>();

        static void Main(string[] args)
        {
            char command = 'a';
            while (command != 'x')
            {
                Console.WriteLine("Enter 's' to save data to cache, 'l' to load data from cache or 'x' to exit");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                command = keyInfo.KeyChar;
                switch(command)
                {
                    case 's':
                        {
                            Console.WriteLine("Enter cache key:");
                            string key = Console.ReadLine();
                            Console.WriteLine("Enter cache data:");
                            string data = Console.ReadLine();
                            _cache.AddOrUpdate(key, data);
                        }
                        break;
                    case 'l':
                        {
                            Console.WriteLine("Enter cache key:");
                            string key = Console.ReadLine();
                            if (_cache.TryGetValue(key,out string data))
                            {
                                Console.WriteLine("Cached data: {0}",data);
                            }
                            else
                            {
                                Console.WriteLine("No data found with key {0}", key);
                            }
                        }
                        break;
                    case 'x':
                        break;
                    default:
                        Console.WriteLine("Invalid key pressed");
                        break;
                }
            }
        }
    }
}