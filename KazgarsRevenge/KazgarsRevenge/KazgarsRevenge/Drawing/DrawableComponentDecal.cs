using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class DrawableComponentDecal: Component
    {
        public DrawableComponentDecal(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {

        }

        public virtual void Draw(Matrix view, Matrix projection) { }
    }
}
