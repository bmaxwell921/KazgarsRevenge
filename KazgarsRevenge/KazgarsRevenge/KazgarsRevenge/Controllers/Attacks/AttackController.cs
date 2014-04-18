﻿using System;
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

namespace KazgarsRevenge
{
    /// <summary>
    /// the base controller for all attacks. handles the Bepu Entity.
    /// </summary>
    public class AttackController : AIComponent
    {
        //the entity that created this attack
        protected AliveComponent creator;
        protected float damage;
        //the types of entities to hit
        protected FactionType factionToHit;
        protected DeBuff debuff = DeBuff.None;
        protected AttackType type;
        public AttackController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator, AttackType type)
            : base(game, entity)
        {
            this.damage = damage;
            this.factionToHit = factionToHit;
            this.creator = creator;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.type = type;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        public void Reflect(FactionType toHit)
        {
            //fchange faction and flip direction
            this.factionToHit = toHit;
            Entity.ChangeFaction(toHit);
            physicalData.LinearVelocity = new Vector3(-physicalData.LinearVelocity.X, 0, -physicalData.LinearVelocity.Z);
        }

        bool lifesteal = false;
        float amountStolen = 0;
        public void ReturnLife(float percentReturned)
        {
            lifesteal = true;
            amountStolen = percentReturned;
        }

        double lifeCounter = 0;
        protected double lifeLength = 500;
        public override void Update(GameTime gameTime)
        {
            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                Entity.KillEntity();
            }

            CheckHitEntities();
            
        }

        public void HitMultipleTargets()
        {
            hitMultiple = true;
        }

        public void AddDebuff(DeBuff d)
        {
            this.debuff = d;
        }

        protected bool hitMultiple = false;
        protected bool dieAfterContact = true;
        /// <summary>
        /// called the first update after a collision is detected.
        /// default behavior is to hit the first enemy contacted only
        /// </summary>
        protected void CheckHitEntities()
        {
            if (hitData.Count > 0)
            {
                if (hitMultiple)
                {
                    foreach (AliveComponent a in hitData)
                    {
                        DamageTarget(a);
                        creator.AddPower(1);
                    }
                }
                else
                {
                    DamageTarget(hitData[0]);
                    creator.AddPower(1);
                }

                hitData.Clear();
                if (dieAfterContact)
                {
                    Entity.KillEntity();
                }
            }
        }

        int damageDealt = 0;
        /// <summary>
        /// Called when a collision is first detected
        /// </summary>
        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                HandleEntityCollision(hitEntity);
            }
        }

        protected List<AliveComponent> hitData = new List<AliveComponent>();
        protected virtual void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Faction == factionToHit)
            {
                AliveComponent healthData = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (healthData != null)
                {
                    lock (hitData)
                    {
                        hitData.Add(healthData);
                    }
                }
            }
        }

        protected void DamageTarget(AliveComponent t)
        {
            if (t != null)
            {
                int toDeal = GetDamage();
                int d = t.Damage(debuff, toDeal, creator.Entity, type, true);
                damageDealt += d;
                if (lifesteal)
                {
                    creator.LifeSteal((int)Math.Ceiling(d * amountStolen));
                }
            }
        }

        /// <summary>
        /// helper to get the damage. can be overridden to account for a critical strike chance
        /// </summary>
        protected virtual int GetDamage()
        {
            return (int)damage;
        }

        //tell the creator of this entity how much damage it dealt
        public override void End()
        {
            creator.HandleDamageDealt(damageDealt);
            base.End();
        }
    }
}
