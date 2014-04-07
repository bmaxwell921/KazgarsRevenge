using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class LevelUpBillboard : ExpandingCircleBillboard
    {
        public LevelUpBillboard(KazgarsRevengeGame game, GameEntity entity, Vector3 position)
            : base(game, entity, position)
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).LevelUpCircleEffect.Clone();
            maxRadius = 1000;
            increasePerSec = 1500;
        }

        public override void Draw(CameraComponent camera)
        {
            effect.Parameters["alpha"].SetValue(1 - (size.X / maxRadius));
            base.Draw(camera);
        }
    }
}
