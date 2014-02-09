using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;
using BEPUphysics;

using KazgarsRevenge;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

namespace KazgarsRevenge
{
    public class KazgarsRevengeGame : Game
    {
        #region components
        protected Space physics;
        public GeneralComponentManager genComponentManager;
        #endregion

        public GameState gameState;

        public CollisionGroup GoodProjectileCollisionGroup;
        public CollisionGroup PlayerCollisionGroup;
        public CollisionGroup BadProjectileCollisionGroup;
        public CollisionGroup EnemyCollisionGroup;
        public CollisionGroup LootCollisionGroup;
        public CollisionGroup SensorLootCollisionGroup;
        public CollisionGroup LevelCollisionGroup;
        public CollisionGroup NetworkedPlayerCollisionGroup;

        public Random rand;

        public KazgarsRevengeGame()
        {
            rand = RandSingleton.Instance;
            InitPhysics();
        }

        private void InitPhysics()
        {
            physics = new Space();

            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < 10 * Environment.ProcessorCount; ++i)
                {
                    //threads! woo!
                    physics.ThreadManager.AddThread();
                }
            }
            physics.ForceUpdater.Gravity = new Vector3(0, -1600, 0);
            physics.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            Services.AddService(typeof(Space), physics);



            //collision groups
            GoodProjectileCollisionGroup = new CollisionGroup();
            PlayerCollisionGroup = new CollisionGroup();
            BadProjectileCollisionGroup = new CollisionGroup();
            EnemyCollisionGroup = new CollisionGroup();
            LootCollisionGroup = new CollisionGroup();
            SensorLootCollisionGroup = new CollisionGroup();
            LevelCollisionGroup = new CollisionGroup();
            NetworkedPlayerCollisionGroup = new CollisionGroup();

            //players
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(PlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);

            //projectiles
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(BadProjectileCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, EnemyCollisionGroup), CollisionRule.NoSolver);

            //networked players
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, LootCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, LevelCollisionGroup), CollisionRule.NoBroadPhase);

            //enemies don't collide with each other
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(EnemyCollisionGroup, EnemyCollisionGroup), CollisionRule.NoBroadPhase);

            //loot dont collide with nuthin
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, EnemyCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, LevelCollisionGroup), CollisionRule.NoBroadPhase);

            //loot sensors don't collide with anything, but generate contacts with loot
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(SensorLootCollisionGroup, LootCollisionGroup), CollisionRule.NoSolver);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(SensorLootCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(SensorLootCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(SensorLootCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(SensorLootCollisionGroup, EnemyCollisionGroup), CollisionRule.NoBroadPhase);
        }

        protected override void Initialize()
        {
            genComponentManager = new GeneralComponentManager(this);
            Components.Add(genComponentManager);
            Services.AddService(typeof(GeneralComponentManager), genComponentManager);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
