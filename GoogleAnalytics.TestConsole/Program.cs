using System;
using System.Threading.Tasks;

namespace GoogleAnalytics.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Tracker tracker = new Tracker("UA-XXXXXXXXX-X");
            await tracker.TrackPageview("/tracker", "Tracker Development", "123");
            
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
