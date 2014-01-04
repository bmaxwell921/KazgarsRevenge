using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public abstract class ALogger : ILogger
    {
        // 0th argument is the level, 1st is the message
        public static readonly string OUTPUT_FORMAT = "[{0}]: {1}";
        // Mapping to show which levels are enabled
        protected IDictionary<Level, bool> levelStatus;

        /// <summary>
        /// Constructs a new logger, by default all logging Levels are enabled
        /// </summary>
        public ALogger()
        {
            levelStatus = new Dictionary<Level, bool>();
            EnableLoggingLevels(Level.DEBUG, Level.ERROR, Level.INFO);
        }

        /// <summary>
        /// Checks to see if the given level of logging is enabled, then calls 
        /// subclass method to perform the loggging
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Log(Level level, string message)
        {
            if (!levelStatus[level])
            {
                return;
            }
            PerformLog(level, message);
        }

        public void EnableLoggingLevels(params Level[] levels)
        {
            SetLoggingLevels(levels, true);
        }

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

        /// <summary>
        /// Method implemented by subclasses to actually perform the logging
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        protected abstract void PerformLog(Level level, string message);
    }
}
