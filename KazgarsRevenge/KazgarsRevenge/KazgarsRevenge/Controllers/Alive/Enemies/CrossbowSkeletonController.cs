using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class CrossbowSkeletonController : EnemyController
    {
        public CrossbowSkeletonController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            settings.aniPrefix = "s_";
            settings.attackAniName = "shoot";
            settings.attackRange = 300;
            settings.stopChasingRange = 350;
            settings.attackLength = animations.GetAniMillis("s_shoot");
            settings.attackCreateMillis = 600;
        }

        protected override void CreateAttack()
        {
            Vector3 forward = physicalData.OrientationMatrix.Forward;
            attacks.CreateArrow(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Agility), this, false, false, false, false);
        }

        protected override void SpawnHitParticles()
        {

        }
    }
}
