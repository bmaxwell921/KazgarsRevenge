using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class FloatingText
    {
        public bool Dead { get; private set; }
        public float scale = 1;
        public Color color = Color.White;
        public Vector3 position;
        public float alpha;
        public string text;
        float risePerSec = 30;
        float alphaPerSec = .6f;

        public FloatingText(Vector3 position, string text, Color color, float scale)
        {
            this.alpha = 1;
            this.position = position;
            this.text = text;
            this.color = color;
            this.scale = scale;
        }

        public FloatingText(Vector3 position, string text, Color color, float scale, float risePerSec, float alphaPerSec)
        {
            this.alpha = 1;
            this.position = position;
            this.text = text;
            this.color = color;
            this.scale = scale;
            this.risePerSec = risePerSec;
            this.alphaPerSec = alphaPerSec;
        }

        public void Update(double elapsed)
        {
            alpha -= (float)elapsed * alphaPerSec / 1000.0f;
            position.Y += (float)elapsed * risePerSec / 1000.0f;
            if (alpha <= 0)
            {
                Dead = true;
            }
        }

    }
}
