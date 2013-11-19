using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class ReceivingMapHandler : BaseHandler
    {
        LevelManager lm;
        public ReceivingMapHandler(KazgarsRevengeGame game)
            : base(game)
        {

        }

        // Reads level bytes into a string, passes off to level manager
        public override void Handle(Lidgren.Network.NetIncomingMessage nim)
        {
            Console.WriteLine("Receiving map data from server");
            if (lm == null)
            {
                lm = game.Services.GetService(typeof(LevelManager)) as LevelManager;
            }
            // TODO make this read ALL BYTES once we actually do level generation
            byte[] mapData = nim.ReadBytes(1);
            string map = "";
            foreach (byte b in mapData)
            {
                map += b + ",";
            }

            lm.CreateMapFrom(map);
        }
    }
}
