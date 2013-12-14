using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class KineticHandler : BaseHandler
    {
        PlayerManager pm;
        int count = 0;

        public KineticHandler(KazgarsRevengeGame game)
            : base(game)
        {

        }

        /// <summary>
        /// Receives the position data from all the other players,
        /// let's the player manager know to update them.
        /// 
        /// Messages come in as:
        ///     byte - MessageType (already taken off)
        ///     byte - playerId
        ///     int32 - x
        ///     int32 - y
        ///     int32 - z
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            if (pm == null)
            {
                pm = game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            }
            Identification id = new Identification(nim.ReadByte());
            Vector3 newLoc = new Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32());

            if (count % 100 == 0)
            {
                //Console.WriteLine("Setting: " + id.id + " to position: " + newLoc);
            }

            pm.SetPlayerLocation(newLoc, id);
        }
    }
}
