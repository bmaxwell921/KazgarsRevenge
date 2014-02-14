using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class CameraOrientedBillboard : DrawableComponentBillboard
    {
        public CameraOrientedBillboard(KazgarsRevengeGame game, GameEntity entity, Vector2 size)
            : base(game, entity, Vector3.Up, Vector3.Forward, size)
        {

        }

        public override void Draw(Matrix view, Matrix projection, Vector3 cameraPos)
        {
            normal = cameraPos - origin;
            normal.Normalize();
            this.left = Vector3.Cross(normal, up);

            base.Draw(view, projection, cameraPos);
        }
    }
}
