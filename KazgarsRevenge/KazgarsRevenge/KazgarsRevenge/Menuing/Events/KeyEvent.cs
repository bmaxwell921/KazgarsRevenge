using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace KazgarsRevenge
{
    /// <summary>
    /// A class used to represent a key press
    /// </summary>
    public class KeyEvent : IEvent
    {
        // The key associated with this event
        public Keys key;

        public KeyEvent(Keys key)
        {
            this.key = key;
        }

        /// <summary>
        /// Checks to see if the key associated with this KeyEvent matches the 
        /// Key associated with the other event
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IEvent other)
        {
            KeyEvent otherKE = other as KeyEvent;
            if (otherKE == null)
            {
                return false;
            }

            return this.key.Equals(otherKE.key);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IEvent);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }
    }
}
