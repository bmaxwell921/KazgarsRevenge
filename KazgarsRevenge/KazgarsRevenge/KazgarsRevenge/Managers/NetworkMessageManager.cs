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

        public void StartGame()
        {
            // TODO make sure we're connected to something???
            NetOutgoingMessage nom = Client.CreateMessage();
            nom.Write((byte)MessageType.GameStateChange);
            nom.Write(players.myId.id);
            nom.Write((byte)GameState.GenerateMap);
            Client.SendMessage(nom, NetDeliveryMethod.ReliableOrdered);
        }

        #endregion
        
        public override void HandleMessages()
        {
            base.HandleMessages();
        }

        #region In Game Messaging
        
        /// <summary>
        /// Sends a message to the server about this client's velocity
        /// Messages are of the form:
        ///     byte - MessageType
        ///     byte - PlayerId
        ///     int32 - xVel
        ///     int32 - yVel
        ///     int32 - zVel
        /// </summary>
        /// <param name="vel"></param>
        public void SendVelocityMessage(Vector3 vel)
        {
            NetOutgoingMessage nom = Client.CreateMessage();
            nom.Write((byte)MessageType.InGame_Kinetic);
            nom.Write((byte)this.players.myId.id);

            nom.Write((int)vel.X);
            nom.Write((int)vel.Y);
            nom.Write((int)vel.Z);

            // If it gets there that's good, if not meh
            Client.SendMessage(nom, NetDeliveryMethod.Unreliable);
        }
        #endregion

        public void CloseConnection()
        {
            // Send this client id so the server knows who to disconnect
            Client.Shutdown(String.Format("{0}", players.myId.id));
        }

        // For now this is just so the current client can know if it's host or not
        public void UpdateHost(byte newHost)
        {
            if (players.myId.id == newHost)
            {
                ((LoggerManager)Game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("I, id: {0}, am host, bitches!!!", players.myId));
                this.isHost = true;
            }
        }
    }
}
