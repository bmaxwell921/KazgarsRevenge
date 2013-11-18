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
        NetworkMessageManager nmm;
        public DiscoveryResponseHandler(KazgarsRevengeGame game)
            : base(game)
        {
            nmm = (NetworkMessageManager) game.Services.GetService(typeof(NetworkMessageManager));
        }

        public override void Handle(NetIncomingMessage nim)
        {
            ServerInfo si = new ServerInfo(nim.ReadString(), nim.SenderEndpoint);

            nmm.connections.Add(si);            
        }
    }
}
