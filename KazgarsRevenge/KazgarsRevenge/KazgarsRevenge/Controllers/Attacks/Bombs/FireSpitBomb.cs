using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class FireSpitBomb : BombController
    {

        public FireSpitBomb(KazgarsRevengeGame game, GameEntity entity, Vector3 targetPosition, AliveComponent creator)
            : base(game, entity, targetPosition, creator, 100)
        {
            yawIncrement = 0;
            pitchIncrement = 0;
        }

        protected override void CreateExplosion()
        {
            (Game.Services.GetService(typeof(AttackManager)) as AttackManager).CreateFireSpitExplosion(targetPosition, radius, creator);
        }
    }
}
