using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class RopeBillboard : StretchingBillboard
    {
        public RopeBillboard(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator)
            : base(game, entity, creator, new Vector2(0, 8))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).RopeEffect;
        }
    }
}
