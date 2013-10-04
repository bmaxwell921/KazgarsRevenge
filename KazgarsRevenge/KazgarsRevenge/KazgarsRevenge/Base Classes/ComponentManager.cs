using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    abstract class ComponentManager : DrawableGameComponent
    {
        List<Component> components = new List<Component>();
        public ComponentManager(MainGame game)
            : base(game)
        {

        }

        public void AddComponent(Component toAdd)
        {
            toAdd.Start();
            components.Add(toAdd);
        }

        public void RemoveComponent(Component toRemove)
        {
            toRemove.End(); 
            components.Remove(toRemove);
        }
    }
}
