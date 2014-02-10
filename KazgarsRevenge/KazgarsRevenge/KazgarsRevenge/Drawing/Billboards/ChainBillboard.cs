using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class ChainBillboard : DrawableComponentBillboard
    {
        Entity creatorData;
        Entity followData;
        AliveComponent creator;
        public ChainBillboard(KazgarsRevengeGame game, GameEntity entity, AliveComponent creator)
            : base(game, entity, Vector3.Up, Vector3.Forward, new Vector2(0, 8))
        {
            followData = entity.GetSharedData(typeof(Entity)) as Entity;
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).ChainEffect;
            this.creator = creator;
            creatorData = creator.Entity.GetSharedData(typeof(Entity)) as Entity;
        }

        float maxSize = 2056;
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
