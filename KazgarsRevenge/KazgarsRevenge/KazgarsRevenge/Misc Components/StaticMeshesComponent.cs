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
    public class StaticMeshesComponent : Component
    {
        protected List<StaticMesh> allCollidables;
        public StaticMeshesComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
        }

        public override void Start()
        {
            this.allCollidables = Entity.GetSharedData(typeof(List<StaticMesh>)) as List<StaticMesh>;

            Space physics = (Game.Services.GetService(typeof(Space)) as Space);

            if (allCollidables != null)
            {
                foreach (StaticMesh s in allCollidables)
                {
                    s.Tag = Entity;
                    physics.Add(s);
                }
            }
        }

        public override void Update(GameTime gametime)
        {

        }

        public override void End()
        {
            Space physics = (Game.Services.GetService(typeof(Space)) as Space);

            if (allCollidables != null)
            {
                foreach (StaticMesh s in allCollidables)
                {
                    physics.Remove(s);
                }
            }
        }
    }
}
