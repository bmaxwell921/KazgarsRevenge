using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;
using System.Threading;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Handler used by the server to establish a connection to a client
    /// </summary>
    public class SStatusChangeHandler : BaseHandler
    {
        public SStatusChangeHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }
        /// <summary>
        /// Receives the new connection request, connects server and client, adds the player to the game, and sends response message
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            SPlayerManager playerManager = (SPlayerManager)game.Services.GetService(typeof(SPlayerManager));
            SNetworkingMessageManager nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            ServerConfig sc = (ServerConfig)game.Services.GetService(typeof(ServerConfig));

            NetConnectionStatus status = (NetConnectionStatus)nim.ReadByte();

            // TODO what about other types??
            if (status == NetConnectionStatus.Connected)
            {
                ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.INFO, "New player connecting!");
                if (nmm.connectedPlayers > sc.maxNumPlayers)
                {
                    // TODO log this issue. Tell player they can't join? Or should that be sent when they send the discovery? <- this probs
                    return;
                }
                bool pHost = isHost(nmm);

                ++nmm.connectedPlayers;

                /*
                 * Response message looks like this:
                 *      byte: playerId
                 *      bool: isHost
                 */
                Identification newId = playerManager.GetId();
                NetOutgoingMessage outMsg = nmm.server.CreateMessage();
                outMsg.Write((byte)MessageType.Connected);
                outMsg.Write(newId.id);
                outMsg.Write(pHost);

                // Tell the new connection who they are in the game
                nmm.server.SendMessage(outMsg, nim.SenderConnection, NetDeliveryMethod.ReliableOrdered);
            }

        }

        private bool isHost(SNetworkingMessageManager nmm)
        {
            return nmm.connectedPlayers == 0;
        }
    }
}
