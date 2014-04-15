﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// a billboard that expands and dies at max diameter
    /// </summary>
    public class ExpandingCircleBillboard : DrawableComponentBillboard
    {
        public ExpandingCircleBillboard(KazgarsRevengeGame game, GameEntity entity, Vector3 position)
            : base(game, entity, Vector3.Up, Vector3.Forward, new Vector2(0, 0))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).CircleBlueEffect;
            origin = position;
        }

        protected float maxDiameter = 18;
        protected float increasePerSec = 80;
        public override void Update(GameTime gameTime)
        {
            float add = (float)(increasePerSec * gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            size.X += add;
            size.Y += add;
            if (size.X >= maxDiameter)
            {
                KillComponent();
            }
            base.Update(gameTime);
        }
    }
}
