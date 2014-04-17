using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    abstract public class AIComponent : QueryComponent
    {

        public AIComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        protected Entity physicalData;
        protected float newDir;
        protected float curDir;

        /// <summary>
        /// gets the absolute world translation of the bone index given
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
                //if close enough to the target direction, snap to it
                if (Math.Abs(curDir - newDir) <= turnSpeed * 1.3f)
                {
                    curDir = newDir;
                }
                else//otherwise, add turnSpeed radians toward the target direction
                {
                    float add = turnSpeed;
                    float diff = curDir - newDir;
                    //if diff is between 0 and pi, or between -pi and -2pi, need to subtract radians.
                    //otherwise add radians
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
                }
                Vector3 newVel = new Vector3((float)Math.Sin(curDir + MathHelper.Pi), 0, (float)Math.Cos(curDir + MathHelper.Pi));
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(curDir, 0, 0);
                newVel *= runSpeed;
                physicalData.LinearVelocity = newVel;
            }
        }
        /// <summary>
        /// get the radians representing the angle in the XZ plane for the given direction
        /// </summary>
        protected float GetYaw(Vector3 dir)
        {
            float retYaw = (float)Math.Atan2(dir.X, dir.Z);
            retYaw += MathHelper.Pi;
            return retYaw;
        }

        /// <summary>
        /// 180 degrees off of GetYaw
        /// </summary>
        protected float GetBackwardsYaw(Vector3 dir)
        {
            float retYaw = GetYaw(dir) - MathHelper.Pi;
            while (retYaw < 0)
            {
                retYaw += MathHelper.Pi * 2;
            }
            while (retYaw > MathHelper.Pi * 2)
            {
                retYaw -= MathHelper.Pi * 2;
            }
            return retYaw;
        }
    }
}
