using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class LooseCannonController : AttackController
    {
        float chargedPercent = 0;
        public LooseCannonController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator, float chargedPercent)
            : base(game, entity, damage, factionToHit, creator)
        {
            lifeLength = 4000;
            dieAfterContact = true;
            this.chargedPercent = chargedPercent;
        }

        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "room")
            {
                Entity.KillEntity();
            }
            base.HandleEntityCollision(hitEntity);
        }

        public override void End()
        {
            (Game.Services.GetService(typeof(AttackManager)) as AttackManager).CreateExplosion(physicalData.Position, (int)damage, creator, chargedPercent * 2);
        }
    }
}
