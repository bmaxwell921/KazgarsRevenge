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
            : base(game, entity, damage, factionToHit, creator)
        {
            this.lifeLength = 1000;
            this.curDir = GetPhysicsYaw(physicalData.LinearVelocity);
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
                (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).Kill();
            }
            base.HandleEntityCollision(hitEntity);
        }

        public override void Update(GameTime gameTime)
        {
            //incrementally adds damage to the attack as it flies longer, ending up at +1000% damage after 1 seconds
            damage += initialDamage * .1f * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 10;

            base.Update(gameTime);

        }
    }
}
