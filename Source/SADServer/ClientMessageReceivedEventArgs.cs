using System;
using System.Net;

namespace SADServer
{
    /// <summary>
    /// The messaged received from a client event arguments.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ClientMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the client endpoint.
        /// </summary>
        /// <value>
        /// The client endpoint.
        /// </value>
        public IPEndPoint ClientEndpoint { get; }

        /// <summary>
        /// Gets the message sent by the client.
        /// </summary>
        /// <value>
        /// The clinet broadcast message as a byte array.
        /// </value>
        public byte[] Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="clientEndpoint">The client endpoint.</param>
        /// <param name="message">The client broadcast message.</param>
        public ClientMessageReceivedEventArgs(IPEndPoint clientEndpoint, byte[] message)
        {
            this.ClientEndpoint = clientEndpoint;
            this.Message = message;
        }
    }
}