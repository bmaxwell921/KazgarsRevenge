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
        private string outerKey;
        private string innerKey;
        public int LineCount { get { return lines == null ? 0 : lines.Count; } }
        Texture2D texHover = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.HOVER);
        Texture2D gotIt = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Place_Holder);
        Texture2D dontShowAgain = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.HOVER);

        public HelpPopUp(List<TooltipLine> lines, string outerKey, string innerKey)
        {
            this.lines = lines;
            this.outerKey = outerKey;
            this.innerKey = innerKey;
        }

        public void Draw(SpriteBatch s, Vector2 topLeft, SpriteFont font, float scale, float lineHeight, Rectangle gotItRect, Rectangle dontShowRect, Dictionary<string, Dictionary<string, Rectangle>> inners)
        {
            float y = 0;
            for (int i = 0; i < lines.Count; ++i)
            {
                s.DrawString(font, lines[i].text, topLeft + new Vector2(0, y), lines[i].color, 0, Vector2.Zero, scale * lines[i].scale, SpriteEffects.None, 0);
                y += lines[i].scale * lineHeight * scale;
            }

            if (outerKey != null && innerKey != null)
            {
                s.Draw(texHover, inners[outerKey][innerKey], Color.White);
            }
            s.Draw(gotIt, gotItRect, Color.White);
            //s.Draw(dontShowAgain, dontShowRect, Color.White);
        }


        #region PopUp definitions
        public static HelpPopUp GetMapPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Press M for Map!", .65f), new TooltipLine(Color.Gold, "Your map is your guide inside of", .4f), new TooltipLine(Color.Gold, "Kazgar's tower. Use it to help you", .4f), new TooltipLine(Color.Gold, "locate the key and defeat the boss.", .4f) }, "buttons", "map");
        }

        public static HelpPopUp GetInventoryPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "I for Inventory!", .65f), new TooltipLine(Color.Gold, "Your Inventory holds all the loot", .4f), new TooltipLine(Color.Gold, "you find on your quest to exact", .4f), new TooltipLine(Color.Gold, "revenge!", .4f) }, "buttons", "inventory");
        }

        public static HelpPopUp GetInventoryPopup2()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Mind Your Space!", .65f), new TooltipLine(Color.Gold, "Your Inventory only has 16 slots", .4f), new TooltipLine(Color.Gold, "for loot!  Visit town to sell and", .4f), new TooltipLine(Color.Gold, "store items.", .4f) }, "buttons", "inventory");
        }

        public static HelpPopUp GetCharacterPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "What a Stud!", .65f), new TooltipLine(Color.Gold, "Press C to open your character", .4f), new TooltipLine(Color.Gold, "menu.  Here you can see your stats", .4f), new TooltipLine(Color.Gold, "and equip gear!", .4f) }, "buttons", "character");
        }

        public static HelpPopUp GetEquipPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Drag and Drop!", .65f), new TooltipLine(Color.Gold, "Equipping gear is easy!  Just", .4f), new TooltipLine(Color.Gold, "click and drag something from your", .4f), new TooltipLine(Color.Gold, " inventory into the correct slot.", .4f) }, "buttons", "character");
        }

        public static HelpPopUp GetAbilityPopup()
        {

            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Use Your Abilities!", .65f), new TooltipLine(Color.Gold, "Right click or drag an unlocked", .4f), new TooltipLine(Color.Gold, "ability to place it on your", .4f), new TooltipLine(Color.Gold, "ability bar.", .4f) }, "extras", "abilities");
        }

        public static HelpPopUp GetTalentPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Very Talented!", .65f), new TooltipLine(Color.Gold, "Press T to open your talents!", .4f), new TooltipLine(Color.Gold, "Here you can assign points gained", .4f), new TooltipLine(Color.Gold, "from leveling up!", .4f) }, "buttons", "talents");

        }

        public static HelpPopUp GetTalentPopup2()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Passive vs Active!", .65f), new TooltipLine(Color.Gold, "You have two types of talents.", .4f), new TooltipLine(Color.Gold, "Active talents are used in combat.", .4f), new TooltipLine(Color.Gold, "Passive talents are always active.", .4f) }, "buttons", "talents");
        }

        public static HelpPopUp GetTalentPopup3()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Actives in Action!", .65f), new TooltipLine(Color.Gold, "Active talents have a red border.", .4f), new TooltipLine(Color.Gold, "Some active talents cost power.", .4f), new TooltipLine(Color.Gold, "Information is in the tooltips.", .4f) }, "buttons", "talents");
        }

        public static HelpPopUp GetPowerPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "I Have the Power!", .65f), new TooltipLine(Color.Gold, "Power is generated by hitting", .4f), new TooltipLine(Color.Gold, "enemies with basic attacks and", .4f), new TooltipLine(Color.Gold, "abilities.", .4f) }, "player", "power");
            
        }

        public static HelpPopUp GetHealthPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Thrive and Survive!", .65f), new TooltipLine(Color.Gold, "When in combat be sure to keep", .4f), new TooltipLine(Color.Gold, "an eye on your health.  Regen", .4f), new TooltipLine(Color.Gold, "using potions and abilities.", .4f) }, "player", "hp");
            
        }

        public static HelpPopUp GetLootPopup()
        {
            return new HelpPopUp(new List<TooltipLine> { new TooltipLine(Color.White, "Collect the Spoils!", .65f), new TooltipLine(Color.Gold, "After slaying an enemy, press", .4f), new TooltipLine(Color.Gold, "SPACE to loot its soul.", .4f) }, null, null);
        }
        #endregion
    }
}
