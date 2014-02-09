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
    public class BombController : Component
    {
        Entity physicalData;
        AbilityName bombType;
        Vector3 targetPosition;
        float radius;
        AliveComponent creator;
        public BombController(KazgarsRevengeGame game, GameEntity entity, AbilityName bombType, Vector3 targetPosition, AliveComponent creator, float radius)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.bombType = bombType;
            this.targetPosition = targetPosition;
            this.radius = radius;
            this.creator = creator;
        }

        float pitch = 0;
        float yaw = 0;
        public override void Update(GameTime gameTime)
        {
            pitch += .12f;
            yaw += .01f;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

            if (physicalData.Position.Y <= 0)
            {
                AttackManager attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
                if (bombType == AbilityName.FlashBomb)
                {
                    attacks.CreateFlashExplosion(targetPosition, radius, creator);
                }
                else if (bombType == AbilityName.TarBomb)
                {
                    attacks.CreateTarExplosion(targetPosition, radius, creator);
                }

                Entity.Kill();
            }
        }

    }
}
