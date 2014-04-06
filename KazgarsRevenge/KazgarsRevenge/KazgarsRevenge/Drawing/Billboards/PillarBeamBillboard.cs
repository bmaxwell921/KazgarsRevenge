using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class PillarBeamBillboard : StretchingBillboard
    {
        public PillarBeamBillboard(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator)
            :base(game, entity, creator, new Vector2(0, 20))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).PillarBeamEffect;
            maxSize = 400;
        }
    }
}
