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
        double lifeLength = 1000;
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
                        (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).KillComponent();
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
            }
        }

        private void EndPull()
        {
            creatorData.IsAffectedByGravity = prevGrav;
            creator.StopPull();
            creatorData.CollisionInformation.CollisionRules.Personal = prevRule;
            creatorData.LinearVelocity = Vector3.Zero;
            Entity.KillEntity();
        }

        bool pulling = false;
        float lastDist = float.MaxValue;
        public override void Update(GameTime gameTime)
        {
            if (pulling)
            {
                Vector3 dir = physicalData.Position - creatorData.Position;
                dir.Y = 0;
                dir.Normalize();
                creatorData.LinearVelocity = dir * 800;


                Vector3 diff = creatorData.Position - physicalData.Position;
                diff.Y = 0;
                float len = diff.Length();
                if (len <= 35 || len > lastDist)
                {
                    EndPull();
                }
                lastDist = len;
            }

            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                if (pulling)
                {
                    EndPull();
                }
                else
                {
                    Entity.KillEntity();
                }
            }
            base.Update(gameTime);
        }

    }
}
