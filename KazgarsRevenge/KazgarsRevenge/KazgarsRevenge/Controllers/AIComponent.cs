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

                if (Math.Abs(curDir - newDir) <= turnSpeed * 1.3f)
                {
                    curDir = newDir;
                    Vector3 newVel = new Vector3((float)Math.Cos(curDir), 0, (float)Math.Sin(curDir));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetBackwardsYaw(newVel), 0, 0);
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
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetBackwardsYaw(newVel), 0, 0);
                    newVel *= runSpeed;
                    physicalData.LinearVelocity = newVel;
                }
            }
        }

        /// <summary>
        /// 180 degrees off of GetYaw
        /// </summary>
        protected float GetBackwardsYaw(Vector3 move)
        {
            float yaw = (float)Math.Atan2(move.X, move.Z);
            yaw += MathHelper.Pi;
            return yaw;
        }

        //get the radians representing the angle in the XZ plane for the given direction
        protected float GetYaw(Vector3 move)
        {
            return -GetBackwardsYaw(move) - MathHelper.PiOver2;
        }
    }
}
