using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// A Menu for the level selection screen
    /// </summary>
    public class LevelSelectMenu : LinkedMenu
    {
        private static readonly string LEVEL_PREPEND = "Level ";
        private static readonly string LOCKED_PREPEND = "(LOCKED) ";

        private static readonly string BACK = "Back";

        // Where to start drawing level names
        private Vector2 levelsDrawLoc;
        public LevelSelectMenu(MenuManager mm, string title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds, Vector2 levelsDrawLoc)
            : base(mm, title, drawLocation, background, backgroundBounds)
        {
            this.levelsDrawLoc = levelsDrawLoc;
        }

        public override void Load(object info)
        {
            base.Load(info);
            Account acctInfo = (Account)info;
            Vector2 nextDrawLoc = levelsDrawLoc;

            float offset = mm.normalFont.MeasureString(LEVEL_PREPEND).Y * mm.guiScale.Y;

            // Levels they can go to
            for (int i = 1; i <= acctInfo.UnlockedFloors; ++i)
            {
                base.AddSelection(new SelectionV2(mm, LEVEL_PREPEND + i, nextDrawLoc), mm.menus[MenuManager.LOADING]);
                nextDrawLoc.Y += offset;
            }

            // Levels they can't go to
            for (int i = acctInfo.UnlockedFloors + 1; i <= Constants.MAX_LEVELS; ++i)
            {
                base.AddSelection(new SelectionV2(mm, LOCKED_PREPEND + LEVEL_PREPEND + i, nextDrawLoc, false), null);
                nextDrawLoc.Y += offset;
            }

            base.AddSelection(new SelectionV2(mm, BACK, nextDrawLoc + new Vector2(0, offset), true), mm.menus[MenuManager.ACCOUNTS]);
        }

        /// <summary>
        /// Returns the number of the level they selected
        /// </summary>
        /// <returns></returns>
        public override object Unload()
        {
            if (currentSel != base.selections.Count - 1)
            {
                SelectionV2 sel = base.selections[currentSel].sel;
                // Name is Level #, we just want the int
                string level = sel.name.Split(' ')[1];
                return Convert.ToInt32(level);
            }
            return -1;
        }
    }
}
