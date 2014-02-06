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

        public override object Clone()
        {
            Room clone = new Room();
            clone.location = (Location)this.location.Clone();
            clone.name = (string)this.name.Clone();
            clone.rotation = this.rotation;

            List<RoomBlock> blocks = new List<RoomBlock>();
            foreach (RoomBlock rb in blocks)
            {
                blocks.Add((RoomBlock)rb.Clone());
            }
            clone.blocks = blocks;
            return clone;
        }
    }
}
