using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;

namespace KazgarsRevenge
{
    class FallingArrowController : Component
    {
        double lifeCounter = 0;
        double lifeLength = 2000;
        Entity physicalData;
        public FallingArrowController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        bool sparked = false;
        public override void Update(GameTime gameTime)
        {
            Vector3 pos = physicalData.Position;
            if (!sparked && pos.Y <= 14)
            {
                sparked = true;
                ExpandingCircleBillboard circle = Entity.GetComponent(typeof(ExpandingCircleBillboard)) as ExpandingCircleBillboard;
                (Game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).AddComponent(circle);
                (Entity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).KillComponent();
            }
            lifeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifeCounter >= lifeLength)
            {
                Entity.KillEntity();
            }
        }
    }
}
