using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    abstract class DrawableComponent2D : Component
    {
        public DrawableComponent2D(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            
        }

        abstract public void Draw(SpriteBatch s);
    }
}
