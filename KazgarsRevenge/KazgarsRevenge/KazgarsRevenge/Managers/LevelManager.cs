using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;

namespace KazgarsRevenge
{
    class LevelManager : DrawableGameComponent
    {
        EntityManager entityManager;

        List<GameEntity> levels = new List<GameEntity>();
        public LevelManager(Game game)
            : base(game)
        {
            
        }

        public override void Initialize()
        {
            entityManager = Game.Services.GetService(typeof(EntityManager)) as EntityManager;
        }

        public void DemoLevel()
        {
            levels.Add(entityManager.CreateLevel("Models\\Levels\\4x4Final", new Vector3(200, -20, -200), 0));
        }


    }
}
