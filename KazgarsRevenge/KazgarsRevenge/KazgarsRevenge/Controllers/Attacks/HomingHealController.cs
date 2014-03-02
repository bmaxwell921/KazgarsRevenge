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
            physicalData.IsAffectedByGravity = false;
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                AliveComponent a = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (a != null && a == target)
                {
                    a.LifeSteal(heal);
                    Entity.KillEntity();
                }
            }
        }

        double turnCounter = 0;
        double turnInterval = 50;
        public override void Update(GameTime gameTime)
        {
            if (target == null || target.Dead)
            {
                Entity.KillEntity();
            }
            else
            {
                turnCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (turnCounter >= turnInterval)
                {
                    Vector3 move = (target.Entity.GetSharedData(typeof(Entity)) as Entity).Position - physicalData.Position;
                    move.Y = 0;
                    if (move != Vector3.Zero)
                    {
                        move.Normalize();
                    }

                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(move), 0, 0);
                    physicalData.LinearVelocity = move * 250.0f;

                    turnCounter = 0;
                }
            }
            
            base.Update(gameTime);
        }


    }
}
