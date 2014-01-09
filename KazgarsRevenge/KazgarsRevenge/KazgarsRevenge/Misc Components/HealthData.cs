using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class HealthData
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


        public HealthData(int maxHealth)
        {
            this.Health = maxHealth;
            this.MaxHealth = maxHealth;
            this.Dead = false;
        }

        public int Damage(int d)
        {
            //TODO: armor and resistance calculations
            int actualDamage = d;
            Health -= actualDamage;
            if (Health <= 0)
            {
                Health = 0;
                Dead = true;
            }
            return actualDamage;
        }
    }
}
