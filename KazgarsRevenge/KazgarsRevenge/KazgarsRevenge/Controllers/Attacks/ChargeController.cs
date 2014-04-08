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
    public class ChargeController : Component
    {
        double lifeLen;
        Entity physicalData;
        FactionType factionToHit;
        AliveComponent creator;
        Entity followData;
        public ChargeController(KazgarsRevengeGame game, GameEntity entity, double duration, FactionType factionToHit, AliveComponent creator)
            :base (game, entity)
        {
            this.lifeLen = duration;
            this.factionToHit = factionToHit;
            this.creator = creator;
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;

            followData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
        }

        public override void Update(GameTime gameTime)
        {
            physicalData.Position = followData.Position;

            lifeLen -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeLen <= 0)
            {
                Entity.KillEntity();
            }
            base.Update(gameTime);
        }


        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null && hitEntity.Faction == factionToHit)
            {
                AliveComponent alive = hitEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (alive != null)
                {
                    alive.Damage(DeBuff.Charge, 0, creator.Entity, AttackType.None);
                    alive.KnockBack(physicalData.Position, 900);
                    (Game.Services.GetService(typeof(CameraComponent)) as CameraComponent).ShakeCamera(4);
                }
            }
        }
    }
}
