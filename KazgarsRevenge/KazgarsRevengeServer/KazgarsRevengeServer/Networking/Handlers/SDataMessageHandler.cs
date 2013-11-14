using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Lidgren.Network;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Since all in game data is sent as Lidgren.MessageType.Data this Handler is a composition of other Handlers
    /// to handle our MessageTypes
    /// </summary>
 
    public class SDataMessageHandler : BaseHandler
    {
        Dictionary<MessageType, BaseHandler> handlers;

        public SDataMessageHandler(KazgarsRevengeGame game)
            : base(game)
        {
            handlers = new Dictionary<MessageType, BaseHandler>();
            AddHandlers();
        }

        private void AddHandlers()
        {
            //handlers[MessageType.GameStateChange] = ;
        }

        /*
         * Messages come in as
         *      - byte MessageType
         *      - byte playerId
         *      - <Additional Info>
         */ 
        public override void Handle(NetIncomingMessage nim)
        {
            BaseHandler handler;
            MessageType mt = EnumParser.GetMessageType(nim.ReadByte());
            handlers.TryGetValue(mt, out handler);

            if (handler == null)
            {
                Console.WriteLine("ERROR: OH GOD PANIC THE DATA MESSAGE HANDLER DIDN'T RECOGNIZE THE MESSAGE TYPE!!!");
                return;
            }

            handler.Handle(nim);
        }
    }
}
