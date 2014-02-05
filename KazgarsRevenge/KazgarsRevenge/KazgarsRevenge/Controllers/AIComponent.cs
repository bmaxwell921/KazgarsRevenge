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
using SkinnedModelLib;

namespace KazgarsRevenge
{
    abstract public class AIComponent : DrawableComponent2D
    {

        public AIComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        protected Entity physicalData;
        protected float newDir;
        protected float curDir;

        protected Vector3 GetBoneTranslation(int index)
        {
            return (Entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer).GetWorldTransforms()[index].Translation;
        }

        /// <summary>
        /// used for turning model smoothly. slowly adjusts orientation towards 
        /// the newDir and changes velocity based on given speeds
        /// </summary>
        protected void AdjustDir(float runSpeed, float turnSpeed)
        {
            if (curDir != newDir)
            {

                if (Math.Abs(curDir - newDir) <= turnSpeed * 1.3f)
                {
                    curDir = newDir;
                    Vector3 newVel = new Vector3((float)Math.Cos(curDir), 0, (float)Math.Sin(curDir));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(newVel), 0, 0);
                    newVel *= runSpeed;
                    physicalData.LinearVelocity = newVel;
                }
                else
                {
                    float add = turnSpeed;
                    float diff = curDir - newDir;
                    if (diff > 0 && diff < MathHelper.Pi || diff < 0 && -diff > MathHelper.Pi)
                    {
                        add *= -1;
                    }
                    curDir += add;
                    if (curDir > MathHelper.TwoPi)
                    {
                        curDir -= MathHelper.TwoPi;
                    }
                    else if (curDir < 0)
                    {
                        curDir += MathHelper.TwoPi;
                    }
                    Vector3 newVel = new Vector3((float)Math.Cos(curDir), 0, (float)Math.Sin(curDir));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(newVel), 0, 0);
                    newVel *= runSpeed;
                    physicalData.LinearVelocity = newVel;
                }
            }
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
        protected GameEntity QueryNearEntity(string entityName, Vector3 position, float outsideOfRadius, float insideOfRadius)
        {
            Vector3 min = new Vector3(position.X - insideOfRadius, 0, position.Z - insideOfRadius);
            Vector3 max = new Vector3(position.X + insideOfRadius, 20, position.Z + insideOfRadius);
            BoundingBox b = new BoundingBox(min, max);

            var entries=Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(b, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == entityName)
                {
                    if (outsideOfRadius != 0)
                    {
                        BoundingBox entryBox = entry.BoundingBox;
                        Vector3 entrymid = (entryBox.Max + entryBox.Min) / 2;
                        Vector3 selfmid = (b.Max + b.Min) / 2;
                        if (Math.Abs(entrymid.X - selfmid.X) > outsideOfRadius && Math.Abs(entrymid.Z - selfmid.Z) > outsideOfRadius)
                        {
                            return other;
                        }
                    }
                    else
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

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch s)
        {

        }
    }
}
