using System;
using System.Threading.Tasks;

namespace Module01.WebJob
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting webjob");

            while (true)
            {
                Console.WriteLine($"Webjob tick @{DateTime.Now}");

                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }
    }
}
