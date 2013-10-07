using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class GameEntity
    {
        public string Name { get; protected set; }
        public string Faction { get; protected set; }
        public bool Dead { get; protected set; }
        protected Dictionary<Type, Component> components = new Dictionary<Type, Component>();

        public GameEntity(string name, string faction)
        {
            this.Name = name;
            this.Faction = faction;
            Dead = false;
        }

        public void AddComponent(Type t, Component o)
        {
            Component possObj;
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

        public void Kill()
        {
            foreach (KeyValuePair<Type, Component> pair in components)
            {
                pair.Value.Kill();
            }

            Dead = true;
        }

        public void RemoveComponent(Type t)
        {
            if (components.ContainsKey(t))
            {
                components.Remove(t);
            }
        }

        public Component GetComponent(Type t)
        {
            Component retComponent = null;
            components.TryGetValue(t, out retComponent);
            //will be null if key is not found
            return retComponent;
        }
    }
}
