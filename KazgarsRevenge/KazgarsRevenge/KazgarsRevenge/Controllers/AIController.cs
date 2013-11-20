using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.ResourceManagement;

namespace KazgarsRevenge
{
    public class AIController : Component
    {
        public AIController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {

        }

        public virtual void PlayHit()
        {

        }

        protected float GetYaw(Vector3 move)
        {
            //orientation
            float yaw = (float)Math.Atan(move.X / move.Z);
            if (move.Z < 0 && move.X >= 0
                || move.Z < 0 && move.X < 0)
            {
                yaw += MathHelper.Pi;
            }
            yaw += MathHelper.Pi;
            return yaw;
        }

        protected GameEntity QueryNearest(string entityName, BoundingSphere s)
        {
            
            var entries=Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(s, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == entityName)
                {
                    return other;
                }
            }
            return null;
        }

        protected GameEntity LookForNearest(string entityName, Entity sensor)
        {
            GameEntity closest = null;
            float nearestLength = float.MaxValue;
            //look for contacts
            foreach (var c in sensor.CollisionInformation.Pairs)
            {
                //found colliding pair
                if (PairIsColliding(c))
                {
                    //getting other entity (don't know if sensor is EntityB or EntityA until we check)
                    Entity e;
                    if (c.EntityA == sensor)
                    {
                        e = c.EntityB;
                    }
                    else
                    {
                        e = c.EntityA;
                    }

                    if (e != null)
                    {
                        GameEntity other = e.CollisionInformation.Tag as GameEntity;
                        if (other != null && other.Name == entityName)
                        {
                            //check if it's nearer than the last result
                            float distance = (e.Position - sensor.Position).Length();
                            if (distance < nearestLength && distance != 0)
                            {
                                //found new nearest entity
                                closest = other;
                                nearestLength = distance;
                            }
                        }
                    }
                }
            }
            return closest;
        }

        protected bool PairIsColliding(CollidablePairHandler pair)
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
