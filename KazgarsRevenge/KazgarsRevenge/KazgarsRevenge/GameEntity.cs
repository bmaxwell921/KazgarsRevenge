using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class GameEntity
    {
        protected Dictionary<Type, GameComponent> components;

        public void AddComponent(Type t, GameComponent o)
        {
            GameComponent possObj;
            if (!components.TryGetValue(t, out possObj))
            {
                //components doesn't have this kind of component yet, so add it
                components.Add(t, o);
            }
            else
            {
                //error? components already contain a component of this type
            }
        }

        public void RemoveComponent(Type t)
        {
            if (components.ContainsKey(t))
            {
                components.Remove(t);
            }
        }

        public GameComponent GetComponent(Type t)
        {
            GameComponent retComponent = null;
            if (components.TryGetValue(t, out retComponent))
            {
                return components[t];
            }
            else
            {
                return null;
            }
        }
    }
}
