using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    abstract class Component
    {
        protected MainGame Game;
        public bool Remove { get; protected set; }
        public void Kill()
        {
            Remove = true;
        }
        public Component(MainGame game)
        {
            this.Game = game;
            Remove = false;
        }

        public virtual void Start()
        {

        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void End()
        {

        }
    }
}
