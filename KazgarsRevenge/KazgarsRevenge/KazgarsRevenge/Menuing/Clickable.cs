using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Interface describing the behavior of objects that are clickable
    /// </summary>
    public interface Clickable
    {
        /// <summary>
        /// Returns the bounding box for this Clickable
        /// </summary>
        /// <returns></returns>
        Rectangle SelectableArea();

        /// <summary>
        /// Sets the bounding box for this Clickable
        /// </summary>
        /// <param name="bound"></param>
        void SetSelectableArea(Rectangle bound);
    }
}
