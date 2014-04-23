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
    public class ShopkeeperController : PlayerInteractiveController
    {
        public ShopkeeperController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity, InteractiveType.Shopkeeper)
        {
            targetedColor = Color.Gold;
            untargetedColor = Color.Gray;

        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
