using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KazgarsRevenge;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    public class SPlayerManager : SEntityManager
    {
        // Map of Identifications to players
        // HOST IS ALWAYS IDENTIFICATION 0 
        public Dictionary<Identification, GameEntity> players
        {
            get;
            protected set;
        }

        public SPlayerManager(KazgarsRevengeGame game)
            : base(game)
        {
            players = new Dictionary<Identification, GameEntity>();
        }

        /// <summary>
        /// Creates and adds a new player to the game, returning the new player's identification
        /// </summary>
        /// <returns></returns>
        public Identification createNewPlayer()
        {
            Identification newId = IdentificationFactory.GenerateNextId();
            //TODO do stuff - actually setting it up

            return newId;
        }

        public void UpdatePlayerPosition(Identification id, Vector3 velocity)
        {
            // TODO Update the position based on velocity?? - I feel like Bepu won't like me doing that -.-
        }

        public Vector3 GetPlayerPosition(Identification id)
        {
            return Vector3.Zero;
        }
    }
}
