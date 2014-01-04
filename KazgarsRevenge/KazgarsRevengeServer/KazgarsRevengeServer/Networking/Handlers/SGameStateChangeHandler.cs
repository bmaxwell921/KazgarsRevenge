using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using KazgarsRevenge;
namespace KazgarsRevengeServer
{
    class SGameStateChangeHandler : BaseHandler
    {
        public SGameStateChangeHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        /*
         * Checks that the player issuing the game state change is host then 
         * sends out game state changes to all the players.
         * 
         * At this point the MessageType has already been read so we only need to read the 
         * playerId and the GameState to move to
         */
        public override void Handle(NetIncomingMessage nim)
        {
            SNetworkingMessageManager nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            
            byte pId = nim.ReadByte();

            if (pId != 0)
            {
                // Lol you wish, not host
                return;
            }

            game.gameState = EnumParser.GetGameState(nim.ReadByte());

            ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.INFO, String.Format("Changing to gameState: {0}", game.gameState));

            this.SendMessages(nmm);
        }

        /*
         * Goes out as
         *      - byte MessageType
         *      - byte GameState
         */ 
        private void SendMessages(SNetworkingMessageManager nmm)
        {
            NetOutgoingMessage msg = nmm.server.CreateMessage();
            GameState state = game.gameState;
            if (game.gameState == GameState.GenerateMap)
            {
                state = GameState.ReceivingMap;
            }
            msg.Write((byte)MessageType.GameStateChange);
            msg.Write((byte)state);

            foreach (NetConnection player in nmm.server.Connections)
            {
                // Use ReliableOrdered because this message has to get there?
                nmm.server.SendMessage(msg, player, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
