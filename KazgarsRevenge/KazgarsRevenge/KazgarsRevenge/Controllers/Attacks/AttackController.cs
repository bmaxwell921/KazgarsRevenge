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
    public class AttackController : AIComponent
    {
        AttackManager attacks;
        //the entity that created this attack
        protected AliveComponent creator;
        protected float damage;
        //either "good" or "bad", for now
        protected FactionType factionToHit;
        protected DeBuff debuff = DeBuff.None;
        public AttackController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator)
            : base(game, entity)
        {
            this.damage = damage;
            this.factionToHit = factionToHit;
            this.creator = creator;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        bool lifesteal = false;
        float amountStolen = 0;
        public void ReturnLife(float percentReturned)
        {
            lifesteal = true;
            amountStolen = percentReturned;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
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

            if (hitData.Count > 0)
            {
                CheckHitEntities();
            }
        }

        /// <summary>
        /// called the first update after a collision is detected.
        /// default behavior is to hit
        /// </summary>
        protected virtual void CheckHitEntities()
        {
            DamageTarget(hitData[0]);
            hitData.Clear();
            Entity.Kill();
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
                int d = t.Damage(debuff, (int)damage, creator.Entity);
                damageDealt += d;
                if (lifesteal)
                {
                    attacks.CreateHomingHeal(physicalData.Position, creator, (int)Math.Ceiling(d * .1f));
                }
            }
        }

        public override void End()
        {
            creator.HandleDamageDealt(damageDealt);
            base.End();
        }
    }
}
