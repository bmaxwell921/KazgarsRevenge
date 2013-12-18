using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class SnapshotHandler : BaseHandler
    {
        public SnapshotHandler(KazgarsRevengeGame game)
            : base(game)
        {

        }

        /// <summary>
        /// Handles game snapshot messages. This will read the message, then let the player
        /// manager know where everyone is
        /// 
        /// Messages come in like this:
        ///     MessageType - GameSnapshot
        ///     byte - playerId1
        ///     int - position1.x
        ///     int - position1.y
        ///     int - position1.z
        ///     
        /// etc
        /// </summary>
        /// <param name="nim"></param>
        public override void Handle(NetIncomingMessage nim)
        {
            PlayerManager pm = game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            for (int i = 0; i < pm.numPlayers; ++i)
            {
                Identification id = new Identification(nim.ReadByte());
                Vector3 loc = new Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32());
                if (!pm.myId.Equals(id))
                {
                    pm.SetPlayerLocation(loc, id);
                }
            }
        }
    }
}
