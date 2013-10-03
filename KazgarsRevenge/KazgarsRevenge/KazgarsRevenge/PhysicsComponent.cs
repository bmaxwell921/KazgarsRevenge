using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;


namespace KazgarsRevenge
{
    class PhysicsComponent : Component
    {
        protected Entity collidable;
        public PhysicsComponent(MainGame game, Entity collidable)
            : base(game)
        {
            this.collidable = collidable;
        }

        public override void Start()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Add(collidable);
        }

        public override void Update(GameTime gametime)
        {
            
        }

        public override void End()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Remove(collidable);
        }
    }
}
