using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    abstract public class VerticalStretchingBillboard : DrawableComponentBillboard
    {
        Entity creatorData;
        Entity followData;
        public VerticalStretchingBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creatorData, Vector2 size)
            : base(game, entity, Vector3.Forward, Vector3.Up, size)
        {
            followData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.creatorData = creatorData;
        }

        protected float maxSize = 600;
        protected float creatorOffsetUp = 0;
        protected float followOffsetUp = 0;
        public override void Update(GameTime gameTime)
        {
            //get length of billboard
            Vector3 creatorPos = creatorData.Position + creatorData.OrientationMatrix.Up * creatorOffsetUp;
            Vector3 followPos = followData.Position + followData.OrientationMatrix.Up * followOffsetUp;
            origin = followData.Position;
            origin.Y = (followPos.Y + creatorPos.Y) / 2;

            size.Y = Math.Abs(followPos.Y - creatorPos.Y);
            source.Y = size.Y / maxSize;

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
