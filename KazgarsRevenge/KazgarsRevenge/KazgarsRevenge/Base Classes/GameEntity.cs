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
    public enum EntityType
    {
        None,
        NormalEnemy,
        EliteEnemy,
        Boss,
        Player,
        Misc,
    }
    public class GameEntity
    {
        public Identification id;

        public EntityType Type { get; private set; }
        public string Name { get; private set; }
        public FactionType Faction { get; private set; }
        public bool Dead { get; private set; }

        private Dictionary<Type, Component> components = new Dictionary<Type, Component>();
        private Dictionary<Type, Object> sharedData = new Dictionary<Type, Object>();

        public GameEntity(string name, FactionType faction, EntityType type)
        {
            this.Name = name;
            this.Faction = faction;
            this.Type = type;
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
            return retData;
        }

        public void AddComponent(Type t, Component o)
        {
            components.Add(t, o);
        }

        /// <summary>
        /// tries to hit this entity for the damage given. returns true if it was an entity with health that could be hit.
        /// </summary>
        public bool Hit(DeBuff neg, int d, GameEntity from)
        {
            Component possAI;
            if (components.TryGetValue(typeof(AliveComponent), out possAI))
            {
                AliveComponent health = components[typeof(AliveComponent)] as AliveComponent;
                (possAI as AliveComponent).Damage(neg, d, from);
                return true;
            }
            return false;
        }

        public void KillEntity()
        {
            foreach (KeyValuePair<Type, Component> pair in components)
            {
                pair.Value.KillComponent();
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
