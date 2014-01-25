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
    public class StaticMeshComponent : Component
    {
        protected StaticMesh collidable;
        public StaticMeshComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.collidable = entity.GetSharedData(typeof(StaticMesh)) as StaticMesh;
        }

        public override void Start()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Add(collidable);
            collidable.Tag = Entity;
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
