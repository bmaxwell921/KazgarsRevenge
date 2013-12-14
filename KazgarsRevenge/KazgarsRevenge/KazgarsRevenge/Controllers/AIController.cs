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

        protected float GetGraphicsYaw(Vector3 move)
        {
            Vector3 lmove = new Vector3();
            lmove.X = move.X;
            lmove.Y = move.Y;
            lmove.Z = move.Z;
            if (lmove.Z == 0)
            {
                lmove.Z = .00000001f;
            }
            else
            {
                lmove.Normalize();
            }
            float yaw = (float)Math.Atan(lmove.X / lmove.Z);
            if (lmove.Z < 0 && lmove.X >= 0
                || lmove.Z < 0 && lmove.X < 0)
            {
                yaw += MathHelper.Pi;
            }
            yaw += MathHelper.Pi;
            return yaw;
        }

        protected float GetPhysicsYaw(Vector3 move)
        {
            float retYaw = -GetGraphicsYaw(move) - MathHelper.PiOver2;
            while(retYaw > MathHelper.TwoPi)
            {
                retYaw -= MathHelper.TwoPi;
            }
            while(retYaw < 0)
            {
                retYaw += MathHelper.TwoPi;
            }

            return retYaw;
        }

        /// <summary>
        /// grabs the entity of an entityName within radius of position (not necessarily the nearest, just the first one it sees)
        /// </summary>
        protected GameEntity QueryNearest(string entityName, Vector3 position, float radius)
        {
            Vector3 min = new Vector3(position.X - radius, 0, position.Z - radius);
            Vector3 max = new Vector3(position.X + radius, 20, position.Z + radius);
            BoundingBox b = new BoundingBox(min, max);

            var entries=Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(b, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == entityName)
                {
                    BoundingBox entryBox = entry.BoundingBox;
                    Vector3 entrymid = (entryBox.Max + entryBox.Min) / 2;
                    Vector3 selfmid = (b.Max + b.Min) / 2;
                    if (Math.Abs(entrymid.X - selfmid.X) > .1f && Math.Abs(entrymid.Z - selfmid.Z) > .1f)
                    {
                        return other;
                    }
                }
            }
            return null;
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
