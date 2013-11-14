using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public abstract class DrawableComponent3D : Component
    {
        public DrawableComponent3D(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {

        }

        abstract public void Draw(GameTime gameTime, Matrix view, Matrix projection, bool edgeDetection);
    }
}
