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
            settings.attackAniName = "onehanded_swing";
            settings.idleAniName = "idle2";
            settings.moveAniName = "run";
            settings.deathAniName = "idle4";
            settings.hitAniName = "idle1";
            settings.walkSpeed = 120;
        }

        protected override void SpawnHitParticles()
        {
            attacks.SpawnHitSparks(physicalData.Position, physicalData.OrientationMatrix.Forward);
        }
        
    }
}
