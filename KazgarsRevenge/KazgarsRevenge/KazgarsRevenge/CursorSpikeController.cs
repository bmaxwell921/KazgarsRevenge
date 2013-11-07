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
        GameEntity entity;
        public CursorSpikeController(MainGame game, AnimationPlayer animations, GameEntity entity)
            : base(game)
        {
            this.entity = entity;
            animations.StartClip(animations.skinningDataValue.AnimationClips["spikes"]);
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
