using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class FlashBomb : BombController
    {

        public FlashBomb(KazgarsRevengeGame game, GameEntity entity, Vector3 targetPosition, AliveComponent creator, float radius)
            : base(game, entity, targetPosition, creator, radius)
        {

        }

        protected override void CreateExplosion()
        {
            (Game.Services.GetService(typeof(AttackManager)) as AttackManager).CreateFlashExplosion(targetPosition, radius, creator);
        }
    }
}
