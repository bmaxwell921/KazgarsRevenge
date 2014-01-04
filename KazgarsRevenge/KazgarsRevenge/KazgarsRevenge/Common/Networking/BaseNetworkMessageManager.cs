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
        private IList<Component> components;
        public BaseNetworkMessageManager(KazgarsRevengeGame game)
            : base(game)
        {
            components = new List<Component>();
            msgHandlers = new Dictionary<NetIncomingMessageType, BaseHandler>();
            AddHandlers();
        }

        protected abstract void AddHandlers();

        public override void Update(GameTime gameTime)
        {
            HandleMessages();
            for (int i = components.Count - 1; i >= 0; --i)
            {
                components[i].Update(gameTime);
                if (components[i].Remove)
                {
                    components[i].End();
                    components.RemoveAt(i);
                }
            }
            base.Update(gameTime);
        }

        public void AddComponent(Component comp)
        {
            comp.Start();
            components.Add(comp);
        }

        // Just lets the correct Handler handle messages. The Handlers can send messages too
        public virtual void HandleMessages()
        {
            NetIncomingMessage msg;
            LoggerManager lm = (LoggerManager)Game.Services.GetService(typeof(LoggerManager));

            while ((msg = ReadMessage()) != null)
            {
                BaseHandler handler;
                msgHandlers.TryGetValue(msg.MessageType, out handler);

                if (handler == null)
                {
                    lm.Log(Level.DEBUG, String.Format("Unhandled message type: {0}. Message was: {1}", msg.MessageType, msg.ReadString()));
                    continue;
                }

                handler.Handle(msg);
            }
        }

        protected abstract NetIncomingMessage ReadMessage();
    }
}
