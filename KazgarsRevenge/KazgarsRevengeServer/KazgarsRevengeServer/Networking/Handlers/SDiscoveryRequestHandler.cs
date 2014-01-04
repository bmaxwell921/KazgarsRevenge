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
        public SDiscoveryRequestHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        public override void Handle(NetIncomingMessage nim)
        {
            // TODO send back if we have room for any connections?
            Console.WriteLine("Received a DiscoveryRequest");
            SNetworkingMessageManager nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            ServerConfig sc = (ServerConfig)game.Services.GetService(typeof(ServerConfig));

            NetOutgoingMessage outMsg = nmm.server.CreateMessage();
            outMsg.Write(sc.serverName);
            nmm.server.SendDiscoveryResponse(outMsg, nim.SenderEndpoint);
        }
    }
}
