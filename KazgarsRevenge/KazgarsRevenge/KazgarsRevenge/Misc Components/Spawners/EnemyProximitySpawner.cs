using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{

    /// <summary>
    /// Spawner that spawns stuff when other stuff gets within a certain proximity.
    /// When spawn is called, enemies will be spawned at every location
    /// </summary>
    public class EnemyProximitySpawner : Spawner
    {
        public const int NO_LIMIT = -1;
        // Distance used to start spawning
        private float proximity;

        // Delay between spawns
        private float delay;

        // When we last spawned something
        private float passedTime;

        // The max number of enemies to spawn
        private int limit;

        // Number of enemies actually spawned
        private int numSpawned;

        PlayerManager players;
        LevelManager levels;
        EnemyManager enemies;

        /// <summary>
        /// Creates a new spawner that spawns based on proximity to players
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="spawnLocation"></param>
        /// <param name="proximity">If the distance from the spawnLocation to the closest enemy is less than proximity, this spawns enemies</param>
        /// <param name="delay">The delay between each spawn</param>
        /// <param name="limit">Set this parameter if you wish to limit the number of enemies a spawner can spawn</param>
        public EnemyProximitySpawner(KazgarsRevengeGame game, GameEntity entity, EntityType type, List<Vector3> spawnLocations, float proximity, float delay, int limit = NO_LIMIT)
            : base(game, entity, type, spawnLocations)
        {
            this.proximity = proximity;
            this.delay = delay;
            passedTime = delay;
            this.limit = limit;

            this.players = game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            this.levels = game.Services.GetService(typeof(LevelManager)) as LevelManager;
            enemies = game.Services.GetService(typeof(EnemyManager)) as EnemyManager;
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
            if (limit != NO_LIMIT && numSpawned >= limit)
            {
                return false;
            }

            if (passedTime >= delay)
            {
                // check if anything is close by
                if (spawnLocations.Count() != 0)
                {
                    // TODO Just check one of them
                    //return QueryNearEntityFaction(FactionType.Players, spawnLocations.GetEnumerator().Current, 0, proximity, false) != null;


                    //more efficient version
                    Vector3 checkLoc = spawnLocations[0];//spawnLocations.GetEnumerator().Current;
                    return players.IsPlayerNear(checkLoc, proximity);
                }
                passedTime = 0;
            }
            return false;
        }

        /// <summary>
        /// Spawns an enemy based on delay time
        /// </summary>
        public override void Spawn()
        {
            foreach (Vector3 loc in spawnLocations)
            {
                ((EnemyManager)Game.Services.GetService(typeof(EnemyManager))).CreateEnemy(type, loc);
                // Make sure to limit as necessary
                ++numSpawned;
            }
        }
    }
}
