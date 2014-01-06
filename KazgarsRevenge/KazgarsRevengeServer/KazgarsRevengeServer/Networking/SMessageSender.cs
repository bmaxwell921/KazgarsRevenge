using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using KazgarsRevenge;
using System.Net;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Convenience class to consolidate message sending
    /// </summary>
    public class SMessageSender
    {
        // Server used to send messages
        private NetServer server;

        // Logger
        private LoggerManager lm;

        public SMessageSender(NetServer server, LoggerManager lm)
        {
            this.server = server;
            this.lm = lm;
        }

        /*
         * Sends a discovery response to recipient
         * 
         * Message looks like:
         *      string - serverName
         */ 
        public void SendDiscoveryResponse(string serverName, IPEndPoint recipient)
        {
            lm.Log(Level.DEBUG, String.Format("Sending discovery response to: {0}", recipient));
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write(serverName);
            server.SendDiscoveryResponse(outMsg, recipient);
        }

        /* 
         * Sends the new game state to all connections
         * 
         * Message looks like:
         *      byte - MessageType
         *      byte - GameState
         */ 
        public void SendGameStateChange(GameState newState)
        {
            NetOutgoingMessage nom = server.CreateMessage();
            nom.Write((byte)MessageType.GameStateChange);
            nom.Write((byte)newState);

            this.SendMessageToAll(nom, NetDeliveryMethod.ReliableOrdered);
        }

        /*
         * Sends a message back to the client that they're connected
         * 
         * Message looks like:
         *      byte - MessageType
         *      byte - id
         *      bool - isHost
         */ 
        public void SendConnectedMessage(NetConnection connection, Identification id, bool isHost)
        {
            NetOutgoingMessage nom = server.CreateMessage();
            nom.Write((byte)MessageType.Connected);
            nom.Write(id.id);
            nom.Write(isHost);

            server.SendMessage(nom, connection, NetDeliveryMethod.ReliableOrdered);
        }

        /*
         * Sends the layout of the map to all clients
         * 
         * If this is what our map looks like:
         *      [ 5 | 3 | 2 ]
         *      [ 2 | 1 | 3 ]
         *      [ 4 | 1 | 4 ]
         * And playerPosMap looks like this:
         *      0 -> (0, 0, 0)
         *      1 -> (10, 0, 0)
         * The the message has this form:
         *      1) Map data
         *      2) Number of players
         *      3) Pairs of <playerId, position>
         *      
         * So with the given information we have a message like this:
         *      5, 3, 2, 2, 1, 3, 4, 1, 4       (This is the map data)
         *      2                               (Number of players)
         *      0, 0, 0, 0                      (Player 0's position)
         *      1, 10, 0, 0                     (Player 1's position)
         * 
         * 
         */
        public void SendMapData(byte[] chunkIds, IDictionary<Identification, Vector3> playerPosMap)
        {
            NetOutgoingMessage nom = server.CreateMessage();
            nom.Write((byte)MessageType.MapData);

            // Send the actual map data
            foreach (byte chunk in chunkIds)
            {
                nom.Write(chunk);
            }

            // number of players
            nom.Write((byte)playerPosMap.Keys.Count);

            // Then send where everyone is
            foreach (Identification playerId in playerPosMap.Keys)
            {
                nom.Write(playerId.id);
                Vector3 pos = playerPosMap[playerId];
                nom.Write(pos.X);
                nom.Write(pos.Y);
                nom.Write(pos.Z);
            }

            this.SendMessageToAll(nom, NetDeliveryMethod.ReliableOrdered);
        }

        /*
         * Send location of everything to clients - right now, just all the player locations
         * 
         * Game Snapshot looks like:
         *      MessageType - GameSnapshot
         *      byte - playerId1
         *      int - player1.x
         *      int - player1.y
         *      int - player1.z
         */
        public void SendGameSnapshot(SPlayerManager pm)
        {
            NetOutgoingMessage nom = server.CreateMessage();
            nom.Write((byte)MessageType.GameSnapshot);

            foreach (Identification id in pm.players.Keys)
            {
                Vector3 loc = pm.GetPlayerPosition(id);
                nom.Write(id.id);
                nom.Write((int)loc.X);
                nom.Write((int)loc.Y);
                nom.Write((int)loc.Z);
            }

            this.SendMessageToAll(nom, NetDeliveryMethod.ReliableOrdered);
        }

        /*
         * Sends the new host id to all clients. This is a bit of a hack since we only need to send this to the client becoming host,
         * but their connection isn't saved anywhere. So I decided to send it to everyone
         */ 
        public void SendNewHost(Identification hostId)
        {
            NetOutgoingMessage nom = server.CreateMessage();
            nom.Write((byte)MessageType.HostUpdate);
            nom.Write(hostId.id);
            this.SendMessageToAll(nom, NetDeliveryMethod.ReliableOrdered);
        }

        // Let all the clients know that someone disconnected
        public void SendDisconnectedPlayerMessage(byte id)
        {
            NetOutgoingMessage nom = server.CreateMessage();
            nom.Write((byte)MessageType.DisconnectedPlayer);
            nom.Write(id);
            this.SendMessageToAll(nom, NetDeliveryMethod.ReliableOrdered);
        }

        // Sends the message to all connections with the given delivery method
        private void SendMessageToAll(NetOutgoingMessage nom, NetDeliveryMethod delivMethod)
        {
            foreach (NetConnection nc in server.Connections)
            {
                if (!nom.IsSent)
                {
                    server.SendMessage(nom, nc, delivMethod);
                }
            }
        }
    }
}
