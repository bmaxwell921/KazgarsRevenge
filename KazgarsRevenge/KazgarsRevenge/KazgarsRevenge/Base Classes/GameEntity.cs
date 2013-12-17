using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public enum FactionType
    {
        Players,
        Enemies,
        Neutral,
    }
    public class GameEntity
    {
        public string Name { get; protected set; }
        public FactionType Faction { get; protected set; }
        public bool Dead { get; protected set; }
        private Dictionary<Type, Component> components = new Dictionary<Type, Component>();
        private Dictionary<Type, Object> sharedData = new Dictionary<Type, Object>();

        public GameEntity(string name, FactionType faction)
        {
            this.Name = name;
            this.Faction = faction;
            Dead = false;
        }

        public void AddSharedData(Type t, Object o)
        {
            sharedData.Add(t, o);
        }

        public Object GetSharedData(Type t)
        {
            Object retData = null;
            sharedData.TryGetValue(t, out retData);
            if (retData == null)
            {
                throw new KeyNotFoundException("You must add shared data of type '" + t.ToString() + "' to '" + Name + "'.");
            }
            return retData;
        }

        public void AddComponent(Type t, Component o)
        {
            components.Add(t, o);
        }

        public void Hit()
        {
            Component possAI;
            if (components.TryGetValue(typeof(AIController), out possAI))
            {
                (possAI as AIController).PlayHit();

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

        public HealthData GetHealth()
        {
            Component possHealth = null;
            components.TryGetValue(typeof(HealthHandlerComponent), out possHealth);
            if (possHealth != null)
            {
                return (possHealth as HealthHandlerComponent).GetHealthData();
            }
            else
            {
                return null;
            }
        }
    }
}
