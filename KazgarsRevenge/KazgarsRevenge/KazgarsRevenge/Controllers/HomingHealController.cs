using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;

namespace KazgarsRevenge
{
    public class HomingHealController: AIComponent
    {
        AliveComponent target;
        int heal;
        public HomingHealController(KazgarsRevengeGame game, GameEntity entity, AliveComponent target, int heal)
            : base(game, entity)
        {
            this.target = target;
            this.heal = heal;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                AliveComponent a = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (a != null)
                {
                    a.Heal(heal);
                    Entity.Kill();
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (target == null || target.Dead)
            {
                Entity.Kill();
            }
            else
            {
                Vector3 move = (target.Entity.GetSharedData(typeof(Entity)) as Entity).Position - physicalData.Position;
                move.Y = 0;
                newDir = GetPhysicsYaw(move);
                AdjustDir(450.0f, .6f);
            }
            
            base.Update(gameTime);
        }


    }
}
