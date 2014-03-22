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
    public class ReflectController : Component
    {
        FactionType factionToReflect;
        public ReflectController(KazgarsRevengeGame game, GameEntity entity, FactionType factionToReflect)
            : base(game, entity)
        {
            this.factionToReflect = factionToReflect;
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
                }
            }
        }

    }
}
