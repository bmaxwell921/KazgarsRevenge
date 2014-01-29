using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to consolidate all sent messages
    /// </summary>
    public class MessageSender
    {
        // Client used to send messages
        private NetClient client;

        // Logger
        private LoggerManager lm;

        public MessageSender(NetClient client, LoggerManager lm)
        {
            this.client = client;
            this.lm = lm;
        }

        /// <summary>
        /// Sends a message to the server to start the game.
        /// </summary>
        /// <param name="id">The id of the player sending the message</param>
        public void SendStartGame(int id)
        {
            // TODO make sure we're connected to something???
            NetOutgoingMessage nom = client.CreateMessage();
            nom.Write((byte)MessageType.GameStateChange);
            nom.Write(id);
            nom.Write((byte)GameState.GenerateMap);
            client.SendMessage(nom, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends a message holding velocity information to the server
        /// </summary>
        /// <param name="id">The id of the player sending the message </param>
        /// <param name="vel">The player's velocity</param>
        public void SendVelocityMessage(int id, Vector3 vel)
        {
            NetOutgoingMessage nom = client.CreateMessage();
            nom.Write((byte)MessageType.InGame_Kinetic);
            nom.Write(id);

            nom.Write((int)vel.X);
            nom.Write((int)vel.Y);
            nom.Write((int)vel.Z);

            // If it gets there that's good, if not meh
            client.SendMessage(nom, NetDeliveryMethod.Unreliable);
        }
    }
}
