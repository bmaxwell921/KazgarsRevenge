using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace KazgarsRevenge
{
    public class HostUpdateHandler : BaseHandler
    {
        public HostUpdateHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        // Tells the nmm that there's a new host in town
        public override void Handle(NetIncomingMessage nim)
        {
            NetworkMessageManager nmm = (NetworkMessageManager)game.Services.GetService(typeof(NetworkMessageManager));
            nmm.UpdateHost(nim.ReadInt32());
        }
    }
}
