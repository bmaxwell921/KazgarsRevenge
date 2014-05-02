using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class EssenceGuyController : Component, IPlayerInteractiveController
    {
        SharedGraphicsParams modelParams;
        public EssenceGuyController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            modelParams = entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
        }

        public InteractiveType Type { get; private set; }

        public void Target()
        {
            modelParams.lineColor = Color.White;
        }

        public void UnTarget()
        {
            modelParams.lineColor = Color.Black;
        }

        public new InteractiveType GetType()
        {
            return InteractiveType.EssenceGuy;
        }
    }
}
