using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// This isn't really a manager based on our architecture, so that's why it's not in the Managers folder.
    /// This class is used to manage logging
    /// </summary>
    public class LoggerManager
    {
        // Mapping to show which levels are enabled
        protected IDictionary<Level, bool> levelStatus;

        private ISet<ILogger> loggers;

        /// <summary>
        /// Creates a LoggerManager with no loggers. By default all Logging levels are enabled
        /// </summary>
        public LoggerManager()
        {
            levelStatus = new Dictionary<Level, bool>();
            loggers = new HashSet<ILogger>();

            EnableLoggingLevels(Level.DEBUG, Level.ERROR, Level.INFO);
        }

        /// <summary>
        /// Logs the given message on all loggers held by this manager, if the given level is currently being logged
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Log(Level level, string message)
        {
            if (!LevelEnabled(level))
            {
                return;
            }

            foreach (ILogger il in loggers)
            {
                il.Log(level, message);
            }
        }

        private bool LevelEnabled(Level level)
        {
            return levelStatus.ContainsKey(level) && levelStatus[level];
        }

        // Adds if it doesn't exist
        public void AddLogger(ILogger logger)
        {
            if (loggers.Contains(logger))
            {
                return;
            }
            loggers.Add(logger);
        }

        // Removes if it exists
        public void RemoveLogger(ILogger logger)
        {
            if (!loggers.Contains(logger))
            {
                return;
            }
            loggers.Remove(logger);
        }

        /// <summary>
        /// Enables logging for the given levels. Level must be enabled for logging to occur
        /// 
        /// Note: params keyword works just like the ellipsis in java
        /// </summary>
        /// <param name="levels"></param>
        public void EnableLoggingLevels(params Level[] levels)
        {
            SetLoggingLevels(levels, true);
        }

        /// <summary>
        /// Disables logging for the given levels.
        /// </summary>
        /// <param name="levels"></param>
        public void DisableLoggingLevels(params Level[] levels)
        {
            SetLoggingLevels(levels, false);
        }

        private void SetLoggingLevels(Level[] levels, bool status)
        {
            foreach (Level l in levels)
            {
                levelStatus[l] = status;
            }
        }
    }
}
