using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class FloatingText
    {
        public float scale = 1;
        public Color color = Color.White;
        public Vector2 position;
        public float alpha;
        public string text;
        public FloatingText(Vector2 position, string text)
        {
            this.alpha = 1;
            this.position = position;
            this.text = text;
        }

        public FloatingText(Vector2 position, string text, Color color)
        {
            this.alpha = 1;
            this.position = position;
            this.text = text;
            this.color = color;
        }

        public FloatingText(Vector2 position, string text, Color color, float scale)
        {
            this.alpha = 1;
            this.position = position;
            this.text = text;
            this.color = color;
            this.scale = scale;
        }

    }
}
