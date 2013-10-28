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
    class ArrowController : Component
    {
        Entity physicalData;
        GameEntity entity;
        int damage;
        public ArrowController(MainGame game, GameEntity arrowEntity, Entity physicalData, int damage)
            : base(game)
        {
            this.entity = arrowEntity;
            this.physicalData = physicalData;
            this.damage = damage;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null && hitEntity.Faction == "bad")
            {
                HealthComponent healthData = hitEntity.GetComponent(typeof(HealthComponent)) as HealthComponent;
                if (healthData != null)
                {
                    healthData.Damage(damage);
                    entity.Kill();
                }
            }
        }
    }
}
