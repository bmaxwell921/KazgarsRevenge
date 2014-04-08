﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class RopeBillboard : HorizontalStretchingBillboard
    {
        public RopeBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creator)
            : base(game, entity, creator, new Vector2(0, 8))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).RopeEffect;
            creatorOffsetRight = 6;
        }
    }
}
