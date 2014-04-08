using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class AccountMenu : LinkedMenu
    {
        private static readonly string NEW_ACCT = "New Account";

        // The accounts
        private IList<Account> accounts;

        // Where to start drawing accounts
        private Vector2 acctsDrawLoc;

        public AccountMenu(MenuManager mm, string title, Vector2 drawLocation, Texture2D background, Rectangle backgroundBounds, Vector2 acctsDrawLoc)
            : base(mm, title, drawLocation, background, backgroundBounds)
        {
            this.acctsDrawLoc = acctsDrawLoc;
        }

        /// <summary>
        /// Loads all of the accounts, does nothing with info
        /// </summary>
        /// <param name="info"></param>
        public override void Load(object info)
        {
            base.Load(info);
            // All the accounts are our selections
            accounts = AccountUtil.Instance.GetAccounts();
            float yOffset = mm.normalFont.MeasureString(NEW_ACCT).Y * mm.guiScale.Y;
            if (accounts.Count > 0)
            {
                // Create a selection for each
                for (int i = 0; i < accounts.Count; ++i)
                {
                    Account account = accounts[i];
                    base.AddSelection(new SelectionV2(base.mm, account.Name, acctsDrawLoc + new Vector2(0, yOffset * i)), mm.menus[MenuManager.LEVELS]);
                }
            }

            base.AddSelection(new SelectionV2(base.mm, NEW_ACCT, acctsDrawLoc + new Vector2(0, yOffset * (accounts.Count + 1))), mm.menus[MenuManager.NEW_ACCOUNT]);
            base.AddSelection(new SelectionV2(base.mm, "Back", acctsDrawLoc + new Vector2(0, yOffset * (accounts.Count + 2))), mm.menus[MenuManager.GAME_TITLE]);
        }

        /// <summary>
        /// Returns the account that was selected
        /// </summary>
        /// <returns></returns>
        public override object Unload()
        {
            if (currentSel < accounts.Count)
            {
                // Actually chose an account
                mm.SetPlayerAccount(accounts[currentSel]);
                // Accounts and selections should match
                return accounts[currentSel];
            }

            // Chose new
            return null;
        }
    }
}
