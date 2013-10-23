using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class HealthComponent : Component
    {
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float HealthPercent
        {
            get
            {
                if (MaxHealth != 0)
                {
                    return (float)Health / (float)MaxHealth;
                }
                else
                {
                    return 0;
                }
            }
        }
        public bool Dead { get; private set; }


        public HealthComponent(MainGame game, int maxHealth)
            : base(game)
        {
            this.Health = maxHealth;
            this.MaxHealth = maxHealth;
            this.Dead = false;
        }

        public override void Start()
        {
            Dead = false;
        }

        #region Damage
        public void Damage(int d)
        {
            Health -= d;
            if (Health <= 0)
            {
                Health = 0;
                Dead = true;
            }
        }

        #endregion

        public override void Update(GameTime gameTime)
        {

        }
    }
}
