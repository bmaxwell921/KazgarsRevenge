using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using KazgarsRevenge;


namespace KazgarsRevengeServer
{
    /// <summary>
    /// Class used to read the server config from a file
    /// </summary>
    class ServerConfigReader
    {
        // Mapping from human readable config names and ServerConfig variable names
        private static IDictionary<string, string> variableMap;

        static ServerConfigReader()
        {
            variableMap = new Dictionary<string, string>();
            variableMap[ServerConfig.READABLE_SERVER_NAME_KEY] = "serverName";
            variableMap[ServerConfig.READABLE_MAX_NUM_PLAYERS_KEY] = "maxNumPlayers";
        }

        public static ServerConfig ReadConfig(LoggerManager lm)
        {
            ServerConfig serverConfig = new ServerConfig(lm);
            string configFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), ServerConfig.CONFIG_FILE_PATH);

            if (!File.Exists(configFilePath))
            {
                lm.Log(Level.INFO, "Server.config file doesn't exist, generating one and using defaults.");
                
                // fire up a new thread to save...and do some magic with lambda expressions to make it work
                new Thread(() => CreateConfigFile(configFilePath, serverConfig)).Start();
                // default values
                return serverConfig;
            }

            lm.Log(Level.INFO, "Reading server.config file");
            ReadConfigFile(configFilePath, serverConfig, lm);

            return serverConfig;
        }

        // creates and writes a new config file 
        private static void CreateConfigFile(string filePath, ServerConfig serverConfig)
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.Write(serverConfig.ToString());
                sw.Flush();
            }
        }

        // reads the config file into serverConfig
        private static void ReadConfigFile(string filePath, ServerConfig serverConfig, LoggerManager lm)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (!ValidLine(line, lm))
                    {
                        continue;
                    }
                    string[] splat = line.Split(ServerConfig.FILE_KEY_VALUE_SEPARATOR);
                    SetConfigValue(splat[0].Trim(), splat[1], serverConfig, lm);
                }
            }
        }

        /* 
         * config lines have to be in the form:
         *  configKey = configValue
         */ 
        private static bool ValidLine(String line, LoggerManager lm)
        {
            string[] splat = line.Split(ServerConfig.FILE_KEY_VALUE_SEPARATOR);
            if (splat.Count() != 2)
            {
                lm.Log(Level.DEBUG, String.Format("Invalid server config line: %s\n\tIncorrect number of equals signs.", line));
                return false;
            }

            if (!variableMap.ContainsKey(splat[0].Trim()))
            {
                lm.Log(Level.DEBUG, String.Format("Invalid server config line: %s\n\tUnrecognized key.", line));
                return false;
            }

            return true;
        }

        /*
         * Uses reflection to set the correct serverConfig property if it exists
         */
        private static void SetConfigValue(string key, string value, ServerConfig serverConfig, LoggerManager lm)
        {
            string propName = variableMap[key];
            PropertyInfo prop = serverConfig.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);

            if (prop == null || !prop.CanWrite)
            {
                lm.Log(Level.DEBUG, String.Format("Unable to write property named: %s\n\tMake sure it's a property...not just a variable", propName));
                return;
            }
            try
            {
                prop.SetValue(serverConfig, Convert.ChangeType(value, prop.PropertyType), null);
            }
            catch (Exception e)
            {
                lm.Log(Level.DEBUG, String.Format("Unable to set property: %s", propName));
                lm.Log(Level.DEBUG, e.StackTrace);
            }
        }
    }
}
