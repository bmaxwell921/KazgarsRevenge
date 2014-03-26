using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class MagicSkeletonController : EnemyController
    {
        public MagicSkeletonController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            settings.aniPrefix = "s_";
            settings.attackAniName = "magic";
            settings.attackRange = 300;
            settings.stopChasingRange = 350;
            settings.attackLength = 1667;
            settings.attackCreateMillis = settings.attackLength / 2;

            chargingBoneNames.Add("s_hand_R");
            chargingBoneNames.Add("s_hand_L");
        }

        protected override void CreateAttack()
        {
            attacks.CreateFrostbolt(physicalData.Position + physicalData.OrientationMatrix.Forward * 16 + physicalData.OrientationMatrix.Right * 8, physicalData.OrientationMatrix.Forward, 1, this as AliveComponent);
        }


        protected override void StartAttack()
        {
            base.StartAttack();
            AddChargeParticles(typeof(FrostChargeSystem));
        }

        protected override void SpawnHitParticles()
        {

        }
    }
}
