using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KazgarsRevenge
{
    /// <summary>
    /// Logger that writes to a file
    /// </summary>
    public class FileWriteLogger : ALogger
    {
        // Used as constructor params
        public static readonly string SERVER_SUB_DIR = "Server";
        public static readonly string CLIENT_SUB_DIR = "Client";


        private static readonly string LOG_DIR = "Logging";
        private static readonly string LOG_EXTENSION = ".log";
        
        // Location of all the logs
        public readonly string FILE_DIRECTORY;
        
        // Format for file names
        public static readonly string FILE_NAME_FORMAT = "yyyy-MM-dd-HH-mm-ss";

        // Each file name will the time when this logger was created to avoid overwriting
        private string filePath;

        /// <summary>
        /// Creates a FileWriteLogger. subDir should be 'Client' for clients and 'Server' for servers
        /// </summary>
        /// <param name="subDir"></param>
        public FileWriteLogger(string subDir)
        {
            FILE_DIRECTORY = Path.Combine(Directory.GetCurrentDirectory(), LOG_DIR, subDir);
            filePath = Path.Combine(FILE_DIRECTORY, DateTime.Now.ToString(FILE_NAME_FORMAT) + LOG_EXTENSION);
            CreateLoggingFile(filePath);
        }

        public override void Log(Level level, string message)
        {
            string output = String.Format(ALogger.OUTPUT_FORMAT, level, message);
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(output);
            }
        }

        private void CreateLoggingFile(string fileName)
        {
            if (!Directory.Exists(FILE_DIRECTORY))
            {
                Directory.CreateDirectory(FILE_DIRECTORY);
            }
            if (!File.Exists(fileName))
            {
                using (File.CreateText(fileName))
                {
                    /*Lol do nothing*/
                }
            }
        }
    }
}
