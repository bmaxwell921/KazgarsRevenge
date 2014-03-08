using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class DragonFrostbolt : ProjectileController
    {
        public DragonFrostbolt(KazgarsRevengeGame game, GameEntity entity, int damage, AliveComponent creator)
            : base(game, entity, damage, FactionType.Players, creator)
        {

        }

        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "firepillar")
            {
                PillarController pillarAI = hitEntity.GetComponent(typeof(PillarController)) as PillarController;
                if (pillarAI != null)
                {
                    pillarAI.TakeHit();
                }
                Entity.KillEntity();
            }
            if (hitEntity.Name == "frostpillar")
            {
                PillarController pillarAI = hitEntity.GetComponent(typeof(PillarController)) as PillarController;
                if (pillarAI != null)
                {
                    pillarAI.Heal();
                }
                Entity.KillEntity();
            }
            base.HandleEntityCollision(hitEntity);
        }
    }
}
