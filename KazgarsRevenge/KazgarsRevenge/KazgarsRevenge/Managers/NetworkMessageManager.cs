using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace KazgarsRevenge
{
    class NetworkMessageManager : BaseNetworkMessageManager
    {
        AttackManager attacks;
        PlayerManager players;
        LevelManager levels;
        EnemyManager enemies;

        NetClient Client;

        public IList<ServerInfo> connections;
        public bool isHost;

        public NetworkMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            connections = new List<ServerInfo>();
            isHost = false;
            SetUpClient();
        }

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

        protected override void AddHandlers()
        {
            msgHandlers[NetIncomingMessageType.DiscoveryResponse] = new DiscoveryResponseHandler(Game as KazgarsRevengeGame);
        }

        protected override NetIncomingMessage ReadMessage()
        {
            return Client.ReadMessage();
        }

    }
}
