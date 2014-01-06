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
            ((LoggerManager)game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, "Received a DiscoveryRequest");
            ServerConfig sc = (ServerConfig)game.Services.GetService(typeof(ServerConfig));
            ((SMessageSender)game.Services.GetService(typeof(SMessageSender))).SendDiscoveryResponse(sc.serverName, nim.SenderEndpoint);
        }
    }
}
