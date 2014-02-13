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
    public class ChainSpearController : AIComponent
    {
        AliveComponent creator;
        Entity creatorData;
        double lifeCounter = 0;
        double lifeLength = 1000;
        FactionType factionToSpear = FactionType.Enemies;
        bool stun = false;
        public ChainSpearController(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator, bool stun)
            : base(game, entity)
        {
            this.creator = creator;
            this.stun = stun;
            this.factionToSpear = creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players;
            this.creatorData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
            physicalData.IsAffectedByGravity = false;
            physicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
        }

        BEPUphysics.CollisionRuleManagement.CollisionRule prevRule;
        bool prevGrav = false;
        AliveComponent target;
        Entity targetData;
        bool gotOneAlready = false;
        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if (!pulling)
            {
                GameEntity hitEntity = other.Tag as GameEntity;
                if (hitEntity != null)
                {
                    if (hitEntity.Name == "room")
                    {
                        Entity.Kill();
                    }
                    if (hitEntity.Faction == factionToSpear)
                    {
                        target = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                        if (target != null)
                        {
                            if (!gotOneAlready)
                            {
                                gotOneAlready = true;
                                targetData = hitEntity.GetSharedData(typeof(Entity)) as Entity;
                                target.Pull();
                                physicalData.LinearVelocity = Vector3.Zero;
                                pulling = true;
                                prevGrav = creatorData.IsAffectedByGravity;
                                targetData.IsAffectedByGravity = false;
                                prevRule = creatorData.CollisionInformation.CollisionRules.Personal;
                                targetData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
                                lifeLength = 8000;
                                if (target != null)
                                {
                                    target.Damage(DeBuff.ForcefulThrow, 0, creator.Entity);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EndPull()
        {
            targetData.IsAffectedByGravity = prevGrav;
            target.StopPull();
            targetData.CollisionInformation.CollisionRules.Personal = prevRule;
            targetData.LinearVelocity = Vector3.Zero;
            Entity.Kill();
        }

        bool pulling = false;
        float lastDist = float.MaxValue;
        public override void Update(GameTime gameTime)
        {
            if (pulling)
            {
                Vector3 dir = creatorData.Position - physicalData.Position;
                dir.Y = 0;
                dir.Normalize();
                targetData.LinearVelocity = dir * 800;
                physicalData.LinearVelocity = dir * 800;

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
                    Entity.Kill();
                }
            }
            base.Update(gameTime);
        }
    }
}
