using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    abstract class Component
    {
        protected MainGame Game;
        protected GameEntity entity;
        public bool Remove { get; protected set; }
        public void Kill()
        {
            Remove = true;
        }
        public Component(MainGame game, GameEntity entity)
        {
            this.Game = game;
            this.entity = entity;
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
