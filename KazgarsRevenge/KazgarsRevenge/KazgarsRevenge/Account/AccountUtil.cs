using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

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

        private static readonly String ACCT_PATH = "accounts";
        private static readonly String ACCT_EXT = ".json";

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
            CreateFiles();
        }

        private void CreateFiles()
        {
            if (!Directory.Exists(ACCT_PATH))
            {
                Directory.CreateDirectory(ACCT_PATH);
            }
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
            string[] saves = Directory.GetFiles(ACCT_PATH, "*" + ACCT_EXT).Select(path => Path.GetFileName(path)).ToArray();
            foreach (string save in saves)
            {
                using (StreamReader sr = new StreamReader(Path.Combine(ACCT_PATH, save)))
                {
                    string json = sr.ReadToEnd();
                    Account read = JsonConvert.DeserializeObject<Account>(json);
                    Console.WriteLine(read);
                    accounts.Add(read);
                }
            }
        }

        // Load used for initial testing
        private void DummyLoad()
        {
            accounts.Add(new Account("Dummy Account 1", 1, 1, 0));
            accounts.Add(new Account("Dummy Account 2", 1, 2, 0));
            accounts.Add(new Account("Dummy Account 3", 1, 5, 0));
        }

        /// <summary>
        /// Writes the account to a file
        /// </summary>
        /// <param name="account"></param>
        public void SaveAccount(Account account)
        {
            using (StreamWriter file = new StreamWriter(Path.Combine(ACCT_PATH, account.Name + ACCT_EXT)))
            {
                file.WriteLine(account.ToString());
            }
        }
    }
}
