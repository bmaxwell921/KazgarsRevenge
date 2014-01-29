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
            NetConnectionStatus status = (NetConnectionStatus)nim.ReadByte();

            // TODO what about other types??
            if (status == NetConnectionStatus.Connected)
            {
                this.HandleConnectedMessage(nim);
            }
            else if (status == NetConnectionStatus.Disconnected)
            {
                this.HandleDisconnectedPlayer(nim);
            }
            else
            {
                ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("Unhandled connection status: {0}", status));
            }

        }

        private void HandleConnectedMessage(NetIncomingMessage nim)
        {
            SPlayerManager playerManager = (SPlayerManager)game.Services.GetService(typeof(SPlayerManager));
            SNetworkingMessageManager nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            ServerConfig sc = (ServerConfig)game.Services.GetService(typeof(ServerConfig));

            if (nmm.connectedPlayers > sc.maxNumPlayers)
            {
                // TODO log this issue. Tell player they can't join? Or should that be sent when they send the discovery? <- this probs
                ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, "Player tried to connect when we have max players already.");
                return;
            }
            
            // If this is the first person to connect, we need to update to the Lobby state
            if (game.gameState == GameState.ServerStart)
            {
                game.gameState = GameState.Lobby;
            }
            Identification newId = playerManager.GetId();
            bool pHost = nmm.isHost(newId);
            ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("New player connected with id of: {0}!", newId));
            ((SMessageSender)game.Services.GetService(typeof(SMessageSender))).SendConnectedMessage(nim.SenderConnection, newId, pHost);
        }

        private void HandleDisconnectedPlayer(NetIncomingMessage nim)
        {
            /*
             * Parse out their id from the message, let the player manager and network manager know they're disconnecting.
             */ 
            int id = Convert.ToInt32(nim.ReadString());
            SPlayerManager pm = (SPlayerManager)game.Services.GetService(typeof(SPlayerManager));
            SNetworkingMessageManager nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            
            ((SMessageSender)game.Services.GetService(typeof(SMessageSender))).SendDisconnectedPlayerMessage(id);
            pm.DisconnectPlayer(id);
            nmm.DisconnectPlayer(id);
        }
    }
}
