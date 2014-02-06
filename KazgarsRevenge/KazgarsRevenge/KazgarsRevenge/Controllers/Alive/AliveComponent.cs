using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public enum StatType
    {
        RunSpeed,
        AttackSpeed,
        Strength,
        Agility,
        Intellect,
        CooldownReduction,
        CritChance,
        Health,
    }
    public enum DeBuff
    {
        None = 0,
        SerratedBleeding,
        MagneticImplant,
    }
    public enum Buff
    {
        None,
        AdrenalineRush,
        Homing,
        Penetrating,
        Leeching,
        SerratedBleeding,
    }
    public enum BuffState
    {
        Starting,
        Ticking,
        Ending,
    }
    public class AliveComponent : AIComponent
    {

        class NegativeEffect
        {
            public DeBuff type { get; private set; }
            public double timeLeft;
            public double nextTick;
            GameEntity from;
            public NegativeEffect(DeBuff type, double time, GameEntity from)
            {
                this.type = type;
                this.timeLeft = time;
                this.nextTick = time - 1000;
                this.from = from;
            }
        }

        class PositiveEffect
        {
            public Buff type { get; private set; }
            public double timeLeft;
            public double nextTick;
            GameEntity from;
            public PositiveEffect(Buff type, double time, GameEntity from)
            {
                this.type = type;
                this.timeLeft = time;
                this.from = from;
                this.nextTick = -10000;
            }
        }


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

        #region stats
        protected int level = 0;
        
        private Dictionary<StatType, float> statsPerLevel = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 0},
            {StatType.AttackSpeed, 0},
            {StatType.Strength, 1},
            {StatType.Agility, 1},
            {StatType.Intellect, 1},
            {StatType.CooldownReduction, 0},
            {StatType.CritChance, 0},
            {StatType.Health, 100},
        };

        protected Dictionary<StatType, float> originalBaseStats = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 120},
            {StatType.AttackSpeed, .05f},
            {StatType.Strength, 1},
            {StatType.Agility, 1},
            {StatType.Intellect, 1},
            {StatType.CooldownReduction, 0},
            {StatType.CritChance, 0},
            {StatType.Health, 100},
        };
        protected Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 120},
            {StatType.AttackSpeed, .05f},
            {StatType.Strength, 1},
            {StatType.Agility, 1},
            {StatType.Intellect, 1},
            {StatType.CooldownReduction, 0},
            {StatType.CritChance, 0},
            {StatType.Health, 100},
        };

        protected Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

        protected virtual void RecalculateStats()
        {
            //add base stats, accounting for level
            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats[(StatType)i] = baseStats[(StatType)i];
            }

            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats[(StatType)i] += statsPerLevel[(StatType)i] * level;
            }
        }
        #endregion

        public AliveComponent(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity)
        {
            this.MaxHealth = 100 * level;
            this.Health = MaxHealth;
            this.level = level;
            this.Dead = false;
        }

        /// <summary>
        /// damage on the controller's part. Deals with aggro, animations, and particles
        /// </summary>
        protected virtual void TakeDamage(int damage, GameEntity from)
        {

        }

        /// <summary>
        /// calculates the actual amount of damage and then returns what was subtracted
        /// </summary>
        protected int Damage(int d)
        {
            //TODO: armor and resistance calculations
            int actualDamage = d;
            if (HasDeBuff(DeBuff.MagneticImplant))
            {
                actualDamage *= 2;
            }
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
            if (n != DeBuff.None)
            {
                AddDebuff(n, from);
            }

            return actualDamage;
        }

        private List<NegativeEffect> activeDebuffs = new List<NegativeEffect>();
        private List<PositiveEffect> activeBuffs = new List<PositiveEffect>();
        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            //handle health (dots and whatnot)
            for (int i = activeDebuffs.Count - 1; i >= 0; --i)
            {
                activeDebuffs[i].timeLeft -= elapsed;
                if (activeDebuffs[i].timeLeft < activeDebuffs[i].nextTick)
                {
                    HandleDeBuff(activeDebuffs[i].type, BuffState.Ticking);
                    activeDebuffs[i].nextTick -= 1000;
                }
                if (activeDebuffs[i].timeLeft <= 0)
                {
                    HandleDeBuff(activeDebuffs[i].type, BuffState.Ending);
                    activeDebuffs.RemoveAt(i);
                }
            }



            for (int i = activeBuffs.Count - 1; i >= 0; --i)
            {
                activeBuffs[i].timeLeft -= elapsed;
                if (activeBuffs[i].timeLeft < activeBuffs[i].nextTick)
                {
                    HandleBuff(activeBuffs[i].type, BuffState.Ticking);
                    activeBuffs[i].nextTick -= 1000;
                }

                if (activeBuffs[i].timeLeft <= 0)
                {
                    HandleBuff(activeBuffs[i].type, BuffState.Ending);
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        public virtual void HandleDamageDealt(int damageDealt)
        {

        }


        private void HandleBuff(Buff b, BuffState state)
        {
            switch (b)
            {
                case Buff.AdrenalineRush:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.RunSpeed] += 80;
                        baseStats[StatType.AttackSpeed] += .6f;
                        RecalculateStats();
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] = originalBaseStats[StatType.RunSpeed];
                        baseStats[StatType.AttackSpeed] = originalBaseStats[StatType.AttackSpeed];
                        RecalculateStats();
                    }
                    break;
            }
        }

        private void HandleDeBuff(DeBuff d, BuffState state)
        {
            switch (d)
            {
                case DeBuff.SerratedBleeding:
                    if (state == BuffState.Starting)
                    {
                        Damage((int)(MaxHealth * .01f));
                    }
                    break;
            }
        }

        private double GetDebuffLength(DeBuff n)
        {
            switch (n)
            {
                case DeBuff.SerratedBleeding:
                    return 6000;
                case DeBuff.MagneticImplant:
                    return 2000;
            }
            return 0;
        }

        private double GetBuffLength(Buff n)
        {
            switch (n)
            {
                case Buff.AdrenalineRush:
                    return 6000;
                case Buff.Homing:
                    return 6000;
                case Buff.Penetrating:
                    return 6000;
            }
            return 0;
        }

        protected void AddBuff(Buff b, GameEntity from)
        {
            HandleBuff(b, BuffState.Starting);
            activeBuffs.Add(new PositiveEffect(b, GetBuffLength(b), from));
        }

        protected void AddDebuff(DeBuff b, GameEntity from)
        {
            activeDebuffs.Add(new NegativeEffect(b, GetDebuffLength(b), from));
            HandleDeBuff(b, BuffState.Starting);
        }

        protected bool HasBuff(Buff b)
        {
            for (int i = 0; i < activeBuffs.Count; ++i)
            {
                if (activeBuffs[i].type == b)
                {
                    return true;
                }
            }
            return false;
        }

        protected bool HasDeBuff(DeBuff b)
        {
            for (int i = 0; i < activeDebuffs.Count; ++i)
            {
                if (activeDebuffs[i].type == b)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
