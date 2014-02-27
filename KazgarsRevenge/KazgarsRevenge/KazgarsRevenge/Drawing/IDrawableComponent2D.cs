using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public interface IDrawableComponent2D
    {
        void Draw(SpriteBatch s);
    }
}
