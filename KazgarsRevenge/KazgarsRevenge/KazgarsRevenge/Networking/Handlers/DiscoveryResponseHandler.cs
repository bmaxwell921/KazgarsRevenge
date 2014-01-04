using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace KazgarsRevenge
{
    /// <summary>
    /// Just adds a new Connection option to the NetworkingMessageManager
    /// </summary>
    public class DiscoveryResponseHandler : BaseHandler
    {
        public DiscoveryResponseHandler(KazgarsRevengeGame game)
            : base(game)
        {
            
        }

        public override void Handle(NetIncomingMessage nim)
        {
            NetworkMessageManager nmm = (NetworkMessageManager)game.Services.GetService(typeof(NetworkMessageManager));
            LoggerManager lm = (LoggerManager)game.Services.GetService(typeof(LoggerManager));
            lm.Log(Level.DEBUG, "Adding some server info");

            ServerInfo si = new ServerInfo(nim.ReadString(), nim.SenderEndpoint);

            nmm.connections.Add(si);            
        }
    }
}
