using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge.Misc_Components;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge.Managers
{
    class SoundManager : GameComponent
    {
        List<SoundComponent> components = new List<SoundComponent>();
        public SoundManager(MainGame game) : base(game)
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

        public void AddComponent(SoundComponent toAdd)
        {
            toAdd.Start();
            components.Add(toAdd);
        }
    }
}
