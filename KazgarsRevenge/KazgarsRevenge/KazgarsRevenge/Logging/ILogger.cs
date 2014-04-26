using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Info: Just for normal information about what's going on
    /// Debug: Stuff to output to help fix problems
    /// Error: When bad things happen
    /// 
    /// Ordered from least severe to most
    /// </summary>
    public enum LogLevel {INFO, DEBUG, ERROR};

    /// <summary>
    /// Defines behavior for loggers
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the given message
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        void Log(LogLevel level, string message);
    }
}
