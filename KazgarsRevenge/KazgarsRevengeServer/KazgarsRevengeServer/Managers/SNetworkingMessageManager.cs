using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Lidgren.Network;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    class SNetworkingMessageManager : GameComponent
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

        // Dictionary of objects used to handle incoming messages
        Dictionary<NetIncomingMessageType, BaseHandler> msgHandlers;

        public SNetworkingMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            msgHandlers = new Dictionary<NetIncomingMessageType, BaseHandler>();
            SetUpNetServer();
            AddHandlers();

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

        private void AddHandlers()
        {
            msgHandlers[NetIncomingMessageType.DiscoveryRequest] = new ConnectionHandler(Game as KazgarsRevengeGame);
        }

        public override void Update(GameTime gameTime)
        {
            HandleMessages();
            base.Update(gameTime);
        }

        // Fairly simple, we just let the appropriate handler handle each message
        private void HandleMessages()
        {
            NetIncomingMessage msg;

            while ((msg = server.ReadMessage()) != null)
            {
                BaseHandler handler;
                msgHandlers.TryGetValue(msg.MessageType, out handler);

                if (handler == null)
                {
                    Console.WriteLine("ERROR: OH GOD MESSAGE TYPE WE DON'T HANDLE!" + msg.MessageType);
                    continue;
                }

                handler.Handle(msg);
            }
        }
    }
}
