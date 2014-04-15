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
    /// <summary>
    /// a billboard close to the ground that indicates player's mouse position
    /// </summary>
    public class AbilityTargetDecal : DrawableComponentBillboard
    {
        public AbilityTargetDecal(KazgarsRevengeGame game, GameEntity entity, float size)
            : base(game, entity, Vector3.Up, Vector3.Forward, new Vector2(size, size))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).GroundTargetEffect;
        }

        public void UpdateMouseLocation(Vector3 position, bool draw, float radius)
        {
            origin = new Vector3(position.X, .1f, position.Z);
            Visible = draw;
            this.size.X = radius * 2;
            this.size.Y = radius * 2;
        }
    }
}
