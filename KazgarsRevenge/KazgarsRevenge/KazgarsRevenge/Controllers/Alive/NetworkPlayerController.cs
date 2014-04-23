using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public class NetworkPlayerController : PlayerController
    {
        enum NetPlayerState
        {
            Standing,
            Running,
        }

        private NetPlayerState state;

        Vector3 targetPos = Vector3.Zero;
        public NetworkPlayerController(KazgarsRevengeGame game, GameEntity entity, Account account)
            : base(game, entity, account)
        {
            this.targetPos = physicalData.Position;
            this.attached = Entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;
            LootManager gearGenerator = Game.Services.GetService(typeof(LootManager)) as LootManager;
            gear[GearSlot.Righthand] = null;
            gear[GearSlot.Lefthand] = null;
            //EquipGear(gearGenerator.GenerateSword(), GearSlot.Righthand);
            //EquipGear(gearGenerator.GenerateBow(), GearSlot.Lefthand);

            stopRadius = 2;
        }

        public void SetPosition(Vector3 pos)
        {
            this.targetPos = pos;
            Vector3 diff = targetPos - physicalData.Position;
            if (Math.Abs(diff.X) > stopRadius && Math.Abs(diff.Z) > stopRadius)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetYaw(diff), 0, 0);
            }


            //get turning speed (should be faster the greater the difference in direction)

            newDir = GetBackwardsYaw(diff);
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
                        animations.StartClip("k_fighting_stance", MixType.None);
                    }
                    else
                    {
                        AdjustDir(runSpeed, turnSpeed);
                    }
                    break;
                case NetPlayerState.Standing:
                    if (Math.Abs(diff.X) > stopRadius && Math.Abs(diff.Z) > stopRadius)
                    {
                        curDir = GetYaw(diff);
                        physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(curDir, 0, 0);
                        animations.StartClip("k_run", MixType.None);
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
