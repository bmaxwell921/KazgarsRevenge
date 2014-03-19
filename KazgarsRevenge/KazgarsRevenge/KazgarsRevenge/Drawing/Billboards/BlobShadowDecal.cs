using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{

    public class BlobShadowDecal : DrawableComponentBillboard
    {
        public BlobShadowDecal(KazgarsRevengeGame game, GameEntity entity, float size)
            : base(game, entity, Vector3.Up, Vector3.Forward, new Vector2(size, size))
        {
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).ShadowEffect;
            this.blend = BlendState.Additive;
        }

        Entity physicalData;
        public override void Update(GameTime gameTime)
        {
            origin = new Vector3(physicalData.Position.X, .1f, physicalData.Position.Z);
            base.Update(gameTime);
        }
    }
}
