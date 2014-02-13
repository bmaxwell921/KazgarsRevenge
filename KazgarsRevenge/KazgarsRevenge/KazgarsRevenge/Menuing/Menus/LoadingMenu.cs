using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// The loading menu...kind breaks the hierarchy a bit but whatever
    /// </summary>
    public class LoadingMenu : SimpleMenu
    {
        // After 1 second passes, add a "."
        private static readonly int ADD_PERIOD_MS = 1000;

        private static readonly string[] PERIOD_STRS = { "", ".", "..", "..." };

        // Current location in the PERIOD_STRS array
        private int curPeriods;

        // How much time until we add another period
        private int timeLeft;

        // The starting title
        private string unModifiedTitle;

        public LoadingMenu(MenuManager mm, string title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds)
            : base(mm, title, drawLocation, background, backgroundBounds)
        {
            curPeriods = 0;
            timeLeft = ADD_PERIOD_MS;
            unModifiedTitle = title;
        }

        /// <summary>
        /// Starts to load the actual level
        /// </summary>
        /// <param name="info"></param>
        public override void Load(object info)
        {
            FloorName level = Extenders.GetFloorName((int)info);
            mm.LoadLevel(level);
        }

        public override object Unload()
        {
            // Do nothing?
            return null;
        }

        public override void Draw(GameTime gameTime)
        {
            timeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeLeft <= 0)
            {
                curPeriods = (++curPeriods) % PERIOD_STRS.Length;
                base.title = this.unModifiedTitle + PERIOD_STRS[curPeriods];
                timeLeft = ADD_PERIOD_MS;
            }
            base.Draw(gameTime);
        }

        #region Do Nothings
        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="e"></param>
        public override void HandleEvent(IEvent e)
        {
            // Do nothing, this should handle no events
        }

        /// <summary>
        /// Always returns false
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool Handles(IEvent e)
        {
            // Doesn't handle any
            return false;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        public override void Register(IEvent e, IAction action)
        {
            // Do nothing
        }
        #endregion
    }
}
