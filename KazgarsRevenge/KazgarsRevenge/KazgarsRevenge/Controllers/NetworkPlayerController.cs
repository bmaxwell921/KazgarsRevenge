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
    public class NetworkPlayerController : AIComponent
    {
        enum NetPlayerState
        {
            Standing,
            Running,
        }

        private NetPlayerState state;

        Vector3 targetPos = Vector3.Zero;
        const float stopRadius = 2;
        AnimationPlayer animations;
        public NetworkPlayerController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.targetPos = physicalData.Position;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
        }

        public void SetPosition(Vector3 pos)
        {
            this.targetPos = pos;
            Vector3 diff = targetPos - physicalData.Position;
            if (Math.Abs(diff.X) > stopRadius && Math.Abs(diff.Z) > stopRadius)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
            }


            //get turning speed (should be faster the greater the difference in direction)

            newDir = GetPhysicsYaw(diff);
            //in case one is around 2pi and the other is around 0 (the difference would be larger than it should)
            float dirdiff = Math.Min(Math.Abs(newDir - curDir), Math.Abs(newDir + MathHelper.TwoPi - curDir));

            //min speed is at 30 degree diff
            turnSpeed = Math.Max(0, dirdiff - .52f);
            float c = turnSpeed / 2.89f;
            turnSpeed *= c;
            turnSpeed += .045f;
        }

        float runSpeed = 120;
        float turnSpeed = .045f;
        public override void Update(GameTime gameTime)
        {

            Vector3 diff = targetPos - physicalData.Position;
            switch (state)
            {
                case NetPlayerState.Running:
                    if (Math.Abs(diff.X) < stopRadius && Math.Abs(diff.Z) < stopRadius)
                    {
                        physicalData.LinearVelocity = Vector3.Zero;
                        state = NetPlayerState.Standing;
                        animations.StartClip("k_fighting_stance");
                    }
                    else
                    {
                        AdjustDir(runSpeed, turnSpeed);
                    }
                    break;
                case NetPlayerState.Standing:
                    if (Math.Abs(diff.X) > stopRadius && Math.Abs(diff.Z) > stopRadius)
                    {
                        curDir = GetGraphicsYaw(diff);
                        physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(curDir, 0, 0);
                        animations.StartClip("k_run");
                        state = NetPlayerState.Running;
                    }
                    break;
            }
        }

        protected override void TakeDamage(int damage, GameEntity from)
        {
            //particles?
        }
    }
}
