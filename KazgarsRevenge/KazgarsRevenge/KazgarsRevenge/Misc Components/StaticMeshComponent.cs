using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionRuleManagement;


namespace KazgarsRevenge
{
    class StaticMeshComponent : Component
    {
        protected StaticMesh collidable;
        public StaticMeshComponent(MainGame game, GameEntity entity, StaticMesh collidable)
            : base(game, entity)
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
