using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class PillarBeamBillboard : HorizontalStretchingBillboard
    {
        public PillarBeamBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creator, bool ice)
            :base(game, entity, creator, new Vector2(0, 100))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).PillarBeamEffect.Clone();
            maxSize = 250;
            blend = BlendState.AlphaBlend;
            percToFollow = .7f;
                
            if (ice)
            {
                col1 = Color.Blue;
                col2 = Color.LightBlue;
            }
            else
            {
                col1 = Color.Red;
                col2 = Color.Orange;
            }
        }

        Color col1;
        Color col2;
        float lerp = 0;
        bool increasing = true;
        float currentTime = 0;
        float rate = .03f;
        public override void Update(GameTime gameTime)
        {
            currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (increasing)
            {
                lerp += rate;
                if (lerp >= 1)
                {
                    increasing = false;
                }
            }
            else
            {
                lerp -= rate;
                if (lerp <= 0)
                {
                    increasing = true;
                }
            }

            effect.Parameters["colorTint"].SetValue(Color.Lerp(col1, col2, lerp).ToVector3());
            effect.Parameters["CurrentTime"].SetValue(currentTime);
            base.Update(gameTime);
        }

    }
}
