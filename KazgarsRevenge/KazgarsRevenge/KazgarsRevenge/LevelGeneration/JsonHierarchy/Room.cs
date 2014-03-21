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

        public int UnRotWidth
        {
            get;
            private set;
        }
        public int UnRotHeight
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

            UnRotWidth = maxX - minX + 1;
            UnRotHeight = maxY - minY + 1;

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
            clone.UnRotWidth = this.UnRotWidth;
            clone.UnRotHeight = this.UnRotHeight;
            clone.blocks = new List<RoomBlock>();
            foreach (RoomBlock rb in this.blocks)
            {
                clone.blocks.Add((RoomBlock)rb.Clone());
            }
            return clone;
        }
    }
}
