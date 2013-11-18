using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class LootSoulController : Component
    {
        Entity physicalData;
        AnimationPlayer animations;
        public Dictionary<int, Item> Loot;
        public LootSoulController(KazgarsRevengeGame game, GameEntity entity, int wanderSeed, List<Item> loot)
            : base(game, entity)
        {
            this.Loot = new Dictionary<int, Item>();
            for(int i=0; i<loot.Count; ++i)
            {
                this.Loot.Add(i, loot[i]);
            }

            rand = new Random(wanderSeed);
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            animations.StartClip("pig_attack");
        }

        Random rand;
        double wanderCounter = 0;
        double wanderLength = 0;
        float groundSpeed = 5.0f;
        public override void Update(GameTime gameTime)
        {
            wanderCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (wanderCounter >= wanderLength)
            {
                wanderCounter = 0;
                wanderLength = rand.Next(4000, 10000);
                float newDir = rand.Next(1, 627) / 10.0f;
                Vector3 newVel = new Vector3((float)Math.Cos(newDir) * groundSpeed, 0, (float)Math.Sin(newDir) * groundSpeed);
                physicalData.LinearVelocity = newVel;
            }

            if (Loot.Count == 0)
            {
                this.Kill();
            }
        }

    }
}
