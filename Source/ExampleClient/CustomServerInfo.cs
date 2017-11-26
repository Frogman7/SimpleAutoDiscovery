using SADClient;
using System.Text;
using System.Net;

namespace ExampleClient
{
    public class CustomServerInfo : ServerInformation
    {
        public string ServerMessage { get; }

        public CustomServerInfo(IPEndPoint serverEndpoint, byte[] message) : base(serverEndpoint, message)
        {
            this.ServerMessage = Encoding.ASCII.GetString(message);
        }
    }
}