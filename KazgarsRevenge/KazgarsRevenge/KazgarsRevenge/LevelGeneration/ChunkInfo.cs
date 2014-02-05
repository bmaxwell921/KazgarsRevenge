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
        // Which 'directions' of this chunk have doors. Directions are post rotation directions
        // ie a chunk with doors at N and E with Rotation.Zero would have doorDirections of W and N after at Rotation.NINETY
        private ISet<Direction> doorDirections;

        // The rotation of this chunk
        private Rotation rotation;

        // The id of the represented chunk
        private int id;

        /// <summary>
        /// Constructs a new ChunkInfo object with doors at the given directions
        /// </summary>
        /// <param name="directions"></param>
        public ChunkInfo(int id, Rotation rotation, params Direction[] doorDirections)
        {
            this.id = id;
            this.rotation = rotation;
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
            return String.Format("ChunkInfo:" + doorDirections.ToString());
        }

        public override bool Equals(object obj)
        {
            return obj != null && (obj as ChunkInfo).doorDirections.Equals(doorDirections);
        }

        public override int GetHashCode()
        {
            return doorDirections.GetHashCode();
        }
    }
}
