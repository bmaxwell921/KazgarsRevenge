using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Utility used to interact with the game and the file system
    /// for user Accounts
    /// </summary>
    public class AccountUtil
    {
        #region Singleton Stuff
        // Singleton instance
        private static AccountUtil instance;

        // Accessor
        public static AccountUtil Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountUtil();
                }
                return instance;
            }
        }
        #endregion

        // All the accounts
        private IList<Account> accounts;

        private AccountUtil()
        {
            accounts = new List<Account>();
        }

        /// <summary>
        /// Gets all of the available accounts
        /// </summary>
        /// <returns></returns>
        public IList<Account> GetAccounts()
        {
            if (accounts.Count <= 0)
            {
                LoadAccounts();
            }

            return accounts;
        }

        // Loads all the accounts from file
        private void LoadAccounts()
        {
            // TODO actually load accounts from file
            DummyLoad();
        }

        // Load used for initial testing
        private void DummyLoad()
        {
            accounts.Add(new Account("Dummy Account 1", 1, 1, 0));
            accounts.Add(new Account("Dummy Account 2", 1, 2, 0));
            accounts.Add(new Account("Dummy Account 3", 1, 5, 0));
        }

        public void SaveAccount(Account account)
        {
            // TODO save to file
            // Update accounts list??
        }
    }
}
