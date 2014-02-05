using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    
    public class ArrowController : AttackController
    {
        float speed = 0;
        public ArrowController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator, float speed)
            : base(game, entity, damage, factionToHit, creator)
        {
            this.speed = speed;
            this.lifeLength = 3000;
            this.curDir = GetPhysicsYaw(physicalData.LinearVelocity);
        }
        bool penetrating = false;
        bool homing = false;
        bool leeching = false;
        bool bleeding = false;


        public void Penetrate()
        {
            penetrating = true;
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
        }

        public void Leeching()
        {
            leeching = true;
        }

        public void Bleed()
        {
            bleeding = true;
            debuff = DeBuff.Bleeding;
        }

        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "room")
            {
                //makes arrows stick in walls
                (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).Kill();
            }
            base.HandleEntityCollision(hitEntity);
        }

        protected override void CheckHitEntities()
        {
            if (penetrating)
            {
                foreach (AliveComponent a in hitData)
                {
                    DamageTarget(a);
                }
                hitData.Clear();
            }
            else
            {
                base.CheckHitEntities();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (homing)
            {
                if(target == null || target.Dead)
                {
                    target = null;
                    GameEntity possEnt = QueryNearEntityFaction(factionToHit, physicalData.Position, 10, 200, true);
                    if (possEnt != null)
                    {
                        target = possEnt.GetComponent(typeof(AliveComponent)) as AliveComponent;
                    }
                }
                if (target != null)
                {
                    Vector3 move = (target.Entity.GetSharedData(typeof(Entity)) as Entity).Position - physicalData.Position;
                    move.Y = 0;
                    newDir = GetPhysicsYaw(move);
                    AdjustDir(speed, .3f);
                }
            }


            base.Update(gameTime);

        }
    }
}
