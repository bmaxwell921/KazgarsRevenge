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
    class PhysicsComponent : Component
    {
        public Entity collidable;
        public Vector3 Position { get { return collidable.Position; } }
        public PhysicsComponent(MainGame game, GameEntity entity)
            : base(game, entity)
        {
            this.collidable = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        public override void Start()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Add(collidable);
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

        /// <summary>
        /// checks if the entity is supported by a physics object
        /// (used for checking if the player can jump or not)
        /// (this is really just a simplified version of the method found on the BepuPhysics website)
        /// </summary>
        /// <returns></returns>
        protected bool IsSupported()
        {
            foreach (var pair in collidable.CollisionInformation.Pairs)
            {
                if (pair.CollisionRule == CollisionRule.Normal)
                {
                    foreach (var c in pair.Contacts)
                    {
                        //It's possible that a subpair has a non-normal collision rule, even if the parent pair is normal.
                        if (c.Pair.CollisionRule != CollisionRule.Normal)
                            continue;
                        //Compute the offset from the position of the character's body to the contact.
                        Vector3 contactOffset;
                        Vector3 pos = collidable.Position;
                        Vector3.Subtract(ref c.Contact.Position, ref pos, out contactOffset);


                        //Calibrate the normal of the contact away from the center of the object.
                        float dot;
                        Vector3 normal;
                        Vector3.Dot(ref contactOffset, ref c.Contact.Normal, out dot);
                        normal = c.Contact.Normal;
                        if (dot < 0)
                        {
                            Vector3.Negate(ref normal, out normal);
                            dot = -dot;
                        }

                        //Support contacts are all contacts on the feet of the character- a set that include contacts that support traction and those which do not.
                        Vector3 down = Vector3.Down;
                        Vector3.Dot(ref normal, ref down, out dot);
                        if (dot > .01f)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
