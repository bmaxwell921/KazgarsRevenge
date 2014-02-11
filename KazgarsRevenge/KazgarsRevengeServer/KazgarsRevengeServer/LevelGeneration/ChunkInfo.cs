using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    // The 'type' of chunks. Just used for generation
    public enum ChunkType
    {
        HOME, KEY, BOSS, NORMAL
    }

    /// <summary>
    /// Class used to hold information about a given chunk.
    /// Used when generating levels
    /// </summary>
    public class ChunkInfo
    {
        // Which 'directions' of this chunk have doors
        private ISet<Direction> doorDirections;

        public Rotation rotation;


        /// <summary>
        /// Constructs a new ChunkInfo object with doors at the given directions
        /// </summary>
        /// <param name="directions"></param>
        public ChunkInfo(params Direction[] doorDirections)
        {
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
