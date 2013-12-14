using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    /// <summary>
    /// Component used to send velocity requests to the server
    /// </summary>
    public class NetMovementComponent : Component
    {
        // Initially the prevVel should be null so we're forced to update the server
        private Vector3? prevVel;

        private NetworkMessageManager nmm;

        public NetMovementComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            prevVel = null;
            nmm = game.Services.GetService(typeof(NetworkMessageManager)) as NetworkMessageManager;
        }

        /// <summary>
        /// At each update, we check to see if the previous velocity
        /// sent to the server is the different from the velocity
        /// the attached entity currently has. If so, then we let the server
        /// know what the new velocity is
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (base.Game.gameState != GameState.Playing)
            {
                return;
            }
            Vector3 curVel = (entity.GetSharedData(typeof(Entity)) as Entity).LinearVelocity;
            if (prevVel == null || DifferentEnough(curVel, prevVel))
            {
                prevVel = curVel;
                nmm.SendVelocityMessage(curVel);
            }
        }

        private Boolean DifferentEnough(Vector3 v1, Vector3? v2)
        {
            return v1 - v2 != Vector3.Zero;
        }
    }
}
