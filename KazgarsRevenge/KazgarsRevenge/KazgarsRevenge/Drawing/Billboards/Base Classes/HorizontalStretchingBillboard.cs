using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    /// <summary>
    /// a billboard that stretches from its creator to its own position
    /// </summary>
    abstract public class HorizontalStretchingBillboard : DrawableComponentBillboard
    {
        Entity creatorData;
        Entity followData;
        public HorizontalStretchingBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creatorData, Vector2 size)
            : base(game, entity, Vector3.Up, Vector3.Forward, size)
        {
            followData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.creatorData = creatorData;
        }

        protected float maxSize = 600;
        protected float creatorOffsetRight = 0;

        protected float percToFollow = 1;
        public override void Update(GameTime gameTime)
        {
            //get length of billboard
            Vector3 followPos = followData.Position;

            Vector3 toFollow = creatorData.Position - followPos;
            Vector3 creatorPos = followPos + toFollow * percToFollow;
            creatorPos += creatorData.OrientationMatrix.Right * creatorOffsetRight;

            Vector3 diff = creatorPos - followPos;
            origin.X = (followPos.X + creatorPos.X) / 2;
            origin.Y = 20;
            origin.Z = (followPos.Z + creatorPos.Z) / 2;
            diff.Y = 0;
            size.X = diff.Length();
            source.X = size.X / maxSize;
            
            //get Up vector3 for billboard (normal crossed with direction towards followData)
            if (diff != Vector3.Zero)
            {
                diff.Normalize();
            }
            up = Vector3.Cross(normal, diff);

            base.Update(gameTime);
        }
    }
}
