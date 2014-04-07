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
            : base(game, entity, Vector3.Forward, Vector3.Right, size)
        {
            followData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.creatorData = creatorData;
        }

        protected float maxSize = 600;
        protected float creatorOffsetRight = 0;
        protected float creatorOffsetUp = 0;
        protected float followOffsetRight = 0;
        protected float followOffsetUp = 0;
        public override void Update(GameTime gameTime)
        {
            //get length of billboard
            Vector3 creatorPos = creatorData.Position;
            creatorPos += creatorData.OrientationMatrix.Right * creatorOffsetRight;
            creatorPos += creatorData.OrientationMatrix.Up * creatorOffsetUp;

            Vector3 followPos = followData.Position;
            followPos += followData.OrientationMatrix.Right * followOffsetRight;
            followPos += followData.OrientationMatrix.Up * followOffsetUp;

            Vector3 diff = followPos - creatorPos;
            origin.X = (followData.Position.X + creatorPos.X) / 2;
            origin.Y = 20;
            origin.Z = (followData.Position.Z + creatorPos.Z) / 2;
            diff.Y = 0;
            size.X = diff.Length();
            source.X = size.X / maxSize;

            //get Up vector3 for billboard (normal crossed with direction towards followData)
            diff.Normalize();
            up = Vector3.Cross(normal, diff);

            base.Update(gameTime);
        }
    }
}
