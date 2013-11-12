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
        HealthData health;
        Entity physicalData;
        AnimationPlayer animations;
        public BruteController(MainGame game, GameEntity entity)
            : base(game, entity)
        {
            this.health = entity.GetSharedData(typeof(HealthData)) as HealthData;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;

            PlayAnimation("pig_attack", false);
        }

        public override void PlayHit()
        {
            AnimationClip clip = animations.skinningDataValue.AnimationClips["pig_hit"];
            animations.MixClipOnce(clip);
        }

        public void PlayAnimation(string animationName, bool mix)
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
    }
}
