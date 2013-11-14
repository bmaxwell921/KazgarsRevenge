using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    public class BasePacket
    {
        protected MessageType type;

        public BasePacket(MessageType type)
        {
            this.type = type;
        }
    }
}
