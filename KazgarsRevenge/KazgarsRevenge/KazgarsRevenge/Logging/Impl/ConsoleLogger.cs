using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Logs messages to the console
    /// </summary>
    public class ConsoleLogger : ALogger
    {
        public ConsoleLogger()
            : base()
        {
        }

        public override void Log(LogLevel level, string message)
        {
            Console.WriteLine(ALogger.OUTPUT_FORMAT, level, message);
        }
    }
}
