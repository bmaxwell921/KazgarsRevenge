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
using BEPUphysics.CollisionRuleManagement;


namespace KazgarsRevenge
{
    public class PhysicsComponent : Component
    {
        public Entity collidable;
        public Vector3 Position { get { return collidable.Position; } }
        public PhysicsComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.collidable = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        public override void Start()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Add(collidable);
            collidable.CollisionInformation.Tag = Entity;
        }

        public override void Update(GameTime gametime)
        {
            
        }

        public override void End()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Remove(collidable);
        }

        public static bool PairIsColliding(CollidablePairHandler pair)
        {
            foreach (var contactInformation in pair.Contacts)
            {
                if (contactInformation.Contact.PenetrationDepth >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
