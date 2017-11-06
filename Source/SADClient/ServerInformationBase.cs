using System.Net;

namespace SADClient
{
    /// <summary>
    /// An abstract datastructure responsible for describing information on a Autodiscovery server.
    /// </summary>
    public abstract class ServerInformationBase
    {
        /// <summary>
        /// Gets the Autodiscovery server's IP endpoint.
        /// </summary>
        /// <value>
        /// The Autodiscovery server's IP endpoint.
        /// </value>
        public IPEndPoint ServerEndpoint { get; }

        /// <summary>
        /// Gets the Autodiscovery servers broadcast response message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        protected byte[] Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerInformationBase"/> class.
        /// </summary>
        /// <param name="serverEndpoint">The IP endpoint of the Autodiscovery server.</param>
        /// <param name="message">The broadcast response message of the Autodiscovery server.</param>
        public ServerInformationBase(IPEndPoint serverEndpoint, byte[] message)
        {
            this.ServerEndpoint = serverEndpoint;
            this.Message = message;
        }
    }
}
