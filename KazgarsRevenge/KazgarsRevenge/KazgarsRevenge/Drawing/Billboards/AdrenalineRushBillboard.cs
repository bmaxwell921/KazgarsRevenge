using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class AdrenalineRushBillboard : DrawableComponentBillboard
    {
        Entity followData;
        public AdrenalineRushBillboard(KazgarsRevengeGame game, GameEntity entity, Entity followData)
            : base(game, entity, Vector3.Forward, Vector3.Up, new Vector2(100, 100))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).AdrenalineRushEffect;
            this.followData = followData;
            originalSize = size.X;
        }

        bool expanding = true;
        float originalSize;
        public override void Update(GameTime gameTime)
        {
            origin = followData.Position + Vector3.Up * 20;
            /*
            if (expanding)
            {
                size.X += (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 100.0f);
                size.Y = size.X;
                if (size.X > originalSize + 10)
                {
                    expanding = false;
                }
            }
            else
            {
                size.X -= (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 100.0f);
                size.Y = size.X;
                if (size.X < originalSize - 10)
                {
                    expanding = true;
                }
            }*/
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
