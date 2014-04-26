using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{

    public class SnipeController : AttackController
    {
        int initialDamage;
        public SnipeController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator, bool magnetic)
            : base(game, entity, damage, factionToHit, creator, AttackType.Ranged)
        {
            this.lifeLength = 1000;
            this.curDir = GetBackwardsYaw(physicalData.LinearVelocity);
            this.initialDamage = damage;
            if (magnetic)
            {
                debuff = DeBuff.MagneticImplant;
            }
        }


        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "room")
            {
                //makes arrows stick in walls
                (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).KillComponent();
            }
            base.HandleEntityCollision(hitEntity);
        }

        public override void Update(GameTime gameTime)
        {
            //incrementally adds damage to the attack as it flies longer, ending up at +500% damage after .5 second
            damage += initialDamage * .05f * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 5;

            base.Update(gameTime);

        }
    }
}
