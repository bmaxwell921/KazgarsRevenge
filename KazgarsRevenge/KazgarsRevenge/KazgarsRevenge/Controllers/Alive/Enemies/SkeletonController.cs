using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class SkeletonController : EnemyController
    {
        AnimatedModelComponent model;
        public SkeletonController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            settings.aniPrefix = "s_";
            settings.attackAniName = "magic";
            settings.attackRange = 300;
            settings.noticePlayerRange = 400;
            settings.stopChasingRange = 350;
        }

        public override void Start()
        {
            this.model = Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent;
            base.Start();
        }

        protected override void CreateAttack()
        {
            attacks.CreateFrostbolt(physicalData.Position + physicalData.OrientationMatrix.Forward * 16 + physicalData.OrientationMatrix.Right * 8, physicalData.OrientationMatrix.Forward, 1, this as AliveComponent);

            model.RemoveEmitter("frostchargeleft");
            model.RemoveEmitter("frostchargeright");
        }


        protected override void DuringAttack(int i)
        {
            if (i == 0)
            {
                model.AddEmitter(typeof(FrostChargeSystem), "frostchargeleft", 50, 0, Vector3.Zero, "s_hand_L");
                model.AddEmitterSizeIncrementExponential("frostchargeleft", 15, 2);

                model.AddEmitter(typeof(FrostChargeSystem), "frostchargeright", 50, 0, Vector3.Zero, "s_hand_R");
                model.AddEmitterSizeIncrementExponential("frostchargeright", 15, 2);
            }
        }

        protected override void SpawnHitParticles()
        {

        }

        protected override void KillAlive()
        {
            model.RemoveEmitter("frostchargeleft");
            model.RemoveEmitter("frostchargeright");
            base.KillAlive();
        }
    }
}
