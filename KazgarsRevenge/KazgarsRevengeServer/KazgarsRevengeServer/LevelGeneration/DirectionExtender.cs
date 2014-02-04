using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    public class DirectionExtender
    {
        public static Direction toDirection(char dir)
        {
            if (dir == 'N' || dir == 'n')
            {
                return Direction.NORTH;
            }
            if (dir == 'S' || dir == 's')
            {
                return Direction.SOUTH;
            }
            if (dir == 'E' || dir == 'e')
            {
                return Direction.EAST;
            }
            if (dir == 'W' || dir == 'w')
            {
                return Direction.WEST;
            }
            throw new ArgumentException("Unknown Direction char: " + dir);
        }
    }
}
