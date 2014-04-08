using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class LevelUpBeamBillboard : VerticalStretchingBillboard
    {
        public LevelUpBeamBillboard(KazgarsRevengeGame game, GameEntity entity, Entity creatorData)
            : base(game, entity, creatorData, new Vector2(10, 35))
        {
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).LevelUpBeamEffect.Clone();
            creatorOffsetUp = 1000;
            followOffsetUp = 999;
            maxSize = 1000;
        }

        bool retracting = false;
        public override void Update(GameTime gameTime)
        {
            if (!retracting)
            {
                followOffsetUp -= (float)(gameTime.ElapsedGameTime.TotalMilliseconds * 2);
                if (followOffsetUp <= 0)
                {
                    followOffsetUp = 0;
                    retracting = true;
                }
            }
            else
            {
                creatorOffsetUp -= (float)(gameTime.ElapsedGameTime.TotalMilliseconds * 2.5f);

                if (creatorOffsetUp <= 0)
                {
                    KillComponent();
                }
            }

            base.Update(gameTime);
        }
    }
}
