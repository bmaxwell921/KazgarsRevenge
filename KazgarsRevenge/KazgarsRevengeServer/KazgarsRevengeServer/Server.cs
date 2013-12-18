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

namespace KazgarsRevengeServer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Server : KazgarsRevengeGame
    {
        GraphicsDeviceManager graphics;
        SNetworkingMessageManager nmm;
        
        protected SLevelManager levels;
        protected SEnemyManager enemies;
        protected SAttackManager attacks;
        protected SPlayerManager players;

        protected MessageQueue msgQ;

        // Milliseconds between each time step
        private readonly int TIME_STEP = 100;
        private int timeToUpdate;
     
        public Server() : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            timeToUpdate = TIME_STEP;
        }

        protected override void Initialize()
        {
            base.Initialize();
            nmm = new SNetworkingMessageManager(this);
            Components.Add(nmm);
            Services.AddService(typeof(SNetworkingMessageManager), nmm);

            levels = new SLevelManager(this);
            Components.Add(levels);
            Services.AddService(typeof(SLevelManager), levels);

            enemies = new SEnemyManager(this);
            Components.Add(enemies);
            Services.AddService(typeof(SEnemyManager), enemies);

            attacks = new SAttackManager(this);
            Components.Add(attacks);
            Services.AddService(typeof(SAttackManager), attacks);

            players = new SPlayerManager(this);
            Components.Add(players);
            Services.AddService(typeof(SPlayerManager), players);

            msgQ = new MessageQueue();
            Services.AddService(typeof(MessageQueue), msgQ);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            /*
             * For in game updates
             */ 
            if (gameState == GameState.Playing)
            {
                physics.Update();
                // Only update the game every once in a while to save bandwidth
                if (timeToUpdate <= 0)
                {
                    timeToUpdate = TIME_STEP;
                    // Base update should just have 
                    base.Update(gameTime);
                    nmm.SendGameSnapshot();
                    return;
                }
                timeToUpdate -= gameTime.ElapsedGameTime.Milliseconds;

                // Always receive messages
                nmm.Update(gameTime);
                return;
            }
            base.Update(gameTime);
        }
    }
}
