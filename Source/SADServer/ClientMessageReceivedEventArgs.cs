using System;
using System.Net;

namespace SADServer
{
    public class ClientMessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint ClientEndpoint { get; }

        public byte[] Message { get; }

        public ClientMessageReceivedEventArgs(IPEndPoint clientEndpoint, byte[] message)
        {
            this.ClientEndpoint = clientEndpoint;
            this.Message = message;
        }
    }
}
