using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    abstract public class StretchingBillboard : DrawableComponentBillboard
    {
        Entity creatorData;
        Entity followData;
        AliveComponent creator;
        public StretchingBillboard(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator, Vector2 size)
            : base(game, entity, Vector3.Up, Vector3.Forward, size)
        {
            followData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.creator = creator;
            creatorData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
        }

        protected float maxSize = 600;
        public override void Update(GameTime gameTime)
        {
            //get length of billboard
            Vector3 diff = followData.Position - creatorData.Position;
            origin.X = (followData.Position.X + creatorData.Position.X) / 2;
            origin.Y = 20;
            origin.Z = (followData.Position.Z + creatorData.Position.Z) / 2;
            diff.Y = 0;
            size.X = diff.Length();
            source.X = Math.Min(1, size.X / maxSize);
            
            //get Up vector3 for billboard (normal crossed with direction towards followData)
            diff.Normalize();
            up = Vector3.Cross(normal, diff);

            base.Update(gameTime);
        }
    }
}
