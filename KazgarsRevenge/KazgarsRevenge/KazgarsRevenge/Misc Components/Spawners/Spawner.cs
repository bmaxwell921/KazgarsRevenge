using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Component used to 
    /// </summary>
    public abstract class Spawner : AIComponent
    {
        // Where to spawn stuff
        protected Vector3 spawnLocation;

        // The type to spawn
        protected EntityType type;

        public Spawner(KazgarsRevengeGame game, GameEntity entity, EntityType type, Vector3 spawnLocation)
            : base(game, entity)
        {
            this.type = type;
            this.spawnLocation = spawnLocation;
        }

        public override void Update(GameTime gameTime)
        {
            if (NeedsSpawn(gameTime))
            {
                Spawn();
            }
        }

        /// <summary>
        /// Returns whether or not this spawner should
        /// spawn something this update cycle
        /// </summary>
        /// <returns></returns>
        public abstract bool NeedsSpawn(GameTime time);

        /// <summary>
        /// Spawns an Entity
        /// </summary>
        public abstract void Spawn();
    }
}
