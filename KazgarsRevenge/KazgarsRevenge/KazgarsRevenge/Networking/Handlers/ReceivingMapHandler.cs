using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class ReceivingMapHandler : BaseHandler
    {
        LevelManager lm;
        PlayerManager pm;
        public ReceivingMapHandler(KazgarsRevengeGame game)
            : base(game)
        {

        }

        // Reads level bytes into a string, passes off to level manager
        // TODO player starting locations should also be here
        public override void Handle(Lidgren.Network.NetIncomingMessage nim)
        {
            Console.WriteLine("Receiving map data from server");
            if (lm == null)
            {
                lm = game.Services.GetService(typeof(LevelManager)) as LevelManager;
            }
            if (pm == null)
            {
                pm = game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            }
            
            // game width and height are the width and height of each level
            byte[] mapData = nim.ReadBytes(game.levelWidth * game.levelHeight);

            lm.CreateMapFrom(mapData);

            int numPlayers = nim.ReadByte();

            // Get the actual player location data
            for (int i = 0; i < numPlayers; ++i)
            {
                byte id = nim.ReadByte();
                Vector3 position = new Vector3(nim.ReadFloat(), nim.ReadFloat(), nim.ReadFloat());

                pm.SetPlayerLocation(position, new Identification(id));
            }
        }
    }
}
