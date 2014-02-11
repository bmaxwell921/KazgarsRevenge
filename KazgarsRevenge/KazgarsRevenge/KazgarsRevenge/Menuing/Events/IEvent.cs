using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Interface describing the behavior of an event
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Method to check whether one IEvent is equal to another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool Equals(IEvent other);

        /// <summary>
        /// Whether this Event has been consumed or not.
        /// Consumed events can no longer be processed
        /// </summary>
        bool isConsumed();

        /// <summary>
        /// Consume this event, so it can't be processed further
        /// </summary>
        void Consume();
    }
}
