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
                model.AddEmitter(typeof(FrostCharge1System), "frostchargeleft", 20, 0, Vector3.Zero, "s_hand_L");
                model.AddEmitter(typeof(FrostCharge1System), "frostchargeright", 20, 0, Vector3.Zero, "s_hand_R");
            }
            if (i == 1)
            {
                model.AddEmitter(typeof(FrostCharge2System), "frostchargeleft", 20, 0, Vector3.Zero, "s_hand_L");
                model.AddEmitter(typeof(FrostCharge2System), "frostchargeright", 20, 0, Vector3.Zero, "s_hand_R");
            }
            else if (i == 2)
            {
                model.AddEmitter(typeof(FrostCharge3System), "frostchargeleft", 20, 0, Vector3.Zero, "s_hand_L");
                model.AddEmitter(typeof(FrostCharge3System), "frostchargeright", 20, 0, Vector3.Zero, "s_hand_R");
            }
            else if (i == 3)
            {
                model.AddEmitter(typeof(FrostCharge4System), "frostchargeleft", 20, 0, Vector3.Zero, "s_hand_L");
                model.AddEmitter(typeof(FrostCharge4System), "frostchargeright", 20, 0, Vector3.Zero, "s_hand_R");
            }
        }

        protected override void SpawnHitParticles()
        {

        }

        protected override void AIDeath()
        {

            model.RemoveEmitter("frostchargeleft");
            model.RemoveEmitter("frostchargeright");
            base.AIDeath();
        }
    }
}
