using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

using KazgarsRevenge;

namespace KazgarsRevenge
{
    public abstract class BaseHandler
    {
        protected KazgarsRevengeGame game;

        public BaseHandler(KazgarsRevengeGame game)
        {
            this.game = game;
        }

        public abstract void Handle(NetIncomingMessage nim);
    }
}
