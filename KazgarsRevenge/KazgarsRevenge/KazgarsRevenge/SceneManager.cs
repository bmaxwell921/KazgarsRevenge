using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class SceneManager : ComponentManager
    {
        private List<GameEntity> entities = new List<GameEntity>();
        public SceneManager(MainGame game)
            : base(game)
        {

        }

        //each method will, in effect, create a new type of entity by adding whatever components you want?
        #region Entity Adding
        public void AddEntity(GameEntity toAdd)
        {
            entities.Add(toAdd);
        }


        #endregion

    }
}
