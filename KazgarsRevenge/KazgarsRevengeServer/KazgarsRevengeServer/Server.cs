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
        protected SMessageSender sms;
        protected LoggerManager lm;

        // Milliseconds between each time step
        private readonly int TIME_STEP = 100;
        private int timeToUpdate;

     
        public Server() : base()
        {
            gameState = GameState.ServerStart;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            timeToUpdate = TIME_STEP;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            // LoggerManager created first since it doesn't rely on anything and everyone will want to use it
            SetUpLoggers();

            ServerConfig sc = ServerConfigReader.ReadConfig((LoggerManager)Services.GetService(typeof(LoggerManager)));
            Services.AddService(typeof(ServerConfig), sc);

            SGameStateMonitor gsm = new SGameStateMonitor(this);
            Components.Add(gsm);
            Services.AddService(typeof(SGameStateMonitor), gsm);

            nmm = new SNetworkingMessageManager(this);
            Components.Add(nmm);
            Services.AddService(typeof(SNetworkingMessageManager), nmm);

            sms = new SMessageSender(nmm.server, (LoggerManager)Services.GetService(typeof(LoggerManager)));
            Services.AddService(typeof(SMessageSender), sms);

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

        protected override void LoadContent()
        {
            base.LoadContent();
        }


        protected void SetUpLoggers()
        {
            lm = new LoggerManager();
            // Log to both the console and a file
            lm.AddLogger(new FileWriteLogger(FileWriteLogger.SERVER_SUB_DIR));
            lm.AddLogger(new ConsoleLogger());
            Services.AddService(typeof(LoggerManager), lm);
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
                    sms.SendGameSnapshot(players);
                    return;
                }
                timeToUpdate -= gameTime.ElapsedGameTime.Milliseconds;

                // Always receive messages
                nmm.Update(gameTime);
                return;
            }
            base.Update(gameTime);
        }

        public void TransitionBackToServerStart()
        {
            // TODO other unloading things?
            gameState = GameState.ServerStart;

            // Reset the id counts
            IdentificationFactory.Reset();

            // Reset the hostId to the default
            nmm.hostId = new Identification(0);

            // Reset all the managers....
            players.Reset();

            levels.Reset();
            enemies.Reset();
            attacks.Reset();
            msgQ.Reset(); ;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
