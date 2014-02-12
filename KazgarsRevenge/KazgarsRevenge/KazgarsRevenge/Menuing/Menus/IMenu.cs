using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Interface describing behavior for menu screens
    /// </summary>
    public interface IMenu : EventHandler
    {
        /// <summary>
        /// Loads the Menu
        /// </summary>
        /// <param name="info">Any necessary information from the previous screen</param>
        void Load(object info);

        /// <summary>
        /// Unloads the menu
        /// </summary>
        /// <returns>any information needed by the next menu, or null</returns>
        object Unload();

        /// <summary>
        /// Draws the menu
        /// </summary>
        void Draw(GameTime gameTime);
    }
}
