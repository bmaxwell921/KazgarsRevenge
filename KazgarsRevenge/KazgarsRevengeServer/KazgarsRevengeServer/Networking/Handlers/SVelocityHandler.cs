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
        public SVelocityHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }
        
        /*
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
            // Just queue up the message to be applied later
            Identification pId = new Identification(nim.ReadInt32());
            Vector3 vel = new Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32());
            MessageQueue mq = game.Services.GetService(typeof(MessageQueue)) as MessageQueue;
            mq.AddMessage(new VelocityMessage(MessageType.InGame_Kinetic, pId, vel));
        }
    }
}
