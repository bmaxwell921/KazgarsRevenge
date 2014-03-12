using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KazgarsRevenge
{
    public abstract class SimpleMenu : IMenu
    {
        protected static readonly Color NORMAL_COLOR = Color.White;

        // Actor
        protected MenuManager mm;

        // Title of this menu
        protected string title;

        // Where we'll try to draw
        protected Vector2 drawLocation;
        
        // Center of the title so it's center justified
        protected Vector2 drawCenter;

        // Background to draw
        protected Texture2D background;

        // Where to draw it
        protected Rectangle backgroundBounds;

        /// <summary>
        /// Constructs a SimpleMenu with the given title and draw location
        /// </summary>
        /// <param name="mm"></param>
        /// <param name="title"></param>
        /// <param name="drawLocation"></param>
        public SimpleMenu(MenuManager mm, string title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds)
        {
            this.mm = mm;
            this.title = title;
            this.drawLocation = drawLocation;
            this.background = background;
            this.backgroundBounds = backgroundBounds;

            drawCenter = mm.titleFont.MeasureString(title) / 2;
        }

        public virtual void Draw(GameTime gameTime)
        {
            mm.sb.Draw(background, backgroundBounds, Color.White);
            // Just draw the title
            mm.sb.DrawString(mm.titleFont, title, drawLocation, NORMAL_COLOR, 0, drawCenter, mm.guiScale, SpriteEffects.None, 0);
        }

        public abstract void Load(object info);

        public abstract object Unload();

        #region Keyboard Registation
        public bool Selected { get; set; }

        // Default do nothing behavior
        public virtual bool ReceiveTextInput(char inputChar)
        {
            return false;
        }
        public virtual bool ReceiveTextInput(string text)
        {
            return false;
        }
        public virtual bool ReceiveCommandInput(char command)
        {
            return false;
        }
        public virtual bool ReceiveSpecialInput(Keys key)
        {
            return false;
        }
        #endregion
    }
}
