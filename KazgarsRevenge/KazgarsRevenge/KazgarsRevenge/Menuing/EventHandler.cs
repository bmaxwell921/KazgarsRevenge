using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Interface describing the behavior for an event handler
    /// </summary>
    public interface EventHandler
    {
        /// <summary>
        /// Handles the event as necessary
        /// </summary>
        /// <param name="e"></param>
        void HandleEvent(IEvent e);

        /// <summary>
        /// Returns true or false based on whether this EventHandler
        /// handles the given IEvent
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool Handles(IEvent e);

        /// <summary>
        /// Registers the EventHandler to handle a type of event
        /// </summary>
        /// <param name="e"></param>
        void Register(IEvent e, IAction action);
    }
}
