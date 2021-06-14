using System;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Module13.Redis.ConsoleApp
{
    public class Program
    {
        public const string AzureRedisName = "";
        public const string AzureRedisKey = "";
        public static readonly string ConnectionString = $"{AzureRedisName}.redis.cache.windows.net:6380,password={AzureRedisKey},ssl=True,abortConnect=False";

        static async Task Main(string[] args)
        {
            using var cache = ConnectionMultiplexer.Connect(ConnectionString);
            
            IDatabase db = cache.GetDatabase();

            var result = await db.ExecuteAsync("ping");
            Console.WriteLine($"PING = {result.Type} : {result}");


            bool setValue = await db.StringSetAsync("test:key", "100");
            Console.WriteLine($"SET: {setValue}");

            string getValue = await db.StringGetAsync("test:key");
            Console.WriteLine($"GET: {getValue}");





        }
    }
}
