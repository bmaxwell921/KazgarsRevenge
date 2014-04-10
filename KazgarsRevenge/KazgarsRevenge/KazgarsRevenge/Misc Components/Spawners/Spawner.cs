using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Component used to Spawn entities. 
    /// </summary>
    public abstract class Spawner : Component
    {
        // Where to spawn stuff
        protected List<Vector3> spawnLocations;

        // The type to spawn
        protected EntityType type;

        public Spawner(KazgarsRevengeGame game, GameEntity entity, EntityType type, List<Vector3> spawnLocations)
            : base(game, entity)
        {
            this.type = type;
            this.spawnLocations = spawnLocations;
        }

        public override void Update(GameTime gameTime)
        {
            if (NeedsSpawn())
            {
                Spawn();
            }
        }

        /// <summary>
        /// Returns whether or not this spawner should
        /// spawn something this update cycle
        /// </summary>
        /// <returns></returns>
        public abstract bool NeedsSpawn();

        /// <summary>
        /// Spawns an Entity
        /// </summary>
        public abstract void Spawn();
    }
}
