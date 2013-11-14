using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class CursorSpikeController : Component
    {
        double millisCreated = 0;
        double millisLife = 800;
        public CursorSpikeController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            AnimationPlayer anims = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            anims.StartClip("Default Take");
        }

        public override void Update(GameTime gameTime)
        {
            millisCreated += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (millisCreated >= millisLife)
            {
                entity.Kill();
            }
        }
    }
}
