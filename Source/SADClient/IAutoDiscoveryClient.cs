using System.Collections;
using System.Collections.Generic;

namespace SADClient
{
    /// <summary>
    /// Describes an interface for an AutoDiscovery client that handles the logic of searching for AutoDiscovery servers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAutoDiscoveryClient<T> where T : ServerInformationBase
    {
        /// <summary>
        /// Gets a value indicating whether the client is currently searching for servers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the client is searching for servers; otherwise, <c>false</c>.
        /// </value>
        bool FindingServers { get; }

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
        IEnumerable<T> FindServers(int timeToReply, byte[] message);
    }
}