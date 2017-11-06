using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SADServer
{
    public class AutoDiscoveryServer : IAutoDiscoveryServer
    {
        /// <summary>
        /// Occurs when a client broadcast is received.
        /// </summary>
        public event EventHandler<ClientMessageReceivedEventArgs> OnClientMessageReceived;

        /// <summary>
        /// The UDP client to use for receiving broadcasts and sending broadcast responses.
        /// </summary>
        protected UdpClient udpClient;

        /// <summary>
        /// The response message when responding to a broadcast.
        /// </summary>
        protected byte[] responseMessage;

        /// <summary>
        /// Determines whether or not the underlying socket should allow socket reuse.
        /// </summary>
        protected bool allowSocketReuse;

        /// <summary>
        /// The server IP endpoint to listen on.
        /// </summary>
        private IPEndPoint serverIPEndPoint;

        /// <summary>
        /// The expected asynchronous result.
        /// </summary>
        private IAsyncResult expectedAsyncResult;

        /// <summary>
        /// The receiving port (the port the client broadcast is sent on).
        /// </summary>
        private ushort receivePort;

        /// <summary>
        /// The port to send replies on.
        /// </summary>
        private ushort sendPort;

        /// <summary>
        /// Tracks whether or not the server is listening for broadcasts.
        /// </summary>
        private bool listening;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoDiscoveryServer"/> class.
        /// </summary>
        /// <param name="port">The port to use for sending and receiving.</param>
        /// <param name="message">The message to respond to broadcasts with.</param>
        /// <param name="allowSocketReuse">If set to <c>true</c> the socket will be setup to allow port reuse.</param>
        public AutoDiscoveryServer(ushort port, byte[] message, bool allowSocketReuse = false) : this(port, port, message, allowSocketReuse)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoDiscoveryServer"/> class.
        /// </summary>
        /// <param name="receivePort">The port to use for listening.</param>
        /// <param name="sendPort">The port to use for sending responses.</param>
        /// <param name="message">The message to respond to broadcasts with.</param>
        /// <param name="allowSocketReuse">If set to <c>true</c> the socket will be setup to allow port reuse.</param>
        public AutoDiscoveryServer(ushort receivePort, ushort sendPort, byte[] message, bool allowSocketReuse = false) : this(IPAddress.Any, receivePort, sendPort, message, allowSocketReuse)
        {
            this.receivePort = receivePort;
            this.sendPort = sendPort;

            this.responseMessage = message;

            this.allowSocketReuse = allowSocketReuse;

            this.listening = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoDiscoveryServer"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address to listen on if the default behavior of listening on all network devices is undesirable.</param>
        /// <param name="receivePort">The port to use for listening.</param>
        /// <param name="sendPort">The port to use for sending responses.</param>
        /// <param name="message">The message to respond to broadcasts with.</param>
        /// <param name="allowSocketReuse">If set to <c>true</c> the socket will be setup to allow port reuse.</param>
        public AutoDiscoveryServer(IPAddress ipAddress, ushort receivePort, ushort sendPort, byte[] message, bool allowSocketReuse = false)
        {
            this.serverIPEndPoint = new IPEndPoint(ipAddress ?? IPAddress.Any, receivePort);

            this.receivePort = receivePort;
            this.sendPort = sendPort;

            this.responseMessage = message;

            this.allowSocketReuse = allowSocketReuse;

            this.listening = false;
        }

        /// <summary>
        /// Gets a value indicating whether the server is listening for and responding to broadcasts.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server is listening; otherwise, <c>false</c>.
        /// </value>
        public bool IsListening
        {
            get
            {
                return this.listening;
            }
        }

        /// <summary>
        /// Starts listening for client broadcasts.
        /// </summary>
        public void StartListening()
        {
            if (this.listening)
            {
                throw new InvalidOperationException("The Autodiscovery server is already listening");
            }

            this.listening = true;

            this.serverIPEndPoint = new IPEndPoint(IPAddress.Any, receivePort);

            this.udpClient = new UdpClient();

            if (this.allowSocketReuse)
            {
                this.udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            this.udpClient.Client.Bind(this.serverIPEndPoint);

            this.expectedAsyncResult = this.udpClient.BeginReceive(this.OnReceive, null);
        }

        /// <summary>
        /// Stops listening.
        /// </summary>
        public void StopListening()
        {
            if (!this.listening)
            {
                throw new InvalidOperationException("The Autodiscovery server is not listening");
            }

            this.expectedAsyncResult = null;

            this.udpClient.Close();

            this.udpClient.Dispose();

            this.udpClient = null;

            this.listening = false;
        }

        /// <summary>
        /// The callback fired when a broadcast is received by the Udp client.
        /// </summary>
        /// <param name="asyncResult">
        /// Status information on the UdpClient asynchronous receive operation.
        /// </param>
        private void OnReceive(IAsyncResult asyncResult)
        {
            if (this.expectedAsyncResult != null && (this.expectedAsyncResult == asyncResult))
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.None, 0);

                try
                {
                    var bytes = this.udpClient.EndReceive(asyncResult, ref endpoint);

                    endpoint.Port = this.sendPort;

                    var byteStr = Encoding.ASCII.GetString(bytes);

                    this.OnClientMessageReceived?.Invoke(this, new ClientMessageReceivedEventArgs(endpoint, bytes));

                    this.udpClient.Send(this.responseMessage, this.responseMessage.Length, endpoint);

                    this.expectedAsyncResult = this.udpClient.BeginReceive(this.OnReceive, null);
                }
                // When the UdpClient is closed the OnReceive will always fire and cause the following exception to be called
                catch (ObjectDisposedException)
                {
                }
            }
        }
    }
}
