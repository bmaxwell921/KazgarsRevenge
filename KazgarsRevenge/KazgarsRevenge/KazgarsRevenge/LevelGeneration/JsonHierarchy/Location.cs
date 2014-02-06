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
        private int x;

        // Y component
        private int y;

        public Location()
            : this(0, 0)
        {
            
        }

        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int getX()
        {
            return x;
        }

        public void setX(int x)
        {
            this.x = x;
        }

        public int getY()
        {
            return y;
        }

        public void setY(int y)
        {
            this.y = y;
        }

        public object Clone()
        {
            return new Location(this.getX(), this.getY());
        }
    }
}
