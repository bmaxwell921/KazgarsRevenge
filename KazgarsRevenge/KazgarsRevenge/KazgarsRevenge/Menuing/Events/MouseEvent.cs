using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to represent a mouse click
    /// </summary>
    public class MouseEvent : AEvent
    {
        // The location that was clicked
        public Vector2 clickLoc;

        /// <summary>
        /// Constructs a new MouseEvent with the given location that
        /// was clicked
        /// </summary>
        /// <param name="clickLoc"></param>
        public MouseEvent(Vector2 clickLoc)
        {
            this.clickLoc = clickLoc;
        }

        /// <summary>
        /// Checks if the click location matches the other event.
        /// Shouldn't really be needed
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(IEvent other)
        {
            MouseEvent otherME = other as MouseEvent;
            if (otherME == null)
            {
                return false;
            }
            return this.clickLoc.Equals(otherME.clickLoc);
        }

        // TODO add a clickedInArea method??

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IEvent);
        }

        public override int GetHashCode()
        {
            return clickLoc.GetHashCode();
        }
    }
}
