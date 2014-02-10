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
    public class GrapplingHookController : AIComponent
    {
        AliveComponent creator;
        Entity creatorData;
        double lifeCounter = 0;
        double lifeLength = 5000;
        public GrapplingHookController(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator)
            : base(game, entity)
        {
            this.creator = creator;
            this.creatorData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
            physicalData.IsAffectedByGravity = false;
            physicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
        }

        BEPUphysics.CollisionRuleManagement.CollisionRule prevRule;
        bool prevGrav = false;
        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                if (!pulling)
                {
                    if (hitEntity.Name == "room")
                    {
                        creator.Pull();
                        physicalData.LinearVelocity = Vector3.Zero;
                        pulling = true;
                        prevGrav = creatorData.IsAffectedByGravity;
                        creatorData.IsAffectedByGravity = false;
                        prevRule = creatorData.CollisionInformation.CollisionRules.Personal;
                        creatorData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
                        Vector3 dir = physicalData.Position - creatorData.Position;
                        dir.Y = 0;
                        dir.Normalize();
                        creatorData.LinearVelocity = dir * 800;
                        lifeLength = 8000;
                    }
                }
                else
                {
                    if (hitEntity == creator.Entity)
                    {
                        EndPull();
                    }
                }
            }
        }

        private void EndPull()
        {
            creatorData.IsAffectedByGravity = prevGrav;
            creator.StopPull();
            creatorData.CollisionInformation.CollisionRules.Personal = prevRule;
            creatorData.LinearVelocity = Vector3.Zero;
            Entity.Kill();
        }

        bool pulling = false;
        public override void Update(GameTime gameTime)
        {
            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                if (pulling)
                {
                    EndPull();
                }
                else
                {
                    Entity.Kill();
                }
            }
            base.Update(gameTime);
        }

    }
}
