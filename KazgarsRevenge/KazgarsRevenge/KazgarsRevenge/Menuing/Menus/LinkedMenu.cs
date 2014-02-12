using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used for Menus that are linked together,
    /// Ie. you can get to one Menu from another
    /// </summary>
    public class LinkedMenu : SimpleMenu
    {
        protected static readonly Color SELECTED_COLOR = Color.Yellow;

        // The events that this Menu handles
        private IDictionary<IEvent, IAction> handledEvents;

        // The selections on this Menu and the LinkedMenu they go to
        protected IList<SelectionBox> selections;

        // The current selection
        protected int currentSel;

        /// <summary>
        /// Creates a new LinkedMenu with the given information
        /// </summary>
        /// <param name="mm"></param>
        /// <param name="title"></param>
        /// <param name="drawLocation"></param>
        public LinkedMenu(MenuManager mm, string title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds)
            : base(mm, title, drawLocation, background, backgroundBounds)
        {
            handledEvents = new Dictionary<IEvent, IAction>();
            this.selections = new List<SelectionBox>();
            this.currentSel = -1;

            SetUpHandledEvents();
        }

        private void SetUpHandledEvents()
        {
            handledEvents[new KeyEvent(Keys.Up)] = new UpAction(this);
            handledEvents[new KeyEvent(Keys.Down)] = new DownAction(this);
        }

        /// <summary>
        /// Adds a selection to this Menu which will take users
        /// to link if selected
        /// </summary>
        /// <param name="sel"></param>
        /// <param name="link"></param>
        public void AddSelection(SelectionV2 sel, IMenu link)
        {
            selections.Add(new SelectionBox(sel, link));
            if (selections.Count == 1)
            {
                currentSel = 0;
                selections[currentSel].sel.Highlight();
            }
        }

        /// <summary>
        /// Gets the Menu that is currently selected
        /// </summary>
        /// <returns></returns>
        public IMenu GetSelection()
        {
            // If we have stuff, and the thing that is selected can be selected
            if (selections.Count > 0 && selections[currentSel].sel.selectable)
            {
                return selections[currentSel].link;
            }

            // Returning null will keep us in the same location
            return null;
        }

        /// <summary>
        /// Moves the selection to the next one (ie down)
        /// </summary>
        private void MoveToNextSel()
        {
            if (selections.Count > 0)
            {
                // Wrapping
                int nextSel = (currentSel + 1) % selections.Count;
                MoveSelTo(currentSel, nextSel);
            }
        }

        /// <summary>
        /// Moves the selection to the previous one (ie up)
        /// </summary>
        private void MoveToPrevSel()
        {
            if (selections.Count > 0)
            {
                // Handle wrapping
                int nextSel = (currentSel - 1 < 0) ? selections.Count - 1 : currentSel - 1;
                MoveSelTo(currentSel, nextSel);
            }
        }

        private void MoveSelTo(int oldSel, int nextSel)
        {
            selections[oldSel].sel.DeHighlight();
            selections[nextSel].sel.Highlight();
            currentSel = nextSel;
        }

        public override void Load(object info)
        {
            // Do nothing
        }

        public override object Unload()
        {
            // Do nothing
            return null;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            foreach (SelectionBox sb in selections)
            {
                sb.sel.Draw();
            }
        }

        public override void HandleEvent(IEvent e)
        {
            /*
             * Down pressed: Go to next selection
             * Up pressed: Go to prev selection
             */
            if (!Handles(e))
            {
                return;
            }
            handledEvents[e].Perform();
        }

        public override bool Handles(IEvent e)
        {
            return handledEvents.ContainsKey(e);
        }

        public override void Register(IEvent e, IAction action)
        {
            handledEvents[e] = action;
        }

        /// <summary>
        /// Class used to box up a selection and the LinkedMenu 
        /// is corresponds with
        /// </summary>
        protected class SelectionBox
        {
            public SelectionV2 sel;
            public IMenu link;

            public SelectionBox(SelectionV2 sel, IMenu link)
            {
                this.sel = sel;
                this.link = link;
            }
        }

        /// <summary>
        /// Action to perform when the Up Arrow is pressed
        /// </summary>
        private class UpAction : IAction
        {
            // SelectionMenu to act on
            private LinkedMenu owner;

            public UpAction(LinkedMenu menu)
            {
                this.owner = menu;
            }
            // Move the selection up
            public void Perform()
            {
                owner.MoveToPrevSel();
            }
        }

        /// <summary>
        /// ACtion to perform when the Down Arrow is pressed
        /// </summary>
        private class DownAction : IAction
        {
            // Menu to act on
            private LinkedMenu owner;

            public DownAction(LinkedMenu menu)
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
