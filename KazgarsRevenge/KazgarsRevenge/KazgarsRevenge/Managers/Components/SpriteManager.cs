using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class SpriteManager : DrawableGameComponent
    {
        List<DrawableComponent2D> components = new List<DrawableComponent2D>();
        public SpriteManager(MainGame game)
            : base(game)
        {

        }

        public override void Update(GameTime gameTime)
        {
            for (int i = components.Count - 1; i >= 0; --i)
            {
                components[i].Update(gameTime);
                if (components[i].Remove)
                {
                    components[i].End();
                    components.RemoveAt(i);
                }
            }
        }

        public void AddComponent(DrawableComponent2D toAdd)
        {
            toAdd.Start();
            components.Add(toAdd);
        }

        public void Draw(SpriteBatch s)
        {
            foreach (DrawableComponent2D d in components)
            {
                d.Draw(s);
            }
        }
    }
}
