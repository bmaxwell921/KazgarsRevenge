using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class PillarBeamBillboard : HorizontalStretchingBillboard
    {
        public PillarBeamBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creator, bool right)
            :base(game, entity, creator, new Vector2(0, 250))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).PillarBeamEffect;
            maxSize = 250;

            if (right)
            {
                followOffsetRight = 50;
            }
            else
            {
                followOffsetRight = -50;
            }
        }
    }
}
