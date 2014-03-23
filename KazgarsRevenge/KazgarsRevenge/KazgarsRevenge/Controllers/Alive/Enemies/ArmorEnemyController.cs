using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class ArmorEnemyController : EnemyController
    {
        public ArmorEnemyController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            settings.aniPrefix = "k_";
            settings.attackAniName = "swing_r";
            settings.idleAniName = "fighting_stance";
            settings.moveAniName = "run";
            settings.deathAniName = "death";
            settings.hitAniName = "idle1";
            settings.walkSpeed = 120;
            settings.attackLength = 1100;
            settings.attackCreateMillis = 350;
        }

        protected override void SpawnHitParticles()
        {
            attacks.SpawnHitSparks(physicalData.Position, physicalData.OrientationMatrix.Forward);
        }
        
    }
}
