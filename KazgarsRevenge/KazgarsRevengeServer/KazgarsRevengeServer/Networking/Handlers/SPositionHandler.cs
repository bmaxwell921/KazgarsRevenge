using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    class SPositionHandler : BaseHandler
    {
        SPlayerManager playerManager;
        SNetworkingMessageManager nmm;
        public SPositionHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }
        
        /*
         * TODO: For now just accept the player's movement request and pass to everyone else
         * Messages come in as
         *      - byte MessageType -> has already been read
         *      - byte playerId
         *      - int32 xVel
         *      - int32 yVel
         *      - int32 zVel
         *     
         * The last 3 int32s are VELOCITY REQUESTS sent from clients
         */
        public override void Handle(NetIncomingMessage nim)
        {
            if (nmm == null)
            {
                nmm = (SNetworkingMessageManager)game.Services.GetService(typeof(SNetworkingMessageManager));
            }
            if (playerManager == null)
            {
                playerManager = (SPlayerManager)game.Services.GetService(typeof(SPlayerManager));
            }
            Identification pId = new Identification(nim.ReadByte());
            Vector3 vel = new Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32());
            Vector3 curPos = playerManager.GetPlayerPosition(pId);
            playerManager.UpdatePlayerPosition(pId, curPos + vel);

            SendMessages(pId, curPos + vel);
        }

        /*
         * Outgoing Message Format:
         *      - byte MessageType
         *      - byte playerId
         *      - int32 x
         *      - int32 y
         *      - int32 z
         *      
         *  Last 3 int32s are position values since the clients are told by the server where things are
         */ 
        private void SendMessages(Identification pId, Vector3 loc)
        {
            NetOutgoingMessage nom = nmm.server.CreateMessage();
            nom.Write((byte)MessageType.InGame_Kinetic);
            nom.Write(pId.id);
            nom.Write(loc.X);
            nom.Write(loc.Y);
            nom.Write(loc.Z);

            foreach (NetConnection player in nmm.server.Connections)
            {
                // Meh, hopefully it gets there
                nmm.server.SendMessage(nom, player, NetDeliveryMethod.Unreliable);
            }
        }
    }
}
