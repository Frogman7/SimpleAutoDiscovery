using System;

namespace SADServer
{
    public interface IAutoDiscoveryServer
    {
        /// <summary>
        /// Occurs when a broadcast is received from a client.
        /// </summary>
        event EventHandler<ClientMessageReceivedEventArgs> OnClientMessageReceived;

        /// <summary>
        /// Gets a value indicating whether the server is listening for and responding to broadcasts.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server is listening; otherwise, <c>false</c>.
        /// </value>
        bool IsListening { get; }

        /// <summary>
        /// Starts listening for broadcasts.
        /// </summary>
        void StartListening();

        /// <summary>
        /// Stops listening.
        /// </summary>
        void StopListening();
    }
}
