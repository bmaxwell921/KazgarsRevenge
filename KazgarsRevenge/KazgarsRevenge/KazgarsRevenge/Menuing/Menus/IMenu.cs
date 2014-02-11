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
        /// Loads the menu
        /// </summary>
        void Load();

        /// <summary>
        /// Unloads the menu, saving information as necessary
        /// </summary>
        void Unload();

        /// <summary>
        /// Draws the menu
        /// </summary>
        void Draw(GameTime gameTime);
    }
}
