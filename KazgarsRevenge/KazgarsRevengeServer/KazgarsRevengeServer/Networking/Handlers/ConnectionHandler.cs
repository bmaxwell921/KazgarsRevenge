using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Handler used by the server to establish a connection to a client
    /// </summary>
    public class ConnectionHandler : BaseHandler
    {
        SPlayerManager playerManager;
        SNetworkingMessageManager nmm;

        public ConnectionHandler(KazgarsRevengeGame game)
            : base(game)
        {
            playerManager = (SPlayerManager) game.Services.GetService(typeof(SPlayerManager));
            nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
        }
        /// <summary>
        /// Receives the new connection request, adds the player to the game, and sends response message
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            if (nmm.connectedPlayers > Constants.MAX_NUM_CONNECTIONS)
            {
                // TODO log this issue
                return;
            }
            bool pHost = isHost();

            ++nmm.connectedPlayers;

            // TODO do I need to update the game state here?

            /*
             * Response message looks like this:
             *      byte: playerId
             *      bool: isHost
             */ 
            Identification newId = playerManager.createNewPlayer();
            NetOutgoingMessage outMsg = nmm.server.CreateMessage();
            outMsg.Write(newId.id);
            outMsg.Write(pHost);
            nmm.server.SendDiscoveryResponse(outMsg, nim.SenderEndpoint);
        }

        private bool isHost()
        {
            return nmm.connectedPlayers == 0;
        }
    }
}
