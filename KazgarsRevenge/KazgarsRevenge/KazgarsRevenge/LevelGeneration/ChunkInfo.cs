using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    // The 'type' of chunks. Just used for generation
    public enum ChunkType
    {
        // Home is the 'home' area at the base of the tower, soulevator is the middle chunk, etc
        HOME, SOULEVATOR, KEY, BOSS, NORMAL
    }

    /// <summary>
    /// Class used to hold information about a given chunk.
    /// Used when generating levels
    /// </summary>
    public class ChunkInfo
    {
        // Figured this was the best place for these?
        public static readonly string CHUNK_EXT = "json";
        public static readonly char CHUNK_NAME_DELIMIT = '-';

        private string _FileName;

        public string FileName
        {
            get
            {
                return new StringBuilder().Append(_FileName).Append(".").Append(CHUNK_EXT).ToString();
            }
            private set
            {
                _FileName = value;
            }
        }

        // Which 'directions' of this chunk have doors. Directions are post rotation directions
        // ie a chunk with doors at N and E with Rotation.Zero would have doorDirections of W and N after at Rotation.NINETY
        private ISet<Direction> doorDirections;

        // The id of the represented chunk
        public int id
        {
            get;
            protected set;
        }

        // The string version of the id, so we don't lose leading zeros
        private string idStr;
        
        // The rotation of this chunk
        public Rotation rotation
        {
            get;
            protected set;
        }

        // The type for this chunk
        public ChunkType chunkType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Constructs a new ChunkInfo object with doors at the given directions
        /// </summary>
        /// <param name="directions"></param>
        public ChunkInfo(string fileName, string id, Rotation rotation, ChunkType chunkType, ISet<Direction> doorDirections)
        {
            this.FileName = fileName;
            this.idStr = id;
            this.id = Convert.ToInt32(id);
            this.rotation = rotation;
            this.chunkType = chunkType;
            this.doorDirections = new HashSet<Direction>();
            foreach (Direction dir in doorDirections)
            {
                this.doorDirections.Add(dir);
            }
        }

        public bool hasDoorAt(Direction direction)
        {
            return doorDirections.Contains(direction);
        }

        /// <summary>
        /// Checks to see if this Chunk has doors in all the right places ;)
        /// </summary>
        /// <param name="reqDir"></param>
        /// <returns></returns>
        public bool satisfiesDoorRequirement(ISet<Direction> reqDirs)
        {
            foreach (Direction reqDir in reqDirs)
            {
                if (!this.hasDoorAt(reqDir))
                {
                    return false;
                }
            }
            return true;
        }

        public int numDoors()
        {
            return doorDirections.Count();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ChunkInfo:");
            foreach (Direction dir in doorDirections)
            {
                sb.Append(dir.ToChar());
            }
            return sb.ToString();
        }

        //public string GetFileName(FloorName floor)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(idStr).Append(CHUNK_NAME_DELIMIT).Append(floor.ToInt()).Append(CHUNK_NAME_DELIMIT).Append(chunkType.ToChar()).Append(CHUNK_NAME_DELIMIT);
        //    foreach (Direction dir in doorDirections)
        //    {
        //        sb.Append(dir.ToChar());
        //    }
        //    sb.Append(".").Append(CHUNK_EXT);
        //    return sb.ToString();
        //}

        public override bool Equals(object obj)
        {
            return obj != null && (obj as ChunkInfo).doorDirections.Equals(doorDirections);
        }

        public override int GetHashCode()
        {
            int PRIME = 31;
            int hash = 1;

            hash = hash * PRIME + id.GetHashCode();
            hash = hash * PRIME + rotation.GetHashCode();
            hash = hash * PRIME + chunkType.GetHashCode();
            hash = hash * PRIME + doorDirections.GetHashCode();

            return hash;
        }
    }
}
