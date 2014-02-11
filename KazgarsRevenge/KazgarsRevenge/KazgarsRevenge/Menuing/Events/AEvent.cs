using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Abstract class defining consumption behavior for events
    /// </summary>
    public abstract class AEvent : IEvent
    {
        // Whether or not this event has been consumed
        protected bool consumed;

        /// <summary>
        /// Constructs a new Event that has not been consumed
        /// </summary>
        public AEvent()
        {
            this.consumed = false;
        }

        public abstract bool Equals(IEvent other);

        /// <summary>
        /// Interface implementation
        /// </summary>
        /// <returns></returns>
        public bool isConsumed()
        {
            return this.consumed;
        }

        /// <summary>
        /// Interface implementation
        /// </summary>
        public void Consume()
        {
            this.consumed = true;
        }
    }
}
