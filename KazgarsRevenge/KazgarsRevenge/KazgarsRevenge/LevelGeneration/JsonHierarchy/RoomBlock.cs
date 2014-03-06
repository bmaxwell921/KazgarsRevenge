﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
        public static readonly string WALL_NAME = "wall";
        public static readonly string WALL_NAME2 = "wallAdj";
        public static readonly string SOULEVATOR = "soulevator";

        public Vector3 inGameLoc;

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

        public bool IsWall()
        {
            return this.name.Equals(WALL_NAME) || this.name.Equals(WALL_NAME2);
        }

        public bool IsSoulevator()
        {
            return this.name.Equals(SOULEVATOR);
        }

        public override object Clone()
        {
            RoomBlock comp = new RoomBlock();
            comp.location = (Location)this.location.Clone();
            comp.name = (string)this.name.Clone();
            comp.rotation = this.rotation;
            return comp;
        }
    }
}
