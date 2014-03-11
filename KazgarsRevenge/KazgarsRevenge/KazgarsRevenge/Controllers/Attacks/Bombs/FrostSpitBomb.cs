﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class FrostSpitBomb : BombController
    {
        int damage;
        public FrostSpitBomb(KazgarsRevengeGame game, GameEntity entity, Vector3 targetPosition, int damage, AliveComponent creator)
            : base(game, entity, targetPosition, creator, 100)
        {
            yawIncrement = 0;
            pitchIncrement = 0;
            this.damage = damage;
        }

        protected override void CreateExplosion()
        {
            (Game.Services.GetService(typeof(AttackManager)) as AttackManager).CreateFrostSpitExplosion(targetPosition, radius, damage, creator);
        }
    }
}