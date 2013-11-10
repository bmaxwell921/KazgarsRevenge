using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KazgarsRevengeCommon
{
    public enum GameState
    {
        StartMenu,
        ServerConnect,
        Lobby,
        Playing,
        PauseMenu,
    };

    public enum MessageType
    {
        GameStateChange,
        InGameMessage_Kinetic,
        InGameMessage_Ability,
        InGameMessage_Status
    }

    public class Constants
    {
        // Key used to connect servers and clients
        public static readonly string CONNECTION_KEY = "Revengeance";

        // Port to connect to
        public static readonly int PORT = 14242;

        // The number of people allowed per server
        public static readonly int MAX_NUM_CONNECTIONS = 5;


    }
}
