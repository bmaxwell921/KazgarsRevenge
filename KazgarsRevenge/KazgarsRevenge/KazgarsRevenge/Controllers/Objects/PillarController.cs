using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class PillarController : Component
    {
        Entity physicalData;
        public PillarController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            health = 3;
        }

        public override void Start()
        {
            this.physicalData = Entity.GetSharedData(typeof(Entity)) as Entity;
            base.Start();
        }

        public override void Update(GameTime gameTime)
        {
            physicalData.LinearVelocity = Vector3.Zero;
            base.Update(gameTime);
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
