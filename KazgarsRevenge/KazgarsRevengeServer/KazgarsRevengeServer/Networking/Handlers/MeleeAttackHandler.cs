using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    public class SMeleeAttackHandler : BaseHandler
    {
        public SMeleeAttackHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        /// <summary>
        /// Message comes in as:
        ///     byte - Messagetype ( already read)
        ///     int - creatorId
        ///     int - attackId
        ///     byte - belongingFaction
        ///     int - position.X
        ///     int - position.Y
        ///     int - position.Z
        ///     int - damage
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            // We're gonna send this on thru, just to see what happens
            MeleeAttackMessage mam = new MeleeAttackMessage(MessageType.InGame_Melee, nim.ReadInt32(), nim.ReadInt32(), (FactionType)nim.ReadByte(), new Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32()), nim.ReadInt32());

            // TODO create it here on the server too?

            ((SMessageSender)game.Services.GetService(typeof(SMessageSender))).SendAttackMessage(mam);
        }
    }
}
