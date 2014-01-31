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
        /// Message looks like:
        ///     byte - Messagetype
        ///     int - id
        ///     byte - GameState
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
        /// Sends a message holding velocity information to the server.
        /// Message looks like:
        ///     byte - MessageType
        ///     int - id
        ///     int - vel.x
        ///     int - vel.y
        ///     int - vel.z
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

        /// <summary>
        /// Sends a message to the server that a Melee attack was created.
        /// 
        /// Message looks like:
        ///     byte - Messagetype
        ///     int - creatorId
        ///     int - attackId
        ///     byte - belongingFaction
        ///     int - position.X
        ///     int - position.Y
        ///     int - position.Z
        ///     int - damage
        /// 
        /// </summary>
        /// <param name="creatorId">The id of the entity that created the attack</param>
        /// <param name="attackId">The id of the attack (for updating purposes)</param>
        /// <param name="faction">The faction the attack is associated with</param>
        /// <param name="position">The position to create the attack at</param>
        /// <param name="damage">The damage associated with the attack</param>
        public void SendMeleeAttackMessage(int creatorId, int attackId, FactionType faction, Vector3 position, int damage)
        {
            NetOutgoingMessage nom = client.CreateMessage();
            nom.Write((byte)MessageType.InGame_Melee);
            nom.Write(creatorId);
            nom.Write(attackId);
            nom.Write((byte)faction);
            nom.Write((int)position.X);
            nom.Write((int)position.Y);
            nom.Write((int)position.Z);
            nom.Write(damage);
        }

        /// <summary>
        /// Closes the connection with the server
        /// Message looks like:
        ///     string - id of disconnecting player
        /// </summary>
        /// <param name="id">This client's id</param>
        public void CloseConnection(int id)
        {
            // Send this client id so the server knows who to disconnect
            String form = String.Format("{0}", id);
            lm.Log(Level.DEBUG, String.Format("Sending disconnect message: {0}!", form)); ;
            client.Shutdown(String.Format("{0}", id));
        }
    }
}
