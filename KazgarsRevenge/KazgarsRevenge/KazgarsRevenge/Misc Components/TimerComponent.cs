using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Component to perform actions after a certain amount of time
    /// </summary>
    public abstract class TimerComponent : Component
    {
        // Amount of time that has passed since the action was performed last
        protected float passedTime;

        // time to wait between actions in milliseconds
        private readonly float waitTime;

        /// <summary>
        /// Constructs a new TimerComponent with the given information
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        /// <param name="waitTime">Number of milliseconds to wait between performing actions</param>
        public TimerComponent(KazgarsRevengeGame game, GameEntity entity, float waitTime)
            : base(game, entity)
        {
            this.waitTime = waitTime;
        }

        public override void Start()
        {
            ResetWait();
        }

        /// <summary>
        /// Resets the timer so it will wait the entire waitTime 
        /// to perform its action
        /// </summary>
        public void ResetWait()
        {
            passedTime = 0;
        }

        public override void Update(GameTime gameTime)
        {
            passedTime += gameTime.ElapsedGameTime.Milliseconds;
            if (passedTime > waitTime)
            {
                PerformAction();
                passedTime = 0;
            }
        }

        protected abstract void PerformAction();
    }
}
