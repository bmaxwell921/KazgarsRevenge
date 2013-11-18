using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public abstract class BaseNetworkMessageManager : GameComponent
    {
        // Dictionary of objects used to handle incoming messages
        protected IDictionary<NetIncomingMessageType, BaseHandler> msgHandlers;

        public BaseNetworkMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            msgHandlers = new Dictionary<NetIncomingMessageType, BaseHandler>();
            AddHandlers();
        }

        protected abstract void AddHandlers();

        public override void Update(GameTime gameTime)
        {
            HandleMessages();
            base.Update(gameTime);
        }

        // Just lets the correct Handler handle messages. The Handlers can send messages too
        public void HandleMessages()
        {
            NetIncomingMessage msg;

            while ((msg = ReadMessage()) != null)
            {
                BaseHandler handler;
                msgHandlers.TryGetValue(msg.MessageType, out handler);

                if (handler == null)
                {
                    Console.WriteLine("ERROR: OH GOD MESSAGE TYPE WE DON'T HANDLE! " + msg.MessageType);
                    Console.WriteLine("Message: " + msg.ReadString());
                    continue;
                }

                handler.Handle(msg);
            }
        }

        protected abstract NetIncomingMessage ReadMessage();
    }
}
