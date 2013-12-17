using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    class SVelocityHandler : BaseHandler
    {
        SPlayerManager playerManager;
        SNetworkingMessageManager nmm;
        public SVelocityHandler(KazgarsRevengeGame game)
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

            // Just queue up the message to be applied later
            Identification pId = new Identification(nim.ReadByte());
            Vector3 vel = new Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32());
            MessageQueue mq = game.Services.GetService(typeof(MessageQueue)) as MessageQueue;
            mq.AddMessage(new VelocityMessage(MessageType.InGame_Kinetic, pId, vel));

            //if (count % 100 == 0)
            //{
            //    Console.WriteLine("Received velocity of: " + vel);
            //}
            //Vector3 curPos = playerManager.GetPlayerPosition(pId);
            //if (curPos.Y < 0)
            //{
            //    curPos.Y = 0;
            //}

            //playerManager.SetPlayerLocation(curPos + vel, pId);

            //SendMessages(pId, curPos + vel);
        }

        int count = 0;

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
            nom.Write((int)loc.X);
            nom.Write((int)loc.Y);
            nom.Write((int)loc.Z);

            foreach (NetConnection player in nmm.server.Connections)
            {
                // Meh, hopefully it gets there
                nmm.server.SendMessage(nom, player, NetDeliveryMethod.Unreliable);
            }
        }
    }
}
