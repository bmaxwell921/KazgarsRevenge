using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    /// <summary>
    /// a vertical animated billboard that centers on the player's position
    /// </summary>
    public class AdrenalineRushBillboard : DrawableComponentBillboard
    {
        Entity followData;
        public AdrenalineRushBillboard(KazgarsRevengeGame game, GameEntity entity, Entity followData)
            : base(game, entity, Vector3.Forward, Vector3.Up, new Vector2(110, 110))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).AdrenalineRushEffect;
            this.followData = followData;
        }

        public override void Update(GameTime gameTime)
        {
            origin = followData.Position + Vector3.Up * 30;
            base.Update(gameTime);
        }

        public override void Draw(CameraComponent camera)
        {
            normal = origin - camera.Position;
            normal.Y = 0;
            if (normal != Vector3.Zero)
            {
                normal.Normalize();
            }
            base.Draw(camera);
        }
    }
}
