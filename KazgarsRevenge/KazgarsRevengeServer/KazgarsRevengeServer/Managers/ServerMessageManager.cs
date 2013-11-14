using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Lidgren.Network;
using KazgarsRevenge;

namespace KazgarsRevengeServer.Managers
{
    class ServerMessageManager : GameComponent
    {
        public static readonly Boolean DEBUGGING = true;

        // Server from Lidgren
        NetServer server;

        // When to send info to connected clients
        double nextUpdateTime;

        // number of people connected, must be < MAX_NUM_CONNECTIONS
        int connectedPlayers;

        //Dictionary<NetIncomingMessageType, BaseParser> messageParsers;

        public ServerMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            SetUpNetServer();
            AddMessageParsers();

            // Server starts in the lobby state since it's awaiting connections
            ((KazgarsRevengeGame)Game).gameState = GameState.Lobby;

            connectedPlayers = 0;
            nextUpdateTime = NetTime.Now;
        }

        private void SetUpNetServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Constants.CONNECTION_KEY);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.Port = Constants.PORT;

            config.MaximumConnections = Constants.MAX_NUM_CONNECTIONS;

            server = new NetServer(config);
            server.Start();
        }

        private void AddMessageParsers()
        {

        }

        public override void Update(GameTime gameTime)
        {
            ReadMessages();
            base.Update(gameTime);
        }

        private void ReadMessages()
        {
            NetIncomingMessage msg;

            while ((msg = server.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.DiscoveryRequest)
                {
                    HandleNewConnection(msg);
                    continue;
                }
            }
        }

        // Special case for connecting players??
        private void HandleNewConnection(NetIncomingMessage msg)
        {
            if (connectedPlayers >= Constants.MAX_NUM_CONNECTIONS)
            {
                // TODO sophisticated logging?
                Console.WriteLine("DEBUG:Someone tried to connect to the server when we already have %d connections.", connectedPlayers);
                return;
            }

            if (DEBUGGING)
            {
                Console.WriteLine("Someone new is connecting from: " + msg.ReadIPEndpoint());
            }
        }
    }
}
