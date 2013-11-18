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
    public class BruteController : AIController
    {
        HealthData health;
        Entity physicalData;
        AnimationPlayer animations;
        LootManager lewts;
        public BruteController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.health = entity.GetSharedData(typeof(HealthData)) as HealthData;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            lewts = game.Services.GetService(typeof(LootManager)) as LootManager;

            PlayAnimation("pig_attack", false);
        }

        List<int> armBoneIndices = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };
        public override void PlayHit()
        {
            animations.MixClipOnce("pig_hit", armBoneIndices);
        }

        public void PlayAnimation(string animationName, bool mix)
        {
            animations.StartClip(animationName);
        }
        public override void Update(GameTime gameTime)
        {
            if (health.Dead)
            {
                entity.Kill();
            }
        }

        public override void End()
        {
            lewts.CreateLootSoul(physicalData.Position, new List<Item>() { lewts.GenerateSword() });
        }
    }
}
