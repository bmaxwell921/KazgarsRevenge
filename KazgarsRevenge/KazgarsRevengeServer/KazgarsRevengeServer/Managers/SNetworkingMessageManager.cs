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

            config.MaximumConnections = ((ServerConfig)Game.Services.GetService(typeof(ServerConfig))).maxNumPlayers;

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
    }
}
