using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class FlashBomb : BombController
    {
        bool tar;
        public FlashBomb(KazgarsRevengeGame game, GameEntity entity, Vector3 targetPosition, AliveComponent creator, float radius, bool tar)
            : base(game, entity, targetPosition, creator, radius)
        {
            this.tar = tar;
        }

        protected override void CreateExplosion()
        {
            (Game.Services.GetService(typeof(AttackManager)) as AttackManager).CreateFlashExplosion(targetPosition, radius, tar, creator);
        }
    }
}
