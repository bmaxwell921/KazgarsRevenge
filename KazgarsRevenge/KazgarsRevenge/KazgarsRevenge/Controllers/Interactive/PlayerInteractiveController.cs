using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public enum InteractiveType
    {
        Shopkeeper,
        EssenceGuy,
        Chest,
        Lootsoul,
    }

    public class PlayerInteractiveController : Component
    {
        protected SharedGraphicsParams modelParams;
        public InteractiveType Type { get; private set; }
        public PlayerInteractiveController(KazgarsRevengeGame game, GameEntity entity, InteractiveType type)
            : base(game, entity)
        {
            this.Type = type;
        }

        public override void Start()
        {
            modelParams = Entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
            modelParams.lineColor = untargetedColor;
            base.Start();
        }

        protected Color targetedColor;
        protected Color untargetedColor;
        public void Target()
        {
            modelParams.lineColor = targetedColor;
        }

        public void UnTarget()
        {
            modelParams.lineColor = untargetedColor;
        }
    }
}
