using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// A menu that has selectable options
    /// </summary>
    public class SelectionMenu : AMenu
    {
        // All the selections associated with this menu
        private IList<Selection> selections;

        public SelectionMenu(SpriteBatch sb, string title, SpriteFont sf, Vector2 guiScale, Vector2 titleLoc)
            : base(sb, title, sf, guiScale, titleLoc)
        {
            selections = new List<Selection>();
        }

        public void AddSelection(Selection selection)
        {
            this.selections.Add(selection);
        }

        public override void Load()
        {
            // do nothing
        }

        public override void Unload()
        {
            // do nothing
        }

        // Draws the title
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            foreach (Selection sel in selections)
            {
                sel.Draw(gameTime);
            }
        }
    }
}
