using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KazgarsRevenge
{
    /// <summary>
    /// A menu that has selectable options
    /// </summary>
    public class SelectionMenu : AMenu
    {
        // All the selections associated with this menu
        private IList<Selection> selections;

        private int currentSelection;

        public SelectionMenu(SpriteBatch sb, string title, SpriteFont sf, Vector2 guiScale, Vector2 titleLoc, Rectangle screenSize = new Rectangle(), Texture2D background = null)
            : base(sb, title, sf, guiScale, titleLoc, screenSize, background)
        {
            selections = new List<Selection>();
            currentSelection = -1;
            SetUpMovementEvents();
        }

        private void SetUpMovementEvents()
        {
            // Up and Down arrow keys
            this.Register(new KeyEvent(Keys.Up), new UpAction(this));
            this.Register(new KeyEvent(Keys.Down), new DownAction(this));
        }

        public void SelectFirst()
        {
            currentSelection = 0;
            if (selections.Count > 0)
            {
                selections[currentSelection].Highlight();
            }
        }

        /// <summary>
        /// Moves the selection to the next one (ie down)
        /// </summary>
        private void MoveToNextSel()
        {
            if (selections.Count > 0)
            {
                // Wrapping
                int nextSel = (currentSelection + 1) % selections.Count;
                MoveSelTo(currentSelection, nextSel);
            }
        }

        private void MoveToPrevSel()
        {
            if (selections.Count > 0)
            {
                // Handle wrapping
                int nextSel = (currentSelection - 1 < 0) ? selections.Count - 1 : currentSelection - 1;
                MoveSelTo(currentSelection, nextSel);
            }
        }

        
        private void MoveSelTo(int oldSel, int nextSel)
        {
            selections[oldSel].DeHighlight();
            selections[nextSel].Highlight();
            currentSelection = nextSel;
        }

        public void AddSelection(Selection selection)
        {
            this.selections.Add(selection);
        }

        public override void Load()
        {
            // do nothing, for now
        }

        public override void Unload()
        {
            // do nothing, for now
        }

        public override void HandleEvent(IEvent e)
        {
            // If we don't handle the event, let the selection try
            base.HandleEvent(e);
            if (!e.isConsumed() && selections.Count > 0)
            {
                selections[currentSelection].HandleEvent(e);
            }
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

        /// <summary>
        /// Action to perform when the Up Arrow is pressed
        /// </summary>
        private class UpAction : IAction
        {
            // SelectionMenu to act on
            private SelectionMenu owner;

            public UpAction(SelectionMenu menu)
            {
                this.owner = menu;
            }
            // Move the selection up
            public void Perform()
            {
                owner.MoveToPrevSel();
            }
        }

        private class DownAction : IAction
        {
            // Menu to act on
            private SelectionMenu owner;

            public DownAction(SelectionMenu menu)
            {
                this.owner = menu;
            }

            //Move selection down
            public void Perform()
            {
                owner.MoveToNextSel();
            }
        }
    }
}
