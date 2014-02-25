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
        // Blocks are always 1 unit by 1 unit
        public static readonly int SIZE = 1;

        // Used to know if this block is a door
        public static readonly string DOOR_NAME = "door";

        public static readonly string PLAYER_SPAWN_NAME = "playerSpawn";

        /// <summary>
        /// Returns whether this block is a door or not
        /// </summary>
        /// <returns></returns>
        public bool IsDoor()
        {
            return this.name.Equals(DOOR_NAME);
        }

        /// <summary>
        /// Returns whether this block is a player spawn location or not
        /// </summary>
        /// <returns></returns>
        public bool IsPlayerSpawn()
        {
            return this.name.Equals(PLAYER_SPAWN_NAME);
        }
    }
}
