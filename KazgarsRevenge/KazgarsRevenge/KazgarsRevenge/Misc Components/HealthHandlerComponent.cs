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
    class HealthHandlerComponent : Component
    {
        HealthData health;
        public HealthHandlerComponent(MainGame game, HealthData health)
            : base(game)
        {
            this.health = health;
        }

        public void Damage(int d)
        {
            health.Damage(d);
        }

        public void Damage(NegativeEffect n, int d)
        {
            health.Damage(d);

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
