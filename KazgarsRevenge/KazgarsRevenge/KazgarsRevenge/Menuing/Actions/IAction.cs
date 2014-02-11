using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Interface describing the behavior of an Action
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Performs the action
        /// </summary>
        void Perform();
    }
}
