using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class GameStateChangeHandler : BaseHandler
    {
        public GameStateChangeHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        public override void Handle(Lidgren.Network.NetIncomingMessage nim)
        {
            game.gameState = (GameState) Enum.ToObject(typeof(GameState), nim.ReadByte());
        }
    }
}
