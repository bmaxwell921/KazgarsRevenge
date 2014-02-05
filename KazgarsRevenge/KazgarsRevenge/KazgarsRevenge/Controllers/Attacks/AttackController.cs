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
        AliveComponent creator;
        SoundEffectLibrary sounds;
        Entity physicalData;
        int damage;
        //either "good" or "bad", for now
        FactionType factionToHit;
        public AttackController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator)
            : base(game, entity)
        {
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.damage = damage;
            this.factionToHit = factionToHit;
            this.creator = creator;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
            sounds = game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;
        }

        double lifeCounter = 0;
        protected double lifeLength = 500;
        public override void Update(GameTime gameTime)
        {
            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                Entity.Kill();
            }
        }

        int damageDealt = 0;
        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                HandleEntityCollision(hitEntity);
            }
        }

        protected virtual void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Faction == factionToHit)
            {
                AliveComponent healthData = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (healthData != null)
                {
                    damageDealt += healthData.Damage(DeBuff.None, damage, creator.Entity);
                }
                Entity.Kill();
            }
        }

        public override void End()
        {
            creator.HandleDamageDealt(damageDealt);
            base.End();
        }
    }
}
