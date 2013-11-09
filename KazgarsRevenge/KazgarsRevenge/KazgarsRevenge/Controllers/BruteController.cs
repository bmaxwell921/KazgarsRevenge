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
    class BruteController : AIController
    {
        GameEntity entity;
        HealthData health;
        Entity physicalData;
        AnimationPlayer animations;
        public BruteController(MainGame game, GameEntity bruteEntity, HealthData bruteHealth, Entity physicalData, AnimationPlayer bruteAnimations)
            : base(game)
        {
            this.entity = bruteEntity;
            this.health = bruteHealth;
            this.physicalData = physicalData;
            this.animations = bruteAnimations;
            PlayAnimation("pig_idle", false);
        }

        public override void PlayHit()
        {
            PlayAnimation("pig_hit", true);
        }

        public void PlayAnimation(string animationName, bool mix)
        {
            AnimationClip clip = animations.skinningDataValue.AnimationClips[animationName];
            animations.StartClip(clip, mix);
        }
        public override void Update(GameTime gameTime)
        {
            if (health.Dead)
            {
                entity.Kill();
            }
        }
    }
}
