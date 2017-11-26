using System;
using System.Linq;
using System.Text;

namespace ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting client!");

            var sadClient = new SADClient.AutoDiscoveryClient<CustomServerInfo>(8756, 8757, false);

            for (int i = 0; i < 5; i++)
            {
                var findServersTask = sadClient.FindServersAsync(1000, Encoding.UTF8.GetBytes("Anyone?"));

                findServersTask.Wait();

                var servers = findServersTask.Result;

                if (servers.Any())
                {
                    foreach (var serverInfo in servers)
                    {
                        Console.WriteLine("Server found at endpoint " + serverInfo.ServerEndpoint + " with message: " + serverInfo.ServerMessage);
                    }
                }
                else
                {
                    Console.WriteLine("No Servers Found!");
                }
            }

            Console.WriteLine("All done, press any key to exit...");
            Console.ReadKey();
        }
    }
}