using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Lidgren.Network;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    class SNetworkingMessageManager : BaseNetworkMessageManager
    {
        public string DUMMY_NAME = "Server Name";
        public static readonly Boolean DEBUGGING = true;

        // Server from Lidgren
        public NetServer server
        {
            get;
            protected set;
        }

        // When to send info to connected clients
        public double nextUpdateTime
        {
            get;
            protected set;
        }

        // number of people connected, must be < MAX_NUM_CONNECTIONS
        public int connectedPlayers;

        public SNetworkingMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            SetUpNetServer();

            // Server starts in the lobby state since it's awaiting connections
            ((KazgarsRevengeGame)Game).gameState = GameState.Lobby;

            connectedPlayers = 0;
            nextUpdateTime = NetTime.Now;
        }

        // Sets up specific config details
        private void SetUpNetServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Constants.CONNECTION_KEY);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.Port = Constants.PORT;

            config.MaximumConnections = Constants.MAX_NUM_CONNECTIONS;

            server = new NetServer(config);
            server.Start();
        }

        protected override void AddHandlers()
        {
            msgHandlers[NetIncomingMessageType.DiscoveryRequest] = new SDiscoveryRequestHandler(Game as KazgarsRevengeGame);
            msgHandlers[NetIncomingMessageType.StatusChanged] = new SStatusChangeHandler(Game as KazgarsRevengeGame);
            msgHandlers[NetIncomingMessageType.Data] = new SDataMessageHandler(Game as KazgarsRevengeGame);
        }

        protected override NetIncomingMessage ReadMessage()
        {
            return server.ReadMessage();
        }

        /*
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
        public void SendLevel(byte[] chunkIds, IDictionary<Identification, Vector3> playerPosMap)
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

            // Tell everyone the info!
            foreach (NetConnection player in server.Connections)
            {
                server.SendMessage(nom, player, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
