﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class ConnectedHandler : BaseHandler
    {
        PlayerManager pm;
        NetworkMessageManager nmm;

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
        public override void Handle(Lidgren.Network.NetIncomingMessage nim)
        {
            Console.WriteLine("Receiving Connection info from server");
            if (pm == null)
            {
                pm = (PlayerManager)game.Services.GetService(typeof(PlayerManager));
            }
            if (nmm == null)
            {
                nmm = (NetworkMessageManager)game.Services.GetService(typeof(NetworkMessageManager));
            }

            byte clientId = nim.ReadByte();
            bool isHost = nim.ReadBoolean();

            // TODO remove this for a position from the server
            pm.CreateMainPlayer(new Vector3(200, 0 , -200), new Identification(clientId));
            nmm.isHost = isHost;
        }
    }
}