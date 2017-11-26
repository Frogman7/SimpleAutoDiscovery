using System;
using System.Text;

namespace ExampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server");

            var sadServer = new SADServer.AutoDiscoveryServer(8756, 8757, Encoding.UTF8.GetBytes("Hello there!"), false);

            sadServer.OnClientMessageReceived += (obj, messageReceivedArgs) =>
            {
                Console.WriteLine("Message received from client: " + Encoding.UTF8.GetString(messageReceivedArgs.Message));
            };

            sadServer.StartListening();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            sadServer.StopListening();
        }
    }
}