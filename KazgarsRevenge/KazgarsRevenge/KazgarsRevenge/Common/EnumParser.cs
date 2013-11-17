﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
namespace KazgarsRevenge
{
    public class EnumParser
    {
        public static GameState GetGameState(byte b)
        {
            if (b == (byte)GameState.Lobby)
            {
                return GameState.Lobby;
            }
            if (b == (byte)GameState.Paused)
            {
                return GameState.Paused;
            }
            if (b == (byte)GameState.Playing)
            {
                return GameState.Playing;
            }
            if (b == (byte)GameState.StartMenu)
            {
                return GameState.StartMenu;
            }
            throw new ArgumentException("Unknown GameState");
        }

        public static MessageType GetMessageType(byte b)
        {
            if (b == (byte)MessageType.GameStateChange)
            {
                return MessageType.GameStateChange;
            }
            if (b == (byte)MessageType.InGame_Ability)
            {
                return MessageType.InGame_Ability;
            }
            if (b == (byte)MessageType.InGame_Kinetic)
            {
                return MessageType.InGame_Kinetic;
            }
            if (b == (byte)MessageType.InGame_Status)
            {
                return MessageType.InGame_Status;
            }
            if (b == (byte)MessageType.MapData)
            {
                return MessageType.MapData;
            }
            throw new ArgumentException("Unknown MessageType");
        }
    }
}