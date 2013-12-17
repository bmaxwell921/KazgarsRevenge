using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Microsoft.Xna.Framework;

namespace KazgarsRevengeServer
{
    public class VelocityMessage : BaseMessage
    {
        public Identification id;
        public Vector3 vel;

        public VelocityMessage(MessageType type, Identification id, Vector3 vel)
            : base(type)
        {
            this.id = id;
            this.vel = vel;
        }
    }
}
