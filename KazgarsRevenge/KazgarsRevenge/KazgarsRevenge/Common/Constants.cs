﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public enum GameState
    {
        Playing,
        Lobby,
        Paused,
        StartMenu
    }
    
    public enum MessageType
    {
        ConnectionMessage,
        GameStateChange,
        InGame_Kinetic,
        InGame_Ability,
        InGame_Status,
    }

    public class Constants
    {
        public static readonly string CONNECTION_KEY = "Revengeance";

        public static readonly int MAX_NUM_CONNECTIONS = 5;

        public static readonly int PORT = 14242;
    }
}
