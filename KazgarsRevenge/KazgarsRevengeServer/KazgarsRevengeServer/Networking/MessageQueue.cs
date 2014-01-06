using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Convience class used to queue up in game messages.
    /// When in gameState.Playing, the server will receive, then
    /// queue up messages. Then every 100 ms, it will apply all the 
    /// updates, then send out a snapshot of the game state
    /// </summary>
    public class MessageQueue
    {
        // the queue of messages, sorted by type
        IDictionary<Type, IList<BaseMessage>> msgQueues;

        public MessageQueue()
        {
            msgQueues = new Dictionary<Type, IList<BaseMessage>>();
        }

        // Adds the given message to the queue
        public void AddMessage(BaseMessage msg)
        {
            IList<BaseMessage> msgs;

            if (!msgQueues.TryGetValue(msg.GetType(), out msgs))
            {
                msgs = new List<BaseMessage>();
            }
            msgs.Add(msg);
            msgQueues[msg.GetType()] = msgs;
        }

        // gets all of the messages of the given type
        public IList<BaseMessage> GetAllOf(Type msgType)
        {
            if (!msgQueues.ContainsKey(msgType))
            {
                return new List<BaseMessage>();
            }
            return msgQueues[msgType];
        }

        // Clears all of the messages of the given type
        public void ClearMsgType(Type msgType)
        {
            if (!msgQueues.ContainsKey(msgType))
            {
                return;
            }
            msgQueues[msgType].Clear();
        }

        public void Reset()
        {
            msgQueues.Clear();
        }
    }
}
