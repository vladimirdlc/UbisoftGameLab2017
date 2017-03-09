using System;
using System.Collections.Generic;

namespace Dissonance.Networking.Server
{
    internal interface IServer<TPeer>
    {
        /// <summary>
        /// Get all peers in the given room. Ensures that peers are never added to the list twice
        /// </summary>
        /// <param name="room"></param>
        /// <param name="recipientsBuffer"></param>
        /// <returns></returns>
        int GetConnectionsInRoom(ushort room, List<TPeer> recipientsBuffer);

        /// <summary>
        /// Try to get the connection to the given player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        bool GetConnectionToPlayer(ushort player, out TPeer connection);

        /// <summary>
        /// Send an unreliable network message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        void SendUnreliable(TPeer connection, ArraySegment<byte> packet);

        /// <summary>
        /// Send a reliable network message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        void SendReliable(TPeer connection, ArraySegment<byte> packet);

        /// <summary>
        /// Invoked when a new client has been added
        /// </summary>
        /// <param name="source"></param>
        /// <param name="client"></param>
        void AddClient(TPeer source, ClientInfo client);
    }
}
