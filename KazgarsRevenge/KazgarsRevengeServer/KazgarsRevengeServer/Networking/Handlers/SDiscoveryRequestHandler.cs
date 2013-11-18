using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KazgarsRevenge;
using Lidgren.Network;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Handler used when Clients send out a DiscoveryRequest
    /// This handler just sends the Client this server's name
    /// </summary>
    public class SDiscoveryRequestHandler : BaseHandler
    {
        SNetworkingMessageManager nmm;

        public SDiscoveryRequestHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        public override void Handle(NetIncomingMessage nim)
        {
            if (nmm == null)
            {
                nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            }
            Console.WriteLine("Incoming request!");
            NetOutgoingMessage outMsg = nmm.server.CreateMessage();
            outMsg.Write(nmm.DUMMY_NAME);
            nmm.server.SendDiscoveryResponse(outMsg, nim.SenderEndpoint);
        }
    }
}
