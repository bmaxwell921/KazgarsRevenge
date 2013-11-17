using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class AttackController : Component
    {
        Entity physicalData;
        int damage;
        double lifeCounter = 0;
        double lifeLength;
        //either "good" or "bad", for now
        string factionToHit;
        public AttackController(KazgarsRevengeGame game, GameEntity entity, Entity physicalData, int damage, double millisDuration, string factionToHit)
            : base(game, entity)
        {
            this.physicalData = physicalData;
            this.damage = damage;
            this.lifeLength = millisDuration;
            this.factionToHit = factionToHit;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;

        }

        public override void Update(GameTime gameTime)
        {
            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                entity.Kill();
            }
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                if (hitEntity.Name == "room")
                {
                    //makes arrows stick in walls
                    (entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).Kill();
                }
                if (hitEntity.Faction == factionToHit)
                {
                    HealthHandlerComponent healthData = hitEntity.GetComponent(typeof(HealthHandlerComponent)) as HealthHandlerComponent;
                    if (healthData != null)
                    {
                        healthData.Damage(damage);
                    }
                    entity.Kill();
                }
            }
        }
    }
}
