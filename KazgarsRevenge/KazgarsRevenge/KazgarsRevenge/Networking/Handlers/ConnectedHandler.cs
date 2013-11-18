using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge.Networking.Handlers
{
    class ConnectedHandler : BaseHandler
    {
        PlayerManager pm;
        NetworkMessageManager nmm;

        public ConnectedHandler(KazgarsRevengeGame game)
            : base(game)
        {
            nmm = (NetworkMessageManager)game.Services.GetService(typeof(NetworkMessageManager));
            pm = (PlayerManager)game.Services.GetService(typeof(PlayerManager));
        }

        /// <summary>
        /// Only thing left in the message at this point should be
        ///        byte newId
        ///        bool isHost
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(Lidgren.Network.NetIncomingMessage nim)
        {
            // TODO remove this for one from the server
            Vector3 addLoc = new Vector3(200, 0, -200);

            byte clientId = nim.ReadByte();
            bool isHost = nim.ReadBoolean();

            pm.CreateMainPlayer(addLoc, new Identification(clientId));
            nmm.isHost = isHost;
        }
    }
}
