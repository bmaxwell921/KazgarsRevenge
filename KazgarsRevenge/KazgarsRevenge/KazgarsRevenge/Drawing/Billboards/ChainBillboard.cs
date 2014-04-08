using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class ChainBillboard : HorizontalStretchingBillboard
    {
        public ChainBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creator)
            : base(game, entity, creator, new Vector2(0, 4))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).ChainEffect;

            maxSize = 200;
            creatorOffsetRight = 6;
        }
    }
}
