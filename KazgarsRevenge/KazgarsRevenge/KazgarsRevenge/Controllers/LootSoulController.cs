using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge.Controllers
{
    class LootSoulController : Component
    {
        public List<Item> Loot { get; private set; }
        public LootSoulController(KazgarsRevengeGame game, GameEntity entity, List<Item> loot)
            : base(game, entity)
        {
            this.Loot = loot;
        }

        public override void Update(GameTime gameTime)
        {

        }

    }
}
