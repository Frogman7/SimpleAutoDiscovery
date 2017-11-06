using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SADClient
{
    /// <summary>
    /// The AutoDiscovery client handles the logic of finding AutoDiscovery servers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="SADClient.IAutoDiscoveryClient{T}" />
    public class AutoDiscoveryClient<T>  : IAutoDiscoveryClient<T> where T : ServerInformationBase
    {
        /// <summary>
        /// The Internet Protocol address to use for receiving broadcast responses
        /// </summary>
        protected IPAddress ipAddress;

        /// <summary>
        /// The port to use for receiving broadcast messages
        /// </summary>
        protected ushort sendPort;

        /// <summary>
        /// The port to use for receiving messages
        /// </summary>
        protected ushort receivePort;

        /// <summary>
        /// Determines if we should setup the socket for port reuse
        /// </summary>
        protected bool allowSocketReuse;

        /// <summary>
        /// Used for tracking the discovered servers when searching
        /// </summary>
        protected IList<T> discoveredServers;

        /// <summary>
        /// The UDP client used for the underlying network connection
        /// </summary>
        protected UdpClient udpClient;

        /// <summary>
        /// Used for tracking whether or not a server search is being performed
        /// </summary>
        protected bool findingServers;

        /// <summary>
        /// The current asynchronous result to track
        /// </summary>
        private IAsyncResult expectedAsyncResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoDiscoveryClient{T}"/> class.
        /// </summary>
        /// <param name="sendPort">The UDP port to use for the broadcast.</param>
        /// <param name="allowPortReuse">If set to <c>true</c> the socket will be setup to allow port reuse.</param>
        public AutoDiscoveryClient(ushort sendPort, bool allowPortReuse = false) : this(sendPort, sendPort, allowPortReuse)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoDiscoveryClient{T}"/> class.
        /// </summary>
        /// <param name="sendPort">The UDP port to use for the broadcast.</param>
        /// <param name="receivePort">The UDP port to use for receiving broadcast responses.</param>
        /// <param name="allowPortReuse">If set to <c>true</c> the socket will be setup to allow port reuse.</param>
        public AutoDiscoveryClient(ushort sendPort, ushort receivePort, bool allowPortReuse = false) : this(IPAddress.Any, sendPort, receivePort, allowPortReuse)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoDiscoveryClient{T}"/> class.
        /// </summary>
        /// <param name="ipAddress">
        /// The IP address to use for network communication (useful if listening on all network interfaces is not desirable).
        /// </param>
        /// <param name="sendPort">The UDP port to use for the broadcast.</param>
        /// <param name="receivePort">The UDP port to use for receiving broadcast responses.</param>
        /// <param name="allowSocketReuse">If set to <c>true</c> the socket will be setup to allow port reuse.</param>
        public AutoDiscoveryClient(IPAddress ipAddress, ushort sendPort, ushort receivePort, bool allowSocketReuse = false)
        {
            this.ipAddress = ipAddress;
            this.sendPort = sendPort;
            this.receivePort = receivePort;
            this.allowSocketReuse = allowSocketReuse;

            this.findingServers = false;

            this.discoveredServers = new List<T>();
        }

        /// <summary>
        /// Gets a value indicating whether the client is currently searching for servers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the client is searching for servers; otherwise, <c>false</c>.
        /// </value>
        public bool FindingServers
        {
            get
            {
                return this.findingServers;
            }
        }

        /// <summary>
        /// Finds the servers.
        /// </summary>
        /// <param name="timeToReply">
        /// The time to wait in milliseconds for broadcast replies.
        /// </param>
        /// <param name="message">The message to send the server.</param>
        /// <returns>
        /// A collection of found server informations.
        /// </returns>
        public virtual IEnumerable<T> FindServers(int timeToReply, byte[] message)
        {
            // Ensure that 'timeToReply' is a valid value
            if (timeToReply < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToReply), "Cannot be less than 0");
            }
            else if (timeToReply == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToReply), "Cannot be 0");
            }

            // If the message byte array is null we'll just assume the developer wanted no message and make it an empty array
            if (message == null)
            {
                message = new byte[0];
            }

            // Make sure a search isn't already running
            if (this.findingServers)
            {
                throw new InvalidOperationException($"{nameof(this.FindServers)} can only be called once at a time, please wait for the last call to {nameof(this.FindServers)} to complete");
            }

            this.findingServers = true;

            // Make sure we clean out the previous 'discovered servers' before we search again
            this.discoveredServers.Clear();

            var endpoint = new IPEndPoint(this.ipAddress, this.receivePort);

            using (var udpClient = new UdpClient())
            {
                this.udpClient = udpClient;

                if (this.allowSocketReuse)
                {
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }

                udpClient.Client.Bind(endpoint);

                udpClient.EnableBroadcast = true;

                this.expectedAsyncResult = this.udpClient.BeginReceive(this.OnReceive, null);

                udpClient.SendAsync(message,  message.Length, new IPEndPoint(IPAddress.Broadcast, sendPort));

                // Perhaps there is a better option for blocking the thread?
                Task.Delay(timeToReply).Wait();

                udpClient.Close();
            }

            var discoveredServersCopy = this.discoveredServers.ToArray();

            this.findingServers = false;

            return discoveredServersCopy;
        }

        /// <summary>
        /// Called when a response from the broadcast is received on the UdpClient
        /// </summary>
        /// <param name="asyncResult">
        /// Status information on the UdpClient asynchronous receive operation.
        /// </param>
        private void OnReceive(IAsyncResult asyncResult)
        {
            if (this.expectedAsyncResult != null && (expectedAsyncResult == asyncResult))
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.None, 0);

                byte[] bytes = null;

                try
                {
                    bytes = this.udpClient.EndReceive(asyncResult, ref endpoint);

                    this.udpClient.BeginReceive(this.OnReceive, null);

                    var newDiscoveredServer = (T)Activator.CreateInstance(typeof(T), endpoint, bytes);

                    lock (this.discoveredServers)
                    {
                        this.discoveredServers.Add(newDiscoveredServer);
                    }
                }
                // When the UdpClient is closed the OnReceive will always fire and cause the following exception to be called
                catch (ObjectDisposedException)
                {
                }
            }
        }
    }
}
