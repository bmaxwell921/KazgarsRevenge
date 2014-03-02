using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public abstract class Component
    {
        protected KazgarsRevengeGame Game;
        public GameEntity Entity { get; private set; }
        public bool Remove { get; protected set; }

        /// <summary>
        /// kills this component by telling the manager to remove it on the next update
        /// </summary>
        public void KillComponent()
        {
            Remove = true;
        }
        public Component(KazgarsRevengeGame game, GameEntity entity)
        {
            this.Game = game;
            this.Entity = entity;
            Remove = false;
        }

        /// <summary>
        /// called when added to a manager's entity list
        /// </summary>
        public virtual void Start()
        {

        }

        public virtual void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// called when this component is removed (on the update after Kill() has been called)
        /// </summary>
        public virtual void End()
        {

        }
    }
}
