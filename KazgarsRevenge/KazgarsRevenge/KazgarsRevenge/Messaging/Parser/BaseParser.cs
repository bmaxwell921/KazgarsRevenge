using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevengeCommon.Packet;
using Lidgren.Network;

namespace KazgarsRevengeCommon.Messaging.Parser
{
    public class BaseParser
    {
        public BaseParser()
        {

        }

        public BasePacket parseMessage(NetIncomingMessage msg)
        {
            return new BasePacket(Util.parseMessageType(msg.ReadByte()));
        }
    }
}
