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
using KazgarsRevengeCommon;
using Lidgren.Network;
using KazgarsRevengeServer.Managers;
using BEPUphysics;

using KazgarsRevenge;
using BEPUphysics.CollisionRuleManagement;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Server : Microsoft.Xna.Framework.Game
    {
        #region components
        Space physics;
        GeneralComponentManager genComponentManager;
        LevelManager levels;
        PlayerManager players;
        EnemyManager enemies;
        AttackManager attacks;
        ServerMessageManager msgManager;
        #endregion
        //  TODO this guy just runs the game normally

        // current state of the game
        public GameState state;

        public static CollisionGroup GoodProjectileCollisionGroup;
        public static CollisionGroup PlayerCollisionGroup;
        public static CollisionGroup BadProjectileCollisionGroup;
        public static CollisionGroup EnemyCollisionGroup;

        Random rand;

        public Server()
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

            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(PlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(BadProjectileCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
        }

        protected override void Initialize()
        {
            rand = new Random();

            //Entity playerCollidable = new Cylinder(Vector3.Zero, 3, 1, 1);

            //camera = new CameraComponent(this);
            //Components.Add(camera);
            //Services.AddService(typeof(CameraComponent), camera);

            ////adding managers
            //renderManager = new ModelManager(this);
            //Components.Add(renderManager);
            //Services.AddService(typeof(ModelManager), renderManager);

            //genComponentManager = new GeneralComponentManager(this);
            //Components.Add(genComponentManager);
            //Services.AddService(typeof(GeneralComponentManager), genComponentManager);

            //spriteManager = new SpriteManager(this);
            //Components.Add(spriteManager);
            //Services.AddService(typeof(SpriteManager), spriteManager);

            //players = new PlayerManager(this);
            //Components.Add(players);
            //Services.AddService(typeof(PlayerManager), players);

            //enemies = new EnemyManager(this);
            //Components.Add(enemies);
            //Services.AddService(typeof(EnemyManager), enemies);

            //levels = new LevelManager(this);
            //Components.Add(levels);
            //Services.AddService(typeof(LevelManager), levels);

            //attacks = new AttackManager(this);
            //Components.Add(attacks);
            //Services.AddService(typeof(AttackManager), attacks);

            msgManager = new ServerMessageManager(this);
            Components.Add(msgManager);
            Services.AddService(typeof(ServerMessageManager), msgManager);
            base.Initialize();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
