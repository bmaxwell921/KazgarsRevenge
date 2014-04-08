using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class TitleMenu : LinkedMenu
    {
        private Vector2 optionsLoc;
        public TitleMenu(MenuManager mm, string title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds, Vector2 optionsDrawLoc)
            : base(mm, title, drawLocation, background, backgroundBounds)
        {
            this.optionsLoc = optionsDrawLoc;
        }

        public override void Load(object info)
        {
            base.Load(info);
            float yOffset = mm.normalFont.MeasureString(MenuManager.PLAY).Y * mm.guiScale.Y;
            base.AddSelection(new SelectionV2(base.mm, MenuManager.PLAY, optionsLoc, true), mm.menus[MenuManager.ACCOUNTS]);
            
            // TODO settings menu
            base.AddSelection(new SelectionV2(base.mm, MenuManager.SETTINGS, optionsLoc + new Vector2(0, yOffset), true), null);
        }
    }
}
