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
using KazgarsRevenge.Libraries;

namespace KazgarsRevenge
{
    public class AttackController : Component
    {
        SoundEffectLibrary sounds;
        Entity physicalData;
        int damage;
        double lifeCounter = 0;
        double lifeLength;
        //either "good" or "bad", for now
        FactionType factionToHit;
        public AttackController(KazgarsRevengeGame game, GameEntity entity, Entity physicalData, int damage, double millisDuration, FactionType factionToHit)
            : base(game, entity)
        {
            this.physicalData = physicalData;
            this.damage = damage;
            this.lifeLength = millisDuration;
            this.factionToHit = factionToHit;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
            sounds = game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;
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
                    if (physicalData.LinearVelocity.Length() > 2)
                    {
                        sounds.playHardSmack();
                    }
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
