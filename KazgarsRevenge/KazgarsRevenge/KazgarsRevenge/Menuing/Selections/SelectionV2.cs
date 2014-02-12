using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Selection for a Menu
    /// </summary>
    public class SelectionV2
    {
        protected static readonly Color SELECT_COLOR = Color.Yellow;
        protected static readonly Color NORMAL_COLOR = Color.White;

        // Owner
        protected MenuManager mm;
        
        // The name of the selection
        public string name;

        // Where to draw
        protected Vector2 drawLocation;

        // Used to center justify text
        protected Vector2 drawCenter;

        // Color to draw with
        protected Color drawColor;

        public SelectionV2(MenuManager mm, string name, Vector2 drawLocation)
        {
            this.mm = mm;
            this.name = name;
            this.drawLocation = drawLocation;
            drawColor = NORMAL_COLOR;

            this.drawCenter = mm.normalFont.MeasureString(name) / 2;
        }

        /// <summary>
        /// Draws the name
        /// </summary>
        public virtual void Draw()
        {
            mm.sb.DrawString(mm.normalFont, name, drawLocation, drawColor, 0, drawCenter, mm.guiScale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Inform this selection that it's highlighted
        /// </summary>
        public virtual void Highlight()
        {
            drawColor = SELECT_COLOR;
        }

        /// <summary>
        /// Inform this selection that it's no longer highlighted
        /// </summary>
        public virtual void DeHighlight()
        {
            drawColor = NORMAL_COLOR;
        }

        /// <summary>
        /// Just compares by name
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            SelectionV2 otherSel = other as SelectionV2;
            if (otherSel == null)
            {
                return false;
            }

            return this.name.Equals(otherSel.name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
