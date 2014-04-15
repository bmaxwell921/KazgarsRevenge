using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Draws 2D components (just player HUD for now)
    /// </summary>
    public class SpriteManager : DrawableGameComponent
    {
        List<Component> components = new List<Component>();
        public SpriteManager(MainGame game)
            : base(game)
        {

            this.UpdateOrder = 3;
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

        public void AddComponent(IDrawableComponent2D toAdd)
        {
            Component cAdd = (Component)toAdd;
            cAdd.Start();
            components.Add(cAdd);
        }

        public void Draw(SpriteBatch s)
        {
            foreach (IDrawableComponent2D d in components)
            {
                d.Draw(s);
            }
        }
    }
}
