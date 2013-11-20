using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
    }
}
