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
        private static readonly string ENEMY_SPAWNER_NAME = "mobSpawn";

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }
        // All the blocks that make up this room
        public List<RoomBlock> blocks
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a list of all the RoomBlocks that represent mobSpawners
        /// </summary>
        /// <returns></returns>
        public IList<RoomBlock> GetEnemySpawners()
        {
            IList<RoomBlock> spawnerLocs = new List<RoomBlock>();
            foreach (RoomBlock rb in blocks)
            {
                if (rb.name.Equals(ENEMY_SPAWNER_NAME))
                {
                    spawnerLocs.Add(rb);
                }
            }
            return spawnerLocs;
        }

        public void CalcWidthHeight()
        {
            if (blocks.Count == 0)
            {
                Width = Height = 0;
                return;
            }

            int minX = blocks[0].location.x;
            int maxX = blocks[0].location.x;

            int minY = blocks[0].location.y;
            int maxY = blocks[0].location.y;

            foreach (RoomBlock block in blocks)
            {
                if (minX > block.location.x)
                {
                    minX = block.location.x;
                }
                if (maxX < block.location.x)
                {
                    maxX = block.location.x;
                }
                if (minY > block.location.y)
                {
                    minY = block.location.y;
                }
                if (maxY < block.location.y)
                {
                    maxY = block.location.y;
                }
            }

            if (rotation == Rotation.ZERO || rotation == Rotation.ONE_EIGHTY)
            {
                Width = maxX - minX + 1;
                Height = maxY - minY + 1;
            }
            else
            {
                Height = maxX - minX + 1;
                Width = maxY - minY + 1;
            }
        }

        public override object Clone()
        {
            Room clone = new Room();
            clone.location = (Location)this.location.Clone();
            clone.name = (string)this.name.Clone();
            clone.rotation = this.rotation;
            clone.Width = this.Width;
            clone.Height = this.Height;
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
