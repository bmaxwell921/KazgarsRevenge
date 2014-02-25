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

        }
    }
}
