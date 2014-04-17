using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class PopUpLine
    {
        public Color color;
        public string text;
        public float scale;
        public PopUpLine(Color color, string text, float scale)
        {
            this.color = color;
            this.text = text;
            this.scale = scale;
        }
    }
    public class HelpPopUp
    {
        private List<TooltipLine> lines;
        private Rectangle highlight;
        public int LineCount { get { return lines == null ? 0 : lines.Count; } }
        Texture2D texHover = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.HOVER);
        Texture2D gotIt = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Place_Holder);
        Texture2D dontShowAgain = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.HOVER);

        public HelpPopUp(List<TooltipLine> lines, Rectangle highlight)
        {
            this.lines = lines;
            this.highlight = highlight;
        }

        //Define tutorial popUps here!
        public static List<HelpPopUp> getTutorial(Dictionary<String, Rectangle> outers, Dictionary<string, Dictionary<string, Rectangle>> inners)
        {
            List<HelpPopUp> toRet = new List<HelpPopUp>();
            toRet.Add(new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Press M for Map!", .65f), new TooltipLine(Color.Gold, "Your map is guide inside of", .4f), new TooltipLine(Color.Gold, "Kazgar's tower.  Use it to help you", .4f), new TooltipLine(Color.Gold, "locate the key and defeat the boss.", .4f) }, outers["map"]));
            toRet.Add(new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "More Tuts", .65f), new TooltipLine(Color.Gold, "Write some damn lines!", .4f)}, outers["xp"]));

            return toRet;
        }

        public void Draw(SpriteBatch s, Vector2 topLeft, SpriteFont font, float scale, float lineHeight, Rectangle gotItRect, Rectangle dontShowRect)
        {
            float y = 0;
            for (int i = 0; i < lines.Count; ++i)
            {
                s.DrawString(font, lines[i].text, topLeft + new Vector2(0, y), lines[i].color, 0, Vector2.Zero, scale * lines[i].scale, SpriteEffects.None, 0);
                y += lines[i].scale * lineHeight;
            }

            s.Draw(texHover, highlight, Color.White);
            s.Draw(gotIt, gotItRect, Color.White);
            //s.Draw(dontShowAgain, dontShowRect, Color.White);
        }
    }
}
