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

        //public Location location
        //{
        //    get;
        //    set;
        //}

        //// Name of the component
        //public string name
        //{
        //    get;
        //    set;
        //}

        //// Rotation of the component
        //public Rotation rotation
        //{
        //    get;
        //    set;
        //}
    }
}
