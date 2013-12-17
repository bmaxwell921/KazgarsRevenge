using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    public class BaseMessage
    {
        public MessageType type;

        public BaseMessage(MessageType type)
        {
            this.type = type;
        }
    }
}
