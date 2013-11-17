using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables;
using BEPUphysics.DataStructures;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public abstract class EntityManager : ModelCreator
    {
        protected ModelManager modelManager;
        protected SpriteManager spriteManager;
        protected GeneralComponentManager genComponentManager;


        public EntityManager(KazgarsRevengeGame game)
            : base(game)
        {
        }

        
        public override void Initialize()
        {
            base.Initialize();
            modelManager = Game.Services.GetService(typeof(ModelManager)) as ModelManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            spriteManager = Game.Services.GetService(typeof(SpriteManager)) as SpriteManager;

        }
    }
}
