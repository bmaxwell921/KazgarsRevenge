using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class TooltipLine
    {
        public Color color;
        public string text;
        public float scale;
        public TooltipLine(Color color, string text, float scale)
        {
            this.color = color;
            this.text = text;
            this.scale = scale;
        }
    }
    public class Tooltip
    {
        private List<TooltipLine> lines;
        public int LineCount { get { return lines == null? 0 : lines.Count;} }
        public Tooltip(List<TooltipLine> lines)
        {
            this.lines = lines;
        }

        public void Draw(SpriteBatch s, Vector2 topLeft, SpriteFont font, float scale, float lineHeight)
        {
            float y = 0;
            for (int i = 0; i < lines.Count; ++i)
            {
                s.DrawString(font, lines[i].text, topLeft + new Vector2(0, y), lines[i].color, 0, Vector2.Zero, scale * lines[i].scale * 2f, SpriteEffects.None, 0);
                y += lines[i].scale * lineHeight;
            }
        }
    }
}
