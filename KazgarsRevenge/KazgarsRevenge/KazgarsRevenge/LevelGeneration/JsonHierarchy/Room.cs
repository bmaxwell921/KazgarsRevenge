using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to represent a Room in Kazgar's Revenge
    /// Copied from the KRChunkEditor for easy deserialization
    /// </summary>
    public class Room : ChunkComponent
    {
        // All the blocks that make up this room
        public List<RoomBlock> blocks
        {
            get;
            set;
        }
    }
}
