using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace KazgarsRevenge
{
    class ConnectedHandler : BaseHandler
    {
        public ConnectedHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        /// <summary>
        /// Only thing left in the message at this point should be
        ///        byte newId
        ///        bool isHost
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            PlayerManager pm = (PlayerManager)game.Services.GetService(typeof(PlayerManager));
            NetworkMessageManager nmm = (NetworkMessageManager)game.Services.GetService(typeof(NetworkMessageManager));
            LoggerManager lm = (LoggerManager)game.Services.GetService(typeof(LoggerManager));
            lm.Log(Level.DEBUG, "Receiving Connection info from server");

            int clientId = nim.ReadInt32();
            bool isHost = nim.ReadBoolean();

            // TODO remove this for a position from the server
            pm.CreateMainPlayer(new Vector3(200, 0, -200), new Identification(clientId, clientId));
            nmm.isHost = isHost;
        }
    }
}
