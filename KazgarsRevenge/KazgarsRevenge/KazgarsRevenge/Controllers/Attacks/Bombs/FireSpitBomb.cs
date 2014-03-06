using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class FireSpitBomb : BombController
    {

        public FireSpitBomb(KazgarsRevengeGame game, GameEntity entity, Vector3 targetPosition, AliveComponent creator, float radius)
            : base(game, entity, targetPosition, creator, radius)
        {

        }

        protected override void CreateExplosion()
        {
            (Game.Services.GetService(typeof(AttackManager)) as AttackManager).CreateFireSpitExplosion(targetPosition, radius, creator);
        }
    }
}
