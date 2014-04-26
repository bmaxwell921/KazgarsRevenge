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
            MessageType mt = (MessageType) Enum.ToObject(typeof(MessageType), nim.ReadByte());
            handlers.TryGetValue(mt, out handler);

            LoggerManager lm = (LoggerManager)game.Services.GetService(typeof(LoggerManager));

            if (handler == null)
            {
                lm.Log(LogLevel.DEBUG, String.Format("Unhandled MessageType: {0}", mt));
                return;
            }
            handler.Handle(nim);
        }
    }
}
