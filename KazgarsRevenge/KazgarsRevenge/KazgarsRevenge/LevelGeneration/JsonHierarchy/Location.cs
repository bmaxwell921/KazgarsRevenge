using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to represent the location of rooms/chunks.
    /// Copied from the KRChunkEditor for easy deserialization
    /// </summary>
    public class Location : ICloneable
    {
        // X component
        public int x
        {
            get;
            set;
        }

        // Y component
        public int y
        {
            get;
            set;
        }

        public Location()
            : this(0, 0)
        {
            
        }

        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public object Clone()
        {
            return new Location(x, y);
        }
    }
}
