using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public abstract class BaseDataMessageHandler : BaseHandler
    {
        protected Dictionary<MessageType, BaseHandler> handlers;

        public BaseDataMessageHandler(KazgarsRevengeGame game)
            : base(game)
        {
            handlers = new Dictionary<MessageType, BaseHandler>();
            AddHandlers();
        }

        protected abstract void AddHandlers();

        public override void Handle(Lidgren.Network.NetIncomingMessage nim)
        {
            BaseHandler handler;
            // TODO can we just cast...?
            MessageType mt = EnumParser.GetMessageType(nim.ReadByte());
            handlers.TryGetValue(mt, out handler);

            if (handler == null)
            {
                Console.WriteLine("ERROR: OH GOD PANIC THE DATA MESSAGE HANDLER DIDN'T RECOGNIZE THE MESSAGETYPE!!!");
                Console.WriteLine("MessageType: " + mt);
                return;
            }
            handler.Handle(nim);
        }
    }
}
