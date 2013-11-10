using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevengeCommon.Packet
{
    public class BasePacket
    {
        MessageType type;

        public BasePacket(MessageType type)
        {
            this.type = type;
        }
    }
}
