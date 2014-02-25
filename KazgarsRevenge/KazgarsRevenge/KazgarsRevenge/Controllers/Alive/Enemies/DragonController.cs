using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class DragonController : EnemyController
    {
        public DragonController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity, 10)
        {
            settings.stopChasingRange = 2000;
        }

        private void AIDragonPhase1(double millis)
        {
            //sit still and rotate towards player

            //every 4 seconds, launch ice or fire attack (alternating)

            //if pillars are destroyed, go to phase 2
        }

        private void AIDragonPhase2(double millis)
        {
            //run towards player

            //if in range, bite player (alternating heads)

            //every 6 seconds, launch fire or ice ground attack (alternating)

            //if 2 minutes pass, go to phase 3
        }

        private void AIDragonPhase3(double millis)
        {
            //breathe fire continuously at player
        }
    }
}
