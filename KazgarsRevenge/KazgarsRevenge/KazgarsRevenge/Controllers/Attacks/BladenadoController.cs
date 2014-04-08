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

namespace KazgarsRevenge
{
    public class SwordnadoController : AOEController
    {
        public SwordnadoController(KazgarsRevengeGame game, GameEntity entity, int damage, AliveComponent creator, double duration, FactionType factionToHit)
            : base(game, entity, 250, damage, DeBuff.None, creator, duration, factionToHit)
        {
            followData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
        }

        Entity followData;
        public override void Update(GameTime gameTime)
        {
            if (creator.Dead)
            {
                Entity.KillEntity();
            }

            if (followData != null)
            {
                physicalData.Position = followData.Position;
            }

            base.Update(gameTime);
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                AttackController possAttack = hitEntity.GetComponent(typeof(AttackController)) as AttackController;
                if (possAttack != null && hitEntity.Faction == factionToHit)
                {
                    possAttack.Reflect(factionToHit);
                }
            }
        }
    }
}
