using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;

namespace KazgarsRevenge
{
    public class AOEController : Component
    {
        double lifeLength = 5000;
        int damage = 0;
        DeBuff debuff = DeBuff.None;
        Entity physicalData;
        protected AliveComponent creator;
        FactionType factionToHit;
        public AOEController(KazgarsRevengeGame game, GameEntity entity, double tickLength, int damage, DeBuff d, AliveComponent creator, double duration, FactionType factionToHit)
            : base(game, entity)
        {
            this.tickLength = tickLength;
            this.damage = damage;
            this.debuff = d;
            this.creator = creator;
            this.lifeLength = duration;
            this.factionToHit = factionToHit;
        }

        public override void Start()
        {
            this.physicalData = Entity.GetSharedData(typeof(Entity)) as Entity;
        }

        //loop through contacts n' stuff
        double tickCounter = 0;
        double tickLength = 1000;
        public override void Update(GameTime gameTime)
        {
            double millis = gameTime.ElapsedGameTime.TotalMilliseconds;
            lifeLength -= millis;
            if (lifeLength <= 0)
            {
                Entity.KillEntity();
            }

            tickCounter += millis;
            if (tickCounter >= tickLength)
            {
                tickCounter = 0;
                int damageDealt = 0;
                //go through contacts and find what entities are colliding with this one
                foreach (var c in physicalData.CollisionInformation.Pairs)
                {
                    if (PairIsColliding(c))
                    {
                        //found colliding pair; figure out which entity is not us
                        Entity e;
                        if (c.EntityA == physicalData)
                        {
                            e = c.EntityB;
                        }
                        else
                        {
                            e = c.EntityA;
                        }

                        if (e != null)
                        {
                            GameEntity entity = e.CollisionInformation.Tag as GameEntity;
                            if (entity != null && entity.Faction == factionToHit)
                            {
                                //if we can damage it, do so
                                AliveComponent alive = entity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                                if (alive != null)
                                {
                                    damageDealt += alive.Damage(debuff, damage, creator.Entity);
                                }
                            }
                        }

                    }
                }
                if (damageDealt > 0)
                {
                    creator.AddPower(1);
                    creator.HandleDamageDealt(damageDealt);
                }
            }
        }

        protected bool PairIsColliding(CollidablePairHandler pair)
        {
            foreach (var contactInformation in pair.Contacts)
            {
                if (contactInformation.Contact.PenetrationDepth >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
