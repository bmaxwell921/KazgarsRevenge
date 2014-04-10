﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class DeathTimerController : Component
    {
        double duration;
        public DeathTimerController(KazgarsRevengeGame game, GameEntity entity, double duration)
            : base(game, entity)
        {
            this.duration = duration;
        }

        public override void Update(GameTime gameTime)
        {
            duration -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (duration <= 0)
            {
                Entity.KillEntity();
            }
            base.Update(gameTime);
        }
    }
}
