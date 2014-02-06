using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Super type of RoomBlocks, Rooms, and Chunks
    /// Kind of copied from the KRChunkEditor for easy deserialization
    /// </summary>
    public class ChunkComponent
    {
        // Location of the component in x,y 
        public Location location
        {
            get;
            set;
        }

        // Name of the component
        public string name
        {
            get;
            set;
        }

        // Rotation of the component
        public Rotation rotation
        {
            get;
            set;
        }
    }
}
