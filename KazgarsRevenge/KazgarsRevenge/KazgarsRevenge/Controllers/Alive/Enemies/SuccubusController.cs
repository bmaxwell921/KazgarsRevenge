using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class SuccubusController : EnemyController
    {
        public SuccubusController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            settings.aniPrefix = "su_";
            settings.attackAniName = "slash";
            settings.idleAniName = "fly";
            settings.moveAniName = "fly";
            settings.deathAniName = "slash";
            settings.walkSpeed = 120;
            settings.attackLength = 2000;
            settings.attackCreateMillis = 1000;
            settings.usesTwoHander = true;
        }

        protected override void DoDamagedGraphics()
        {
            SpawnHitParticles();
        }
        
    }
}
