using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge.Drawing.Billboards
{
    public class HealthBarBillboard : CameraOrientedBillboard
    {
        AliveComponent owner;
        public HealthBarBillboard(KazgarsRevengeGame game, GameEntity entity, float size, AliveComponent owner)
            : base(game, entity, new Vector2(size, size * 10))
        {
            this.owner = owner;
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).GroundTargetEffect;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
