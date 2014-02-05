using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge.Controllers.Attacks
{
    public class OmnishotController : AttackController
    {
        public OmnishotController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator)
            : base(game, entity, damage, factionToHit, creator)
        {
            this.lifeLength = 1000;
        }

        public override void End()
        {
            //Game.Services.GetService(typeof(AttackManager)) as AttackManager;
        }
    }
}
