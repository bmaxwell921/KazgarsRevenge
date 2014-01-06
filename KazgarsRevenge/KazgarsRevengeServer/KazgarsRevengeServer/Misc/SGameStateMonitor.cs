using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    // Monitors the state of the game. Currently used to detect when no one is connected anymore
    public class SGameStateMonitor : GameComponent
    {
        public SGameStateMonitor(KazgarsRevengeGame game)
            : base(game)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            SNetworkingMessageManager nmm = (SNetworkingMessageManager)Game.Services.GetService(typeof(SNetworkingMessageManager));

            // The only time when it's ok for there to be no connections is when we're in the ServerStart. After that at least one person should be connected
            if (((KazgarsRevengeGame)Game).gameState != GameState.ServerStart && nmm.connectedPlayers == 0)
            {
                ((LoggerManager)Game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, "No more connected clients, transitioning back to ServerStart.");
                ((Server)Game).TransitionBackToServerStart();
            }
        }
    }
}
