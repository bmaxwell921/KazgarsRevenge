using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class DataMessageHandler : BaseDataMessageHandler
    {
        public DataMessageHandler(KazgarsRevengeGame game)
            : base(game)
        {
        }
        protected override void AddHandlers()
        {
            handlers[MessageType.Connected] = new ConnectedHandler(game);
            handlers[MessageType.MapData] = new ReceivingMapHandler(game);
            handlers[MessageType.GameStateChange] = new GameStateChangeHandler(game);
            handlers[MessageType.InGame_Kinetic] = new KineticHandler(game);
            handlers[MessageType.GameSnapshot] = new SnapshotHandler(game);
            handlers[MessageType.HostUpdate] = new HostUpdateHandler(game);
            handlers[MessageType.DisconnectedPlayer] = new DisconnectedPlayerHandler(game);
        }
    }
}
