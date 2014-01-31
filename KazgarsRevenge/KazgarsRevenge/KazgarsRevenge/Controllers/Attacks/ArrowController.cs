using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    class ArrowController : AttackController
    {

        public ArrowController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator)
            : base(game, entity, damage, factionToHit, creator)
        {
            lifeLength = 3000;
        }

        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "room")
            {
                //makes arrows stick in walls
                (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).Kill();
            }
            base.HandleEntityCollision(hitEntity);
        }
    }
}
