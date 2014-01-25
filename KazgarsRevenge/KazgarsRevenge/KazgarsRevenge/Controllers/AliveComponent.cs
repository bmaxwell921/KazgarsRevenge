using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public enum DeBuff
    {
        None,
    }
    public enum Buff
    {
        None,
    }
    public class AliveComponent : AIComponent
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


        //todo: put stats here?


        public AliveComponent(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity)
        {
            this.Health = 100;
            this.MaxHealth = 100;
            this.Dead = false;
        }

        /// <summary>
        /// calculates the actual amount of damage and then returns what was subtracted
        /// </summary>
        protected int Damage(int d)
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

        public int Damage(DeBuff n, int d, GameEntity from)
        {
            int actualDamage = Damage(d);
            TakeDamage(actualDamage, from);

            //handle negative effect


            return actualDamage;
        }

        public override void Update(GameTime gameTime)
        {
            //handle health (dots and whatnot)

        }

        public virtual void HandleDamageDealt(int damageDealt)
        {

        }
    }
}
