using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace KazgarsRevengeServer
{
    interface MessageParser
    {
        BasePacket ParseMessage(NetIncomingMessage nim);
    }
}
