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
            
            int pId = nim.ReadInt32();

            if (!nmm.isHost(new Identification(pId)))
            {
                // Lol you wish, not host
                ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("Not host tried to send a gamestate change. HostId: {0}, SenderId: {1}", nmm.hostId, pId));
                return;
            }

            game.gameState = (GameState) Enum.ToObject(typeof(GameState), nim.ReadByte());

            ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("Changing to gameState: {0}", game.gameState));

            this.SendMessages(nmm);
        }

        /*
         * Goes out as
         *      - byte MessageType
         *      - byte GameState
         */ 
        private void SendMessages(SNetworkingMessageManager nmm)
        {
            GameState state = game.gameState;
            if (game.gameState == GameState.GenerateMap)
            {
                state = GameState.ReceivingMap;
            }
            ((SMessageSender)game.Services.GetService(typeof(SMessageSender))).SendGameStateChange(state);
        }
    }
}
