using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace KazgarsRevenge.Networking
{
    public class MeleeAttackHandler : BaseHandler
    {
        public MeleeAttackHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }

        ///Message comes in as:
        ///     byte - Messagetype ( already read)
        ///     int - creatorId
        ///     int - attackId
        ///     byte - belongingFaction
        ///     int - position.X
        ///     int - position.Y
        ///     int - position.Z
        ///     int - damage
        public override void Handle(NetIncomingMessage nim)
        {
            AttackManager am = (AttackManager)game.Services.GetService(typeof(AttackManager));
            am.CreateMeleeAttack(nim.ReadInt32(), nim.ReadInt32(), (FactionType)nim.ReadByte(), new Microsoft.Xna.Framework.Vector3(nim.ReadInt32(), nim.ReadInt32(), nim.ReadInt32()), nim.ReadInt32(), false);
        }
    }
}
