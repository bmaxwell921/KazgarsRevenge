using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;


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

        public static ServerConfig ReadConfig()
        {
            ServerConfig serverConfig = new ServerConfig();
            string configFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), ServerConfig.CONFIG_FILE_PATH);

            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("server.config file doesn't exist, generating one and using defaults.");
                
                // fire up a new thread to save...and do some magic with lambda expressions to make it work
                new Thread(() => CreateConfigFile(configFilePath, serverConfig)).Start();
                // default values
                return serverConfig;
            }

            ReadConfigFile(configFilePath, serverConfig);

            return serverConfig;
        }

        // creates and writes a new config file 
        private static void CreateConfigFile(string filePath, ServerConfig serverConfig)
        {
            StreamWriter sw = File.CreateText(filePath);
            sw.Write(serverConfig.ToString());
            sw.Flush();
            sw.Close();
        }

        // reads the config file into serverConfig
        private static void ReadConfigFile(string filePath, ServerConfig serverConfig)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (!ValidLine(line))
                    {
                        continue;
                    }
                    string[] splat = line.Split(ServerConfig.FILE_KEY_VALUE_SEPARATOR);
                    SetConfigValue(splat[0].Trim(), splat[1], serverConfig);
                }
            }
        }

        /* 
         * config lines have to be in the form:
         *  configKey = configValue
         */ 
        private static bool ValidLine(String line)
        {
            string[] splat = line.Split(ServerConfig.FILE_KEY_VALUE_SEPARATOR);
            if (splat.Count() != 2)
            {
                Console.WriteLine("Invalid server config line: %s\n\tIncorrect number of equals signs.");
                return false;
            }

            if (!variableMap.ContainsKey(splat[0].Trim()))
            {
                Console.WriteLine("Invalid server config line: %s\n\tUnrecognized key.");
                return false;
            }

            return true;
        }

        /*
         * Uses reflection to set the correct serverConfig property if it exists
         */
        private static void SetConfigValue(string key, string value, ServerConfig serverConfig)
        {
            string propName = variableMap[key];
            PropertyInfo prop = serverConfig.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);

            if (prop == null || !prop.CanWrite)
            {
                Console.Write("ERROR: Unable to write property named: %s\n\tMake sure it's a property...not just a variable", propName);
                return;
            }
            try
            {
                prop.SetValue(serverConfig, Convert.ChangeType(value, prop.PropertyType), null);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to set property: %s", propName);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
