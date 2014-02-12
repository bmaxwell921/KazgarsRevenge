using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class defining extenders for Enums
    /// </summary>
    public static class Extenders
    {
        #region Rotation Extenders
        /// <summary>
        /// Converts the rotation to it's equivalent value in degrees
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float ToDegrees(this Rotation rotation)
        {
            if (rotation == Rotation.ZERO)
            {
                return 0;
            } 
            else if (rotation == Rotation.NINETY)
            {
                return 90;
            }
            else if (rotation == Rotation.ONE_EIGHTY)
            {
                return 180;
            }
            else
            {
                return 270;
            }
        }

        /// <summary>
        /// Converts the rotation to it's equivalent value in radians
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float ToRadians(this Rotation rotation)
        {
            return MathHelper.ToRadians(rotation.ToDegrees());
        }

        /// <summary>
        /// Returns a new Rotation representing the given rotation rotated by 90 degrees counter clockwise.
        /// This isn't really an extender in the c# sense, so call it on the Extenders class
        /// </summary>
        /// <param name="orig"></param>
        /// <returns></returns>
        public static Rotation RotateCounter(Rotation orig)
        {
            int FULL_CIRCLE = 360;
            int ROTATE_AMT = 90;
            float newDegrees = (orig.ToDegrees() + ROTATE_AMT) % FULL_CIRCLE;

            foreach (Rotation rotation in Enum.GetValues(typeof(Rotation)))
            {
                if (rotation.ToDegrees() == newDegrees)
                {
                    return rotation;
                }
            }

            throw new ArgumentException(String.Format("Rotated to an unrecognized Rotation: {0}", newDegrees));
        }
        #endregion

        #region Direction Extenders

        /// <summary>
        /// Returns a new Rotation representing the given Direction rotated by 90 degrees counter clockwise
        /// This isn't really an extender in the c# sense, so call it on the Extenders class
        /// </summary>
        /// <param name="orig"></param>
        /// <returns></returns>
        public static Direction RotateCounter(Direction orig)
        {
            if (orig == Direction.NORTH)
            {
                return Direction.WEST;
            }
            else if (orig == Direction.WEST)
            {
                return Direction.SOUTH;
            }
            else if (orig == Direction.SOUTH)
            {
                return Direction.EAST;
            }
            else
            {
                return Direction.NORTH;
            }
        }

        /// <summary>
        /// Returns a new Direction that represents the given direction rotated 
        /// counterclockwise by the given rotation
        /// Ie RotateTo(Direction.NORTH, Rotation.ONE_EIGHTY) results in Direction.SOUTH
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="newRot"></param>
        /// <returns></returns>
        public static Direction RotateTo(Direction orig, Rotation newRot)
        {
            int ROTATION_DEGREES = 90;
            int rotationCount = (int) newRot.ToDegrees() / ROTATION_DEGREES;
            Direction result = orig;
            // Just do the proper number of 90 degree rotations
            for (int i = 0; i < rotationCount; ++i)
            {
                result = RotateCounter(result);
            }

            return result;
        }


        private static char NORTH = 'N'; private static char SOUTH = 'S'; private static char EAST = 'E'; private static char WEST = 'W';
        /// <summary>
        /// Converts the given string to its enum equivalent, or throws an exception 
        /// if the string isn't recognized
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Direction FromChar(char direction)
        {
            direction = Char.ToUpper(direction);
            
            if (direction == NORTH)
            {
                return Direction.NORTH;
            }
            else if (direction == SOUTH)
            {
                return Direction.SOUTH;
            }
            else if (direction == EAST)
            {
                return Direction.EAST;
            }
            else if (direction == WEST)
            {
                return Direction.WEST;
            }

            throw new ArgumentException(String.Format("Unrecognized direction string: {0}", direction));
        }

        public static char ToChar(this Direction direction)
        {
            if (direction == Direction.NORTH)
            {
                return NORTH;
            }
            else if (direction == Direction.SOUTH)
            {
                return SOUTH;
            }
            else if (direction == Direction.EAST)
            {
                return EAST;
            }
            else if (direction == Direction.WEST)
            {
                return WEST;
            }

            throw new ArgumentException(String.Format("Unrecognized direction string: {0}", direction));

        }
        #endregion

        #region ChunkType Extenders

        private static char HOME = 'H'; private static char SOUL = 'S'; private static char BOSS = 'B'; private static char KEY = 'K'; private static char NORMAL = 'N';
        /// <summary>
        /// Converts the given chunkType character to its enum value
        /// </summary>
        /// <param name="chunkType"></param>
        /// <returns></returns>
        public static ChunkType ChunkTypeFromChar(char chunkType)
        {
            chunkType = Char.ToUpper(chunkType);
            if (chunkType == HOME)
            {
                return ChunkType.HOME;
            }
            else if (chunkType == SOUL)
            {
                return ChunkType.SOULEVATOR;
            }
            else if (chunkType == BOSS)
            {
                return ChunkType.BOSS;
            }
            else if (chunkType == KEY)
            {
                return ChunkType.KEY;
            }
            else if (chunkType == NORMAL)
            {
                return ChunkType.NORMAL;
            }

            throw new ArgumentException(String.Format("Unrecognized chunkType: {0}", chunkType));
        }

        public static char ToChar(this ChunkType chunkType)
        {
            if (chunkType == ChunkType.HOME)
            {
                return HOME;
            }
            else if (chunkType == ChunkType.SOULEVATOR)
            {
                return SOUL;
            }
            else if (chunkType == ChunkType.BOSS)
            {
                return BOSS;
            }
            else if (chunkType == ChunkType.KEY)
            {
                return KEY;
            }
            else if (chunkType == ChunkType.NORMAL)
            {
                return NORMAL;
            }

            throw new ArgumentException("Unaccounted enum value!");
        }
        #endregion

        #region FloorName Extenders
        /// <summary>
        /// Converts an integer to the corresponding number.
        /// 
        /// NOTE: This is not done by Ordinal!!! - The numbers are as stated in the 
        /// Design doc:
        ///     1 = Dungeon
        ///     2 = Torture Chamber
        ///     3 = Lab
        ///     4 = Library
        ///     5 = Grand Hall
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static FloorName GetFloorName(int level)
        {
            if (level == 1)
            {
                return FloorName.Dungeon;
            }
            else if (level == 2)
            {
                return FloorName.TortureChamber;
            }
            else if (level == 3)
            {
                return FloorName.Lab;
            }
            else if (level == 4)
            {
                return FloorName.Library;
            }
            else if (level == 5)
            {
                return FloorName.GrandHall;
            }
            throw new ArgumentException(String.Format("Unknown floor type: {0}", level));
        }
        #endregion
    }
}
