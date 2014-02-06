using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Spawner that spawns stuff when other stuff gets within a certain proximity
    /// </summary>
    public class EnemyProximitySpawner : Spawner
    {
        // Distance used to start spawning
        private float proximity;

        // Delay between spawns
        private float delay;

        // When we last spawned something
        private float passedTime;

        /// <summary>
        /// Creates a new spawner that spawns based on proximity to players
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="spawnLocation"></param>
        /// <param name="proximity">If the distance from the spawnLocation to the closest enemy is less than proximity, this spawns enemies</param>
        /// <param name="delay">The delay between each spawn</param>
        public EnemyProximitySpawner(KazgarsRevengeGame game, GameEntity entity, EntityType spawnType, Vector3 spawnLocation, float proximity, float delay)
            : base(game, entity, spawnType, spawnLocation)
        {
            this.proximity = proximity;
            this.delay = delay;
            passedTime = delay;
        }

        public override void Update(GameTime gameTime)
        {
            // Even if we aren't spawning still update this so it spawns immediately
            passedTime += gameTime.ElapsedGameTime.Milliseconds;
            base.Update(gameTime);
        }

        /// <summary>
        /// Returns true if a player is within the proximity
        /// </summary>
        /// <returns></returns>
        public override bool NeedsSpawn()
        {
            // check if anything is close by
            return QueryNearEntityFaction(FactionType.Players, spawnLocation, 0, proximity, true) != null;          
        }

        /// <summary>
        /// Spawns an enemy based on delay time
        /// </summary>
        public override void Spawn()
        {
            if (passedTime >= delay)
            {
                // TODO actual level
                ((EnemyManager)Game.Services.GetService(typeof(EnemyManager))).CreateBrute(IdentificationFactory.getId(type, Identification.NO_CLIENT), spawnLocation, 1);
                passedTime = 0;
            }
        }
    }
}
