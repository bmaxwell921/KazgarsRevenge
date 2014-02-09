using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;

namespace KazgarsRevenge
{
    class FallingArrowController : Component
    {
        double lifeCounter = 0;
        double lifeLength = 5000;
        bool dying = false;
        public FallingArrowController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            (entity.GetSharedData(typeof(Entity)) as Entity).CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        public override void Update(GameTime gameTime)
        {
            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                Entity.Kill();
            }
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null && hitEntity.Name == "room")
            {
                (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).Kill();
                lifeCounter = 0;
                lifeLength = 1000;
            }
        }
    }
}
