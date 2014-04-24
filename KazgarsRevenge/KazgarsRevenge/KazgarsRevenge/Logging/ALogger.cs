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

        /// <summary>
        /// Constructs a new logger
        /// </summary>
        public ALogger()
        {
        }

        public abstract void Log(LogLevel level, string message);

        // Just use the name for hashes and equality
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return this.GetType().Name.Equals(obj.GetType().Name);
        }
    }
}
