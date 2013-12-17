using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Since all in game data is sent as Lidgren.MessageType.Data this Handler is a composition of other Handlers
    /// to handle our MessageTypes
    /// </summary>
 
    public class SDataMessageHandler : BaseDataMessageHandler
    {

        public SDataMessageHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        protected override void AddHandlers()
        {
            handlers[MessageType.GameStateChange] = new SGameStateChangeHandler(game);
            handlers[MessageType.InGame_Kinetic] = new SVelocityHandler(game);
        }
    }
}
