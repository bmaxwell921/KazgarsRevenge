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
        public int connectedPlayers
        {
            get
            {
                return server.ConnectionsCount;
            }
        }

        public Identification hostId;

        public SNetworkingMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            SetUpNetServer();

            hostId = new Identification(0, 0);

            nextUpdateTime = NetTime.Now;
        }

        // Sets up specific config details
        private void SetUpNetServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Constants.CONNECTION_KEY);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.Port = Constants.PORT;

            config.MaximumConnections = ((ServerConfig)Game.Services.GetService(typeof(ServerConfig))).maxNumPlayers;
            config.ConnectionTimeout = 2000f; // 2 seconds?
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

        public bool isHost(Identification id)
        {
            return id.Equals(hostId);
        }

        public void DisconnectPlayer(int id)
        {
            if (hostId.Equals(new Identification(id, id)))
            {
                // Choose a new host
                SPlayerManager pm = (SPlayerManager)Game.Services.GetService(typeof(SPlayerManager));

                // Just get the lowest id...
                hostId = pm.GetLowestId();

                if (hostId == null)
                {
                    // Everyone already disconnected so we'll be going back to the start state 
                    return;
                }
                // Tell everyone about it
                ((SMessageSender)Game.Services.GetService(typeof(SMessageSender))).SendNewHost(hostId);
            }
        }
    }
}
