﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class AbilityTargetDecal : DrawableComponentBillboard
    {
        public AbilityTargetDecal(KazgarsRevengeGame game, GameEntity entity, float size)
            : base(game, entity, Vector3.Up, Vector3.Forward, new Vector2(size, size))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).GroundTargetEffect;
        }

        public void UpdateMouseLocation(Vector3 position, bool draw, float size)
        {
            origin = new Vector3(position.X, -18.4f, position.Z);
            Visible = draw;
            this.size.X = size;
            this.size.Y = size;
        }
    }
}