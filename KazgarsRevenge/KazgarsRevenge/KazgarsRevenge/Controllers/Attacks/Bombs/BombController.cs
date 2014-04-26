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
    /// <summary>
    /// a controller that rotates and explodes when it hits the ground
    /// </summary>
    abstract public class BombController : Component
    {
        
        protected Entity physicalData;
        protected Vector3 targetPosition;
        protected float radius;
        protected AliveComponent creator;
        public BombController(KazgarsRevengeGame game, GameEntity entity, Vector3 targetPosition, AliveComponent creator, float radius)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.targetPosition = targetPosition;
            this.radius = radius;
            this.creator = creator;
        }

        float pitch = 0;
        float yaw = 0;

        protected float pitchIncrement = .02f;
        protected float yawIncrement = .12f;
        public override void Update(GameTime gameTime)
        {
            pitch += pitchIncrement;
            yaw += yawIncrement;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

            if (physicalData.Position.Y <= 0)
            {
                AttackManager attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
                CreateExplosion();

                Entity.KillEntity();
            }
        }

        protected virtual void CreateExplosion()
        {

        }

    }
}
