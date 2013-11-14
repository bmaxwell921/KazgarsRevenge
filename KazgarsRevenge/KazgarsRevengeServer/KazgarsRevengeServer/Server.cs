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
        SNetworkingMessageManager msgManager;
     
        public Server() : base()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            msgManager = new SNetworkingMessageManager(this);
            Components.Add(msgManager);
            Services.AddService(typeof(SNetworkingMessageManager), msgManager);
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
