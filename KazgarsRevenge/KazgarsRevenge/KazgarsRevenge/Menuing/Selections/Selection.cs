using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class to represent a Selectable item on a menu
    /// </summary>
    public abstract class Selection : EventHandler
    {
        // Any event this Selection should handle
        public IEvent e;

        // Any action associated with this Selection
        public IAction action;

        // Where this Selection can be drawn
        public Vector2 startDrawLoc;

        /// <summary>
        /// Constructs a new Selection with the given event and action
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        public Selection(IEvent e, IAction action, Vector2 startDrawLoc)
        {
            this.Register(e, action);
            this.startDrawLoc = startDrawLoc;
        }

        /// <summary>
        /// Interface implementation
        /// </summary>
        /// <param name="e"></param>
        public void HandleEvent(IEvent e)
        {
            if (!Handles(e))
            {
                return;
            }
            action.Perform();
            e.Consume();
        }

        /// <summary>
        /// Interface implementation
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Handles(IEvent e)
        {
            return this.e != null && this.e.Equals(e);
        }

        /// <summary>
        /// Inteface implementation
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        public void Register(IEvent e, IAction action)
        {
            this.e = e;
            this.action = action;
        }

        /// <summary>
        /// Draws the current selection
        /// </summary>
        public abstract void Draw(GameTime gameTime);

        /// <summary>
        /// Method used to inform the selection that the user is highlighting
        /// the selection, be it by hovering with the mouse or otherwise
        /// </summary>
        public abstract void Highlight();

        /// <summary>
        /// Method used to inform the selection that the user is no
        /// longer highlighting the selection
        /// </summary>
        public abstract void DeHighlight();
    }
}
