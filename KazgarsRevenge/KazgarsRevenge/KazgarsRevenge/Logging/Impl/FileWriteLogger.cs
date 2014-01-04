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
        // Location of all the logs
        public static readonly string FILE_DIRECTORY = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Logging");
        // Format for file names
        public static readonly string FILE_NAME_FORMAT = "yyyy-MM-dd-HH-mm";

        // Each file name will the time when this logger was created to avoid overwriting
        private string filePath;

        /// <summary>
        /// Creates a FileWriteLogger. subDir should be 'Client' for clients and 'Server' for servers
        /// </summary>
        /// <param name="subDir"></param>
        public FileWriteLogger(string subDir)
        {
            filePath = Path.Combine(FILE_DIRECTORY, subDir, DateTime.Now.ToString(FILE_NAME_FORMAT), ".log");
            CreateLoggingFile(filePath);
        }

        protected override void PerformLog(Level level, string message)
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
