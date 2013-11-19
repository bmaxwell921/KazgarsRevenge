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

        // Used on server
        GenerateMap
    }

    public enum MessageType
    {
        // Client -> Server or Server -> Client
        GameStateChange,
        InGame_Kinetic,
        InGame_Ability,
        InGame_Status,

        // Server -> Client Only
        MapData,
        Connected, // Used for clients to parse out their Id and host 
    }

    public class Constants
    {
        public static readonly string CONNECTION_KEY = "Revengeance";

        public static readonly int MAX_NUM_CONNECTIONS = 5;

        public static readonly int PORT = 14242;
    }
}
