using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Class used to hold information about the game
    /// </summary>
    class ServerConfig
    {
        // file where all the config is stored
        public static readonly string CONFIG_FILE_PATH = "server.config";
        public static readonly string READABLE_SERVER_NAME_KEY = "Server Name";
        public static readonly string READABLE_MAX_NUM_PLAYERS_KEY = "Max Players";
        public static readonly char FILE_KEY_VALUE_SEPARATOR = '=';

        #region Defaults
        // Defaults
        public static readonly string DEFAULT_SERVER_NAME = "Server Name";
        public static readonly int DEFAULT_MAX_NUM_PLAYERS = 5;
        public static readonly int MAX_NUM_PLAYERS = 5;
        #endregion

        private LoggerManager lm;

        #region Values
        // name of the server sent to clients
        public string serverName
        {
            get;
            set;
        }

        // max number of clients connecting
        private int _maxNumPlayers;
        // accessor
        public int maxNumPlayers
        {
            get
            {
                return _maxNumPlayers;
            }

            set
            {
                if (value > MAX_NUM_PLAYERS || value <= 0)
                {
                    lm.Log(Level.INFO, String.Format("Invalid max number of players: %d. Using default value of: %d.", value, DEFAULT_MAX_NUM_PLAYERS));
                    value = DEFAULT_MAX_NUM_PLAYERS;
                }
                _maxNumPlayers = value;
            }
        }
        #endregion

        // TODO should this have the level width?
        // public int levelWidth;

        public ServerConfig(LoggerManager lm)
            : this(DEFAULT_SERVER_NAME, DEFAULT_MAX_NUM_PLAYERS)
        {
            this.lm = lm;
        }

        public ServerConfig(string serverName, int maxNumPlayers)
        {
            this.serverName = serverName;
            this.maxNumPlayers = maxNumPlayers;
        }

        public override string ToString()
        {
            string keyValueSep = " " + FILE_KEY_VALUE_SEPARATOR + " ";
            StringBuilder sb = new StringBuilder();
            sb.Append(READABLE_SERVER_NAME_KEY).Append(keyValueSep).AppendLine(serverName);
            sb.Append(READABLE_MAX_NUM_PLAYERS_KEY).Append(keyValueSep).Append(maxNumPlayers);
            return sb.ToString();
        }
    }
}
