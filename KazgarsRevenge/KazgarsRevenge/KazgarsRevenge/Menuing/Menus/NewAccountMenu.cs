using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KazgarsRevenge
{
    public class NewAccountMenu : LinkedMenu
    {
        public static readonly int NAME_LENGTH_LIMIT = 12;
        private static readonly string BIG_CHARACTER = "m";

        private static readonly string SAVE_STR = "Save";
        private static readonly string BACK_STR = "Back";

        // Where they input the name
        private TextBox input;
        private Vector2 textBoxLoc;

        public NewAccountMenu(MenuManager mm, string  title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds, Vector2 textBoxLoc) 
            : base(mm, title, drawLocation, background, backgroundBounds)
        {
            input = new TextBox(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.INPUT_TEXTURE), Texture2DUtil.Instance.GetTexture(TextureStrings.WHITE), mm.normalFont);
            input.Width = (int)(NAME_LENGTH_LIMIT * mm.normalFont.MeasureString(BIG_CHARACTER).X * mm.guiScale.X) + 5;
            input.X = (int)textBoxLoc.X - input.Width / 2;
            input.Y = (int)textBoxLoc.Y;
            input.guiScale = mm.guiScale;
            this.textBoxLoc = textBoxLoc;
            input.Selected = true;
        }

        public override void Load(object info)
        {
            base.Load(info);
            float yOffset = mm.normalFont.MeasureString(SAVE_STR).Y * mm.guiScale.Y;
            base.AddSelection(new SelectionV2(base.mm, SAVE_STR, textBoxLoc + new Vector2(0, yOffset * 2), true), base.mm.menus[MenuManager.ACCOUNTS]);
            base.AddSelection(new SelectionV2(base.mm, BACK_STR, textBoxLoc + new Vector2(0, yOffset * 3), true), base.mm.menus[MenuManager.ACCOUNTS]);
        }

        public override object Unload()
        {
            // Not the back key
            if (currentSel != base.selections.Count - 1 && input.Text.Length != 0)
            {
                AccountUtil.Instance.SaveAccount(new Account(input.Text));
            }
            input.ClearText();
            return null;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            input.Draw(mm.sb, gameTime);
        }

        public override bool ReceiveTextInput(char inputChar)
        {
            input.ReceiveTextInput(inputChar);
            return true;
        }

        public override bool ReceiveTextInput(string text)
        {
            input.ReceiveTextInput(text);
            return true;
        }

        public override bool ReceiveCommandInput(char command)
        {
            input.ReceiveCommandInput(command);
            return true;
        }
    }
}
