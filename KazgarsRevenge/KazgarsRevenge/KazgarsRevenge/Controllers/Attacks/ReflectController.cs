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
    public class ReflectController : Component
    {
        FactionType factionToReflect;
        double life = 1000;
        AliveComponent creator;
        public ReflectController(KazgarsRevengeGame game, GameEntity entity, FactionType factionToReflect, AliveComponent creator)
            : base(game, entity)
        {
            this.factionToReflect = factionToReflect;
            (this.Entity.GetSharedData(typeof(Entity)) as Entity).CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
            this.creator = creator;
        }

        public override void Update(GameTime gameTime)
        {
            life -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (life <= 0)
            {
                Entity.KillEntity();
            }
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                AttackController possAttack = hitEntity.GetComponent(typeof(AttackController)) as AttackController;
                if (possAttack != null && hitEntity.Faction == factionToReflect)
                {
                    possAttack.Reflect(factionToReflect);
                    creator.AddPower(1);
                }
            }
        }

    }
}
