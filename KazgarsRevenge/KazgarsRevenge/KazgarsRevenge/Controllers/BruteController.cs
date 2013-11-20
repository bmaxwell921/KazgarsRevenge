using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionRuleManagement;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public class BruteController : AIController
    {
        HealthData health;
        Entity physicalData;
        AnimationPlayer animations;
        LootManager lewts;

        Entity sensorData;
        public BruteController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.health = entity.GetSharedData(typeof(HealthData)) as HealthData;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            lewts = game.Services.GetService(typeof(LootManager)) as LootManager;

            //sensors are only for contact data; no actual collision
            sensorData = new Box(physicalData.Position, 100, 20, 100);
            sensorData.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
            (Game.Services.GetService(typeof(Space)) as Space).Add(sensorData);


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
            Vector3 pos = physicalData.Position;
            pos.Y = 10;
            lewts.CreateLootSoul(pos, new List<Item>() { lewts.GenerateSword() });
            (Game.Services.GetService(typeof(Space)) as Space).Remove(sensorData);
        }
    }
}
