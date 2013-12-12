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
            Vector3 lmove = new Vector3();
            lmove.X = move.X;
            lmove.Y = move.Y;
            lmove.Z = move.Z;
            if (lmove == Vector3.Zero)
            {
                lmove.Z = .00000001f;
            }
            else
            {

                lmove.Normalize();
            }
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

        protected BoundingBox GetSensor(Vector3 position, float radius)
        {
            Vector3 min = new Vector3(position.X - radius, 0, position.Z - radius);
            Vector3 max = new Vector3(position.X + radius, 20, position.Z + radius);
            BoundingBox b = new BoundingBox(min, max);
            return b;
        }

        protected GameEntity QueryNearest(string entityName, BoundingBox s)
        {
            var entries=Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(s, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == entityName)
                {
                    BoundingBox entryBox = entry.BoundingBox;
                    Vector3 entrymid = (entryBox.Max + entryBox.Min) / 2;
                    Vector3 selfmid = (s.Max + s.Min) / 2;
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
