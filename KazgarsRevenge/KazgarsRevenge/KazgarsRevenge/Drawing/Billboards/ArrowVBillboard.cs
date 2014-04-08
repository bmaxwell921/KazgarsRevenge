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
    public class ArrowVBillboard : DrawableComponentBillboard
    {
        Entity followData;
        public ArrowVBillboard(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity, Vector3.Up, Vector3.Forward, new Vector2(15, 55))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).ArrowVEffect;
            blend = BlendState.Additive;
        }
        public override void Start()
        {
            followData = Entity.GetSharedData(typeof(Entity)) as Entity;
            base.Start();
        }

        public override void Update(GameTime gameTime)
        {
            up = followData.OrientationMatrix.Forward;
            origin = followData.Position - up * 15;
            base.Update(gameTime);
        }
    }
}
