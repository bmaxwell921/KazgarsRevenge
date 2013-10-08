using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{


    #region buff declarations
    enum NegativeEffect
    {
        None,
    }
    enum PositiveEffect
    {
        None,
    }
    class NegData
    {
        public double millisLeft;
        public NegativeEffect effect;
        public NegData(NegativeEffect effect, int length)
        {
            millisLeft = length;
            this.effect = effect;
        }
    }
    class PosData
    {
        public double millisLeft;
        public PositiveEffect effect;
        public PosData(PositiveEffect effect, int length)
        {
            millisLeft = length;
            this.effect = effect;
        }
    }
    #endregion


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

        List<NegData> currentDebuffs = new List<NegData>();
        List<PosData> currentBuffs = new List<PosData>();

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

        public void Damage(int d, NegativeEffect effect, int length)
        {
            Damage(d);
            if (effect != NegativeEffect.None)
            {
                currentDebuffs.Add(new NegData(effect, length));
            }
        }

        public void Damage(int d, PositiveEffect effect, int length)
        {
            Damage(d);
            if (effect != PositiveEffect.None)
            {
                currentBuffs.Add(new PosData(effect, length));
            }
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;

            //negative effects
            for (int i = currentDebuffs.Count - 1; i >= 0; --i)
            {
                currentDebuffs[i].millisLeft -= elapsed;
                if (currentDebuffs[i].millisLeft <= 0)
                {
                    currentDebuffs.RemoveAt(i);
                }
            }

            //positive effects
            for (int i = currentBuffs.Count - 1; i >= 0; --i)
            {
                currentBuffs[i].millisLeft -= elapsed;
                if (currentBuffs[i].millisLeft <= 0)
                {
                    currentBuffs.RemoveAt(i);
                }
            }
        }
    }
}
