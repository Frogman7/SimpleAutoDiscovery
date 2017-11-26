using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SADClient
{
    /// <summary>
    /// Describes an interface for an AutoDiscovery client that handles the logic of searching for AutoDiscovery servers.
    /// </summary>
    /// <typeparam name="T">
    /// The ServerInformationBase implementation.
    /// </typeparam>
    public interface IAutoDiscoveryClient<T> where T : ServerInformation
    {
        /// <summary>
        /// Occurs when a server is found.
        /// </summary>
        event EventHandler<T> ServerFound;

        /// <summary>
        /// Gets a value indicating whether the client is currently searching for servers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the client is searching for servers; otherwise, <c>false</c>.
        /// </value>
        bool FindingServers { get; }

        /// <summary>
        /// Searches for servers asynchronously.
        /// </summary>
        /// <param name="timeToReply">
        /// The time to wait in milliseconds for broadcast replies.
        /// </param>
        /// <param name="message">The message to send the server.</param>
        /// <returns>
        /// A collection of found server informations.
        /// </returns>
        Task<IEnumerable<T>> FindServersAsync(int timeToReply, byte[] message);
    }
}