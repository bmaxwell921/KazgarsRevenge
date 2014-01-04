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
    public enum Level {INFO, DEBUG, ERROR};

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
        void Log(Level level, string message);

        /// <summary>
        /// Enables logging for the given levels. Method calls to Log at a level not found 
        /// in the given array will not be logged.
        /// 
        /// Note: params keyword works just like the ellipsis in java
        /// </summary>
        /// <param name="levels"></param>
        void EnableLoggingLevels(params Level[] levels);

        /// <summary>
        /// Disables logging for the given levels.
        /// </summary>
        /// <param name="levels"></param>
        void DisableLoggingLevels(params Level[] levels);
    }
}
