using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    /// <summary>
    /// Used for checking if attack controllers should hit them or not
    /// </summary>
    public enum FactionType
    {
        Players,
        Enemies,
        Neutral,
    }

    /// <summary>
    /// Used for loot drops, spawning, and sensors (can find all entities of given type in radius)
    /// </summary>
    public enum EntityType
    {
        None,
        NormalEnemy,
        EliteEnemy,
        Boss,
        Player,
        Misc,
    }

    /// <summary>
    /// An Entity holds some data about what categories it falls into
    /// and holds all of the components that make it up.
    /// 
    /// e.g. a typical Enemy entity is made up of a
    /// PhysicsComponent, AnimatedModelComponent,
    /// EnemyController, DropTable, and BlobShadowDecal.
    /// </summary>
    public class GameEntity
    {
        public Identification id;

        public EntityType Type { get; private set; }
        public string Name { get; private set; }
        public FactionType Faction { get; private set; }
        public void ChangeFaction(FactionType t)
        {
            this.Faction = t;
        }
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
                components[t].KillComponent();
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
