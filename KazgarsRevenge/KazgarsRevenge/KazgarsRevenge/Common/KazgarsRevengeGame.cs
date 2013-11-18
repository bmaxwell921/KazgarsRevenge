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
        protected GeneralComponentManager genComponentManager;
        #endregion

        public GameState gameState;

        public CollisionGroup GoodProjectileCollisionGroup;
        public CollisionGroup PlayerCollisionGroup;
        public CollisionGroup BadProjectileCollisionGroup;
        public CollisionGroup EnemyCollisionGroup;
        public CollisionGroup LootCollisionGroup;

        Random rand;

        public KazgarsRevengeGame()
        {
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
            physics.ForceUpdater.Gravity = new Vector3(0, -80f, 0);
            Services.AddService(typeof(Space), physics);


            //collision groups
            GoodProjectileCollisionGroup = new CollisionGroup();
            PlayerCollisionGroup = new CollisionGroup();
            BadProjectileCollisionGroup = new CollisionGroup();
            EnemyCollisionGroup = new CollisionGroup();
            LootCollisionGroup = new CollisionGroup();

            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(PlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(BadProjectileCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);

            //loot dont collide with nuthin
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(LootCollisionGroup, EnemyCollisionGroup), CollisionRule.NoBroadPhase);
        }

        protected override void Initialize()
        {
            rand = new Random();

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
