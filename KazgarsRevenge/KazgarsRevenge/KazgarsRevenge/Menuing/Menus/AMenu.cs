using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public abstract class AMenu : IMenu
    {
        private static readonly Color TITLE_COLOR = Color.White;

        // All the events that this Menu handles
        private IDictionary<IEvent, IAction> handledEvents;

        // The title of this menu
        public string title;

        // Where to draw this menu
        public Vector2 titleLoc;

        // SpriteBatch used for drawing
        protected SpriteBatch sb;

        // SpriteFont used for drawing
        protected SpriteFont sf;

        // Center of the string, used to center text
        protected Vector2 drawCenter;

        // Scale used for drawing
        private Vector2 guiScale;

        // Used for drawing background
        private Rectangle screenSize;

        // Background to draw, if any
        private Texture2D background;

        public AMenu(SpriteBatch sb, string title, SpriteFont sf, Vector2 guiScale, Vector2 titleLoc, Rectangle screenSize = new Rectangle(), Texture2D background = null)
        {
            this.sb = sb;
            this.title = title;
            this.sf = sf;
            this.guiScale = guiScale;
            this.titleLoc = titleLoc;
            this.screenSize = screenSize;
            this.background = background;
            this.handledEvents = new Dictionary<IEvent, IAction>();

            drawCenter = sf.MeasureString(title) / 2;
        }

        public virtual void HandleEvent(IEvent e)
        {
            if (!Handles(e))
            {
                return;
            }
            handledEvents[e].Perform();

            // Don't let anyone else use it
            e.Consume();
        }

        public bool Handles(IEvent e)
        {
            return handledEvents.ContainsKey(e);
        }

        public void Register(IEvent e, IAction action)
        {
            handledEvents[e] = action;
        }

        public abstract void Load();

        public abstract void Unload();

        public virtual void Draw(GameTime gameTime)
        {
            // Only draw if given a background
            if (background != null)
            {
                sb.Draw(background, screenSize, Color.White);
            }
            sb.DrawString(sf, title, titleLoc, TITLE_COLOR, 0, drawCenter, guiScale, SpriteEffects.None, 0);
        }
    }
}
