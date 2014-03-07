using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class DebrisController : Component
    {
        Entity physicalData;
        double lifeLength = 1000;
        public DebrisController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            lifeLength += RandSingleton.U_Instance.Next(2500, 5500);
        }

        public override void Start()
        {
            physicalData = Entity.GetSharedData(typeof(Entity)) as Entity;
        }

        public override void Update(GameTime gameTime)
        {
            double millis = gameTime.ElapsedGameTime.TotalMilliseconds;

            lifeLength -= millis;
            if (lifeLength <= 0)
            {
                Entity.KillEntity();
            }

            float velAdd = (float)(-200 * millis / 1000);
            Vector3 newVel = physicalData.LinearVelocity;
            newVel.Y += velAdd;
            physicalData.LinearVelocity = newVel;
        }
    }
}
