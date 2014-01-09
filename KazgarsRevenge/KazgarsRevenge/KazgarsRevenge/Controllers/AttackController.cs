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
        //the entity that created this attack
        GameEntity creator;
        SoundEffectLibrary sounds;
        Entity physicalData;
        int damage;
        double lifeCounter = 0;
        double lifeLength;
        //either "good" or "bad", for now
        FactionType factionToHit;
        public AttackController(KazgarsRevengeGame game, GameEntity entity, Entity physicalData, int damage, double millisDuration, FactionType factionToHit, GameEntity creator)
            : base(game, entity)
        {
            this.physicalData = physicalData;
            this.damage = damage;
            this.lifeLength = millisDuration;
            this.factionToHit = factionToHit;
            this.creator = creator;
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

        int damageDealt = 0;
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
                        damageDealt += healthData.Damage(damage);
                    }
                    entity.Kill();
                }
            }
        }

        public override void End()
        {
            PlayerController possPlayer = creator.GetComponent(typeof(PlayerController)) as PlayerController;
            if (possPlayer != null)
            {
                possPlayer.HandleDamageDealt(damageDealt);
            }
            base.End();
        }
    }
}
