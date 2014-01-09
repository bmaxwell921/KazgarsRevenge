using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public enum NegativeEffect
    {
        None,
    }
    public class HealthHandlerComponent : Component
    {
        HealthData health;
        public HealthHandlerComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.health = entity.GetSharedData(typeof(HealthData)) as HealthData;
        }

        public int Damage(int d)
        {
            entity.Hit();
            return health.Damage(d);
        }

        public int Damage(NegativeEffect n, int d)
        {
            return health.Damage(d);

            //handle negative effect
        }

        public HealthData GetHealthData()
        {
            return health;
        }

        public override void Update(GameTime gameTime)
        {
            //handle health (dots and whatnot)

        }
    }
}
