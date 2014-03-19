using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class ChainBillboard : StretchingBillboard
    {
        public ChainBillboard(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator)
            : base(game, entity, creator, new Vector2(0, 4))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).ChainEffect;

            maxSize = 200;
            offsetRight = 6;
        }
    }
}
