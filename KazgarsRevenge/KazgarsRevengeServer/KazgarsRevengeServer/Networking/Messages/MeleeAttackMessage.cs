using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    // Class to hold a melee attack message
    public class MeleeAttackMessage : BaseMessage
    {
        // The id of the person who created this attack
        public int creatorId;

        // The id of the actual attack
        public int attackId;

        // The faction this attack is associated with
        public FactionType assocFact;

        // The position to create this attack at
        public Vector3 position;

        // The damage associated with this attack
        public int damage;

        public MeleeAttackMessage(MessageType type, int creatorId, int attackId, FactionType assocFact, Vector3 position, int damage)
            : base(type)
        {
            this.creatorId = creatorId;
            this.attackId = attackId;
            this.assocFact = assocFact;
            this.position = position;
            this.damage = damage;
        }
    }
}
