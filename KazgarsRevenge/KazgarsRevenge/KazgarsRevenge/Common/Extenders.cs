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

        /// <summary>
        /// Converts the given string to its enum equivalent, or throws an exception 
        /// if the string isn't recognized
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Direction DirectionFromChar(char direction)
        {
            direction = Char.ToUpper(direction);
            char NORTH = 'N'; char SOUTH = 'S'; char EAST = 'E'; char WEST = 'W';
            
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
        #endregion

        #region ChunkType Extenders

        /// <summary>
        /// Converts the given chunkType character to its enum value
        /// </summary>
        /// <param name="chunkType"></param>
        /// <returns></returns>
        public static ChunkType ChunkTypeFromChar(char chunkType)
        {
            char HOME = 'H'; char SOUL = 'S'; char BOSS = 'B'; char KEY = 'K'; char NORMAL = 'N';
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
        #endregion
    }
}
