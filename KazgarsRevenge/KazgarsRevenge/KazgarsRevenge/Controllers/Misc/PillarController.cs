using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class PillarController : Component
    {
        public PillarController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            health = 3;
        }

        int health = 3;
        public void TakeHit()
        {
            --health;
            if (health <= 0)
            {
                Entity.KillEntity();
            }
        }

        public void Heal()
        {
            ++health;
            if (health > 3)
            {
                health = 3;
            }
        }
    }
}
