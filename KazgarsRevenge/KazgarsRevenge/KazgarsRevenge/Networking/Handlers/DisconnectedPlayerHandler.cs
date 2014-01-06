using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace KazgarsRevenge
{
    // Used to remove a disconnected client from this client
    public class DisconnectedPlayerHandler : BaseHandler
    {
        public DisconnectedPlayerHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        public override void Handle(NetIncomingMessage nim)
        {
            Identification id = new Identification(nim.ReadByte());
            ((PlayerManager)game.Services.GetService(typeof(PlayerManager))).DeletePlayer(id);
        }
    }
}
