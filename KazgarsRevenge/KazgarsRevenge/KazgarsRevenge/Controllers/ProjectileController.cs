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
    class ProjectileController : Component
    {
        Entity physicalData;
        GameEntity entity;
        int damage;
        double lifeCounter = 0;
        double lifeLength;
        //either "good" or "bad", for now
        string factionToHit;
        public ProjectileController(MainGame game, GameEntity arrowEntity, Entity physicalData, int damage, double millisDuration, string factionToHit)
            : base(game)
        {
            this.entity = arrowEntity;
            this.physicalData = physicalData;
            this.damage = damage;
            this.lifeLength = millisDuration;
            this.factionToHit = factionToHit;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
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
            if (hitEntity != null && hitEntity.Faction == factionToHit)
            {
                HealthComponent healthData = hitEntity.GetComponent(typeof(HealthComponent)) as HealthComponent;
                if (healthData != null)
                {
                    healthData.Damage(damage);
                }
                entity.Kill();
            }
        }
    }
}
