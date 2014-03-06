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
            // All the accounts are our selections
            accounts = AccountUtil.Instance.GetAccounts();
            if (accounts.Count > 0)
            {
                float yOffset = mm.normalFont.MeasureString(accounts[0].Name).Y;
                // Create a selection for each
                for (int i = 0; i < accounts.Count; ++i)
                {
                    Account account = accounts[i];
                    base.AddSelection(new SelectionV2(base.mm, account.Name, acctsDrawLoc + new Vector2(0, yOffset * i)), (LinkedMenu)mm.menus[MenuManager.LEVELS]);
                }
            }
        }

        /// <summary>
        /// Returns the account that was selected
        /// </summary>
        /// <returns></returns>
        public override object Unload()
        {
            mm.SetPlayerAccount(accounts[currentSel]);
            // Accounts and selections should match
            return accounts[currentSel];
        }
    }
}
