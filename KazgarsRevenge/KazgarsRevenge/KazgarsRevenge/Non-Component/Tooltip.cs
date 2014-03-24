﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public struct TooltipLine
    {
        public Color color;
        public string text;
        public TooltipLine(Color color, string text)
        {
            this.color = color;
            this.text = text;
        }
    }
    public class Tooltip
    {
        private List<TooltipLine> lines;
        public Tooltip(List<TooltipLine> lines)
        {
            this.lines = lines;
        }

        public void Draw(SpriteBatch s, Vector2 topLeft, SpriteFont font, float scale, float lineHeight)
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                s.DrawString(font, lines[i].text, topLeft + new Vector2(lineHeight) * i, lines[i].color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
        }
    }
}
