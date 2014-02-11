using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class to represent a Selection that contains clickable text on a menu
    /// </summary>
    public class TextSelection : Selection, Clickable
    {
        private static readonly Color SELECT_COLOR = Color.Yellow;
        private static readonly Color NORMAL_COLOR = Color.White;

        // The text to draw for this selection
        public string text;

        // SpriteBatch used for drawing
        private SpriteBatch sb;

        // Font used for drawing
        private SpriteFont sf;

        // Color used when drawing
        private Color drawColor;

        // Center the text in the middle of the drawArea
        private Vector2 drawLoc;

        // The 'center' (ie where to draw from) of the string
        private Vector2 drawCenter;

        // Duh
        private Vector2 guiScale;

        public TextSelection(SpriteBatch sb, String text, SpriteFont sf, Vector2 guiScale, IEvent e, IAction action, Rectangle drawArea)
            : base(e, action, drawArea)
        {
            this.sb = sb;
            this.text = text;
            this.sf = sf;
            this.drawColor = NORMAL_COLOR;

            // Middle of both
            drawLoc = new Vector2(drawArea.X, drawArea.Y) / 2;
            drawCenter = sf.MeasureString(text) / 2;
        }

        /// <summary>
        /// Implementation of interface
        /// </summary>
        /// <returns></returns>
        public Rectangle SelectableArea()
        {
            return base.drawArea;
        }

        /// <summary>
        /// Shouldn't be called
        /// </summary>
        /// <param name="bound"></param>
        public void SetSelectableArea(Rectangle bound)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the text color as Yellow
        /// </summary>
        public override void Highlight()
        {
            this.drawColor = SELECT_COLOR;
        }

        /// <summary>
        /// Sets the text color as White
        /// </summary>
        public override void DeHighlight()
        {
            this.drawColor = NORMAL_COLOR;
        }

        /// <summary>
        /// Just draws the text
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            sb.DrawString(sf, text, drawLoc, drawColor, 0, drawCenter, guiScale, SpriteEffects.None, 0);
        }
    }
}
