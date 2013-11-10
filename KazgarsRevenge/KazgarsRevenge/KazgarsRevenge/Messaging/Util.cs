using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevengeCommon.Messaging
{
    class Util
    {
        public static MessageType parseMessageType(byte type)
        {
            if (type == (byte)MessageType.GameStateChange)
            {
                return MessageType.GameStateChange;
            }
            if (type == (byte)MessageType.InGameMessage_Ability)
            {
                return MessageType.InGameMessage_Ability;
            }
            if (type == (byte)MessageType.InGameMessage_Kinetic)
            {
                return MessageType.InGameMessage_Kinetic;
            }
            if (type == (byte)MessageType.InGameMessage_Status)
            {
                return MessageType.InGameMessage_Status;
            }

            throw new Exception("Unknown message type!: " + type);
        }
    }
}
