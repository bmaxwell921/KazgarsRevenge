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

        bool prevGrav = false;
        AliveComponent target;
        Entity targetData;
        //if this controller collides with the same entity, tell it so and start pulling it towards the creator
        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            lock (creatorData)
            {
                if (!pulling)
                {
                    GameEntity hitEntity = other.Tag as GameEntity;
                    if (hitEntity != null)
                    {
                        if (hitEntity.Name == "room")
                        {
                            Entity.KillEntity();
                        }
                        if (hitEntity.Faction == factionToSpear)
                        {
                            target = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                            if (target != null)
                            {
                                targetData = hitEntity.GetSharedData(typeof(Entity)) as Entity;
                                target.Pull();
                                physicalData.LinearVelocity = Vector3.Zero;
                                physicalData.Position = targetData.Position;
                                pulling = true;
                                prevGrav = creatorData.IsAffectedByGravity;
                                lifeLength = 8000;
                                if (target != null)
                                {
                                    target.Damage(stun ? DeBuff.ForcefulThrow : DeBuff.None, 0, creator.Entity, AttackType.None, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EndPull()
        {
            if (targetData != null)
            {
                targetData.LinearVelocity = Vector3.Zero;
                target.StopPull();
            }
            Entity.KillEntity();
        }

        bool pulling = false;
        float lastDist = float.MaxValue;
        public override void Update(GameTime gameTime)
        {
            if (pulling)
            {//force entity being pulled to slide towards creator
                Vector3 dir = creatorData.Position - physicalData.Position;
                dir.Y = 0;
                if (dir != Vector3.Zero)
                {
                    dir.Normalize();
                }
                targetData.LinearVelocity = dir * 800;
                physicalData.LinearVelocity = dir * 800;

                Vector3 diff = creatorData.Position - physicalData.Position;
                diff.Y = 0;
                float len = diff.Length();
                if (len <= 35 || len > lastDist || target.Dead)
                {
                    EndPull();
                }
                lastDist = len;
            }
            else
            {
                lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (lifeCounter >= lifeLength)
                {
                    Entity.KillEntity();
                }
            }
            base.Update(gameTime);
        }
    }
}
