using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Rooms are made up of blocks. These objects are used to build the movement graph
    /// Copied from the KRChunkEditor for easy deserialization
    /// </summary>
    public class RoomBlock : ChunkComponent
    {
        public static readonly int SIZE = 1;
    }
}
