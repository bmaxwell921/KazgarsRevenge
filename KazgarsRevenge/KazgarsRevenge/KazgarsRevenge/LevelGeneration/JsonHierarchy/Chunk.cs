using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to represent a chunk in Kazgar's Revenge
    /// Copied from the KRChunkEditor for easy deserialization
    /// </summary>
    public class Chunk : ChunkComponent
    {
        // The rooms that make up this chunk
        public List<Room> rooms
        {
            get;
            set;
        }

        public override object Clone()
        {
            Chunk ret = new Chunk();
            ret.name = (string) this.name.Clone();
            ret.location = (Location) this.location.Clone();
            ret.rotation = this.rotation;
            ret.rooms = new List<Room>();

            foreach (Room r in rooms)
            {
                ret.rooms.Add((Room)r.Clone());
            }
            return ret;
        }
    }
}
