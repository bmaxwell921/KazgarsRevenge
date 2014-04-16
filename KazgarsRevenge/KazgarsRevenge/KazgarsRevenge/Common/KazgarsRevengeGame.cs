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

        public CollisionGroup PlayerCollisionGroup;
        public CollisionGroup EnemyCollisionGroup;
        public CollisionGroup LevelCollisionGroup;
        public CollisionGroup NetworkedPlayerCollisionGroup;
        public CollisionGroup UntouchableCollisionGroup;

        public Random rand;

        public KazgarsRevengeGame()
        {
            rand = RandSingleton.S_Instance;
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
            // Comment this to remove gravity
            physics.ForceUpdater.Gravity = new Vector3(0, 0, 0);
            physics.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            Services.AddService(typeof(Space), physics);

            //collision groups
            PlayerCollisionGroup = new CollisionGroup();
            EnemyCollisionGroup = new CollisionGroup();
            LevelCollisionGroup = new CollisionGroup();
            NetworkedPlayerCollisionGroup = new CollisionGroup();
            UntouchableCollisionGroup = new CollisionGroup();

            //players don't collide with each other
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(PlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);

            //networked players dont collide with local players or the level
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(NetworkedPlayerCollisionGroup, LevelCollisionGroup), CollisionRule.NoBroadPhase);

            //enemies don't collide with each other
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(EnemyCollisionGroup, EnemyCollisionGroup), CollisionRule.NoBroadPhase);

            //untouchables only collide with level (e.g. if kazgar is tumbling)
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(UntouchableCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(UntouchableCollisionGroup, EnemyCollisionGroup), CollisionRule.NoBroadPhase);
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

        /// <summary>
        /// Method used to turn on and off the updating of Managers who
        /// should only update when we're in the game playing state
        /// </summary>
        /// <param name="enabled"></param>
        public virtual void SetInGameManagersEnabled(bool enabled)
        {
            genComponentManager.Enabled = enabled;
        }
    }
}
