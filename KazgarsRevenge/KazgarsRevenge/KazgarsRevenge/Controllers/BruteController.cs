using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class BruteController : DrawableComponent2D
    {
        GameEntity entity;
        HealthComponent health;
        Entity physicalData;
        AnimationPlayer animations;
        public BruteController(MainGame game, GameEntity bruteEntity, HealthComponent bruteHealth, Entity physicalData, AnimationPlayer bruteAnimations)
            : base(game)
        {
            this.entity = bruteEntity;
            this.health = bruteHealth;
            this.physicalData = physicalData;
            this.animations = bruteAnimations;
            PlayAnimation("pig_idle");
        }

        public void PlayAnimation(string animationName)
        {
            AnimationClip clip = animations.skinningDataValue.AnimationClips[animationName];
            animations.StartClip(clip);
        }
        public override void Update(GameTime gameTime)
        {
            if (health.Dead)
            {
                entity.Kill();
            }
        }

        public override void Draw(SpriteBatch s)
        {

        }
    }
}
