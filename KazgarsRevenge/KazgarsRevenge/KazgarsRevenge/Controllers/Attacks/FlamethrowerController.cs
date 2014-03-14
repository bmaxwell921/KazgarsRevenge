using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;

namespace KazgarsRevenge
{
    public class FlamethrowerController : AOEController
    {
        public FlamethrowerController(KazgarsRevengeGame game, GameEntity entity, int damage, AliveComponent creator, double duration)
            : base(game, entity, 250, damage, DeBuff.None, creator, duration, FactionType.Players)
        {

        }

        public override void Start()
        {
            this.physicalData = Entity.GetSharedData(typeof(Entity)) as Entity;
            this.dragonData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
            base.Start();
        }

        Entity physicalData;
        Entity dragonData;
        public override void Update(GameTime gameTime)
        {
            if (creator.Dead)
            {
                Entity.KillEntity();
            }
            Matrix3X3 orientation = dragonData.OrientationMatrix;
            physicalData.OrientationMatrix = orientation;

            physicalData.Position = dragonData.Position + orientation.Forward * 200;


            base.Update(gameTime);
        }
    }
}
