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
        public Vector2 position;
        public float alpha;
        public string text;
        public FloatingText(Vector2 position, string text)
        {
            this.alpha = 1;
            this.position = position;
            this.text = text;
        }

    }
}
