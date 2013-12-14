using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace KazgarsRevenge
{
    public class KineticHandler : BaseHandler
    {
        PlayerManager pm;

        public KineticHandler(KazgarsRevengeGame game)
            : base(game)
        {

        }

        /// <summary>
        /// Receives the position data from all the other players,
        /// let's the player manager know to update them
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            
        }
    }
}
