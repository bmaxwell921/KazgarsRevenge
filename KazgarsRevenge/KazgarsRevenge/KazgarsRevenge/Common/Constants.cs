using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public enum GameState
    {
        Playing,
        ConnectionScreen,
        Lobby,
        ReceivingMap,
        Paused,
        StartMenu,
        Settings,
        Loading,

        // Used on server
        ServerStart,
        GenerateMap
    }

    public enum MessageType
    {
        // Client -> Server or Server -> Client
        GameStateChange,
        InGame_Kinetic,
        InGame_Ability,
        InGame_Melee,
        InGame_Range,
        InGame_Magic,
        InGame_Status,

        // Server -> Client Only
        MapData,
        Connected, // Used for clients to parse out their Id and host 
        GameSnapshot,
        HostUpdate,
        DisconnectedPlayer,
    }

    // Used for locations of doors on chunks. North faces the top of the page, East faces to the right
    public enum Direction
    {      
        NORTH, SOUTH, EAST, WEST
    }

    public enum Rotation
    {
        ZERO, NINETY, ONE_EIGHTY, TWO_SEVENTY
    }

    public class Constants
    {
        public static readonly string CONNECTION_KEY = "Revengeance";

        public static readonly int MAX_NUM_CONNECTIONS = 5;

        public static readonly int PORT = 14242;

        // The width, in chunks, of one level of KR
        public static readonly int LEVEL_WIDTH = 3;

        // The height, in chunks, of one level of KR
        public static readonly int LEVEL_HEIGHT = 3;

        // The number of story mode levels
        public static readonly int MAX_LEVELS = 5;
    }
}
