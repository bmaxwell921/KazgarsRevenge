using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    /// <summary>
    /// The base controller for projectiles
    /// </summary>
    public class ProjectileController : AttackController
    {
        public ProjectileController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator)
            : base(game, entity, damage, factionToHit, creator, AttackType.Ranged)
        {
            this.lifeLength = 2000;

            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
        }
        bool homing = false;

        public void Penetrate()
        {
            hitMultiple = true;
            dieAfterContact = false;
        }


        AliveComponent target;
        public void Home(AliveComponent target)
        {
            homing = true;
            this.target = target;
        }
        public void Home()
        {
            homing = true;
            this.target = null;
            this.curDir = GetYaw(physicalData.LinearVelocity);
        }

        public void Bleed()
        {
            debuff = DeBuff.SerratedBleeding;
        }

        public void KillOnFirstContact()
        {
            stickInWalls = false;
        }

        private bool stickInWalls = true;
        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "room")
            {
                if (stickInWalls)
                {
                    (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).KillComponent();
                    ArrowVBillboard possV = Entity.GetComponent(typeof(ArrowVBillboard)) as ArrowVBillboard;
                    if (possV != null)
                    {
                        possV.KillComponent();
                    }
                }
                else
                {
                    Entity.KillEntity();
                }
            }
            base.HandleEntityCollision(hitEntity);
        }

        public override void Update(GameTime gameTime)
        {
            if (homing)
            {//if homing, look for closest target and rotate towards it
                if(target == null || target.Dead)
                {
                    target = null;
                    GameEntity possEnt = QueryNearEntityFaction(factionToHit, physicalData.Position, 10, 200, true);
                    if (possEnt != null)
                    {
                        target = possEnt.GetComponent(typeof(AliveComponent)) as AliveComponent;
                    }
                }
                else
                {
                    Vector3 move = (target.Entity.GetSharedData(typeof(Entity)) as Entity).Position - physicalData.Position;
                    move.Y = 0;
                    newDir = GetYaw(move);
                    if (newDir < 0)
                    {
                        newDir += MathHelper.Pi * 2;
                    }
                    else if (newDir > MathHelper.Pi * 2)
                    {
                        newDir -= MathHelper.Pi * 2;
                    }
                    AdjustDir(450.0f, .25f);
                }
            }


            base.Update(gameTime);

        }
    }
}
