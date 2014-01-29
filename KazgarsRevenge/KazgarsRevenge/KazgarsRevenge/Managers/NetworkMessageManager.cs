using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace KazgarsRevenge
{
    public class NetworkMessageManager : BaseNetworkMessageManager
    {
        AttackManager attacks;
        PlayerManager players;
        LevelManager levels;
        EnemyManager enemies;

        public NetClient Client;

        public IList<ServerInfo> connections;
        public bool isHost;

        public NetworkMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            connections = new List<ServerInfo>();
            isHost = false;
            SetUpClient();
        }

        // Lidgren initialization
        private void SetUpClient()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Constants.CONNECTION_KEY);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

            Client = new NetClient(config);
            Client.Start();
        }

        public override void Initialize()
        {
            Client.DiscoverLocalPeers(Constants.PORT);

            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            players = Game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            levels = Game.Services.GetService(typeof(LevelManager)) as LevelManager;
            enemies = Game.Services.GetService(typeof(EnemyManager)) as EnemyManager;
        }

        #region Pregame messaging
        public void ConnectTo(int connIndex)
        {
            Client.Connect(connections[connIndex].ServerEndpoint);
        }

        protected override void AddHandlers()
        {
            msgHandlers[NetIncomingMessageType.DiscoveryResponse] = new DiscoveryResponseHandler(Game as KazgarsRevengeGame);
            msgHandlers[NetIncomingMessageType.Data] = new DataMessageHandler(Game as KazgarsRevengeGame);
        }

        protected override NetIncomingMessage ReadMessage()
        {
            return Client.ReadMessage();
        }

        #endregion
        
        public override void HandleMessages()
        {
            base.HandleMessages();
        }

        public void CloseConnection()
        {
            // Send this client id so the server knows who to disconnect
            String form = String.Format("{0}", players.myId.id);
            Console.WriteLine(form);
            Client.Shutdown(String.Format("{0}", players.myId.id));
        }

        // For now this is just so the current client can know if it's host or not
        public void UpdateHost(int newHost)
        {
            if (players.myId.id == newHost)
            {
                ((LoggerManager)Game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("I, id: {0}, am host, bitches!!!", players.myId));
                this.isHost = true;
            }
        }
    }
}
