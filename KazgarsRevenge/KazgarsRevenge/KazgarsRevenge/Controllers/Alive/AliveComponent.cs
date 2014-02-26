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
        None,
        Frost,
        SerratedBleeding,
        MagneticImplant,
        FlashBomb,
        Tar,
        Stunned,//hide this from gui (just show description for flashbomb or forceful throw or w/e)
        ForcefulThrow,
        Igniting,
    }
    public enum Buff
    {
        None,
        AdrenalineRush,
        Homing,
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
            public double tickLength { get; private set; }
            GameEntity from;
            public NegativeEffect(DeBuff type, double time, GameEntity from, double tickLength)
            {
                this.type = type;
                this.timeLeft = time;
                this.nextTick = time - tickLength;
                this.from = from;
                this.tickLength = tickLength;
            }
        }

        class PositiveEffect
        {
            public Buff type { get; private set; }
            public double timeLeft;
            public double nextTick;
            public double tickLength { get; private set; }
            GameEntity from;
            public PositiveEffect(Buff type, double time, GameEntity from, double tickLength)
            {
                this.type = type;
                this.timeLeft = time;
                this.nextTick = -10000;
                this.from = from;
                this.tickLength = tickLength;
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
        protected bool pulling = false;
        
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
            if (stats.Count == 0)
            {
                for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
                {
                    stats.Add((StatType)i, 0);
                }
            }
            //add base stats, accounting for level
            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                if (baseStats[(StatType)i] < 0)
                {
                    baseStats[(StatType)i] = 0;
                }
                stats[(StatType)i] = baseStats[(StatType)i];
            }

            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats[(StatType)i] += statsPerLevel[(StatType)i] * level;
            }
        }
        #endregion

        protected AttackManager attacks;
        protected Random rand;
        public AliveComponent(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity)
        {
            rand = game.rand;

            this.MaxHealth = 100 * level;
            this.Health = MaxHealth;
            this.level = level;
            this.Dead = false;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            RecalculateStats();
        }

        /// <summary>
        /// damage on the controller's part. Deals with aggro, animations, and particles
        /// </summary>
        protected virtual void TakeDamage(int damage, GameEntity from)
        {

        }

        protected int GeneratePrimaryDamage(StatType s)
        {
            float ret = stats[s] * 10;
            if (rand.Next(0, 101) < stats[StatType.CritChance])
            {
                ret *= 1.25f;
            }

            return (int)ret;
        }

        /// <summary>
        /// calculates the actual amount of damage and then returns what was subtracted
        /// </summary>
        protected int Damage(int d, bool truedamage)
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

        public void Pull()
        {
            pulling = true;
        }

        public virtual void StopPull()
        {
            pulling = false;
        }

        public int Damage(DeBuff db, int d, GameEntity from)
        {
            if (db == DeBuff.Igniting && HasDeBuff(DeBuff.Tar))
            {
                d *= 4;
                attacks.SpawnExplosionParticles(physicalData.Position);
            }

            int actualDamage = Damage(d, false);
            TakeDamage(actualDamage, from);



            //handle negative effect
            if (db != DeBuff.None)
            {
                AddDebuff(db, from);
            }

            return actualDamage;
        }

        private void TryIgnite(int d)
        {
            if (HasDeBuff(DeBuff.Tar))
            {

            }
        }

        protected void Heal(int h)
        {
            if (!Dead)
            {
                Health += h;
                if (Health > MaxHealth)
                {
                    Health = MaxHealth;
                }
            }
        }

        AnimatedModelComponent model;
        public void LifeSteal(int h)
        {
            Heal(h);
            if (model == null)
            {
                model = Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent;
            }
            model.AddEmitter(typeof(LifestealParticleSystem), "lifesteal", 10, 15, Vector3.Zero);
            model.AddParticleTimer("lifesteal", 1000);
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
                    activeDebuffs[i].nextTick -= activeDebuffs[i].tickLength;
                }
                if (activeDebuffs[i].timeLeft <= 0 || Dead)
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
                    activeBuffs[i].nextTick -= activeBuffs[i].tickLength;
                }
                if (activeBuffs[i].timeLeft <= 0 || Dead)
                {
                    HandleBuff(activeBuffs[i].type, BuffState.Ending);
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        public virtual void HandleDamageDealt(int damageDealt)
        {

        }

        public virtual void HandleStun()
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
                case DeBuff.Frost:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.RunSpeed] -= baseStats[StatType.RunSpeed] * .5f;
                        RecalculateStats();
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] = originalBaseStats[StatType.RunSpeed];
                        RecalculateStats();
                    }
                    break;
                case DeBuff.SerratedBleeding:
                    if (state == BuffState.Ticking)
                    {
                        Damage((int)Math.Ceiling(MaxHealth * .01f), true);
                        attacks.SpawnLittleBloodSpurt(physicalData.Position);
                    }
                    break;
                case DeBuff.FlashBomb:
                    if (state == BuffState.Starting)
                    {
                        AddDebuff(DeBuff.Stunned, debuffLengths[DeBuff.FlashBomb], null);
                    }
                    break;
                case DeBuff.Tar:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.RunSpeed] -= 80;
                        RecalculateStats();
                    }
                    if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] = originalBaseStats[StatType.RunSpeed];
                        RecalculateStats();
                    }
                    break;
                case DeBuff.ForcefulThrow:
                    if (state == BuffState.Starting)
                    {
                        AddDebuff(DeBuff.Stunned, debuffLengths[DeBuff.ForcefulThrow], null);
                    }
                    break;
            }
        }

        Dictionary<DeBuff, double> debuffLengths = new Dictionary<DeBuff, double>()
        {
            {DeBuff.SerratedBleeding, 5000},
            {DeBuff.Frost, 1000},
            {DeBuff.FlashBomb, 3000},
            {DeBuff.Tar, 6000},
            {DeBuff.MagneticImplant, 2000},
            {DeBuff.ForcefulThrow, 3000},
            {DeBuff.Igniting, 0}
        };
        Dictionary<DeBuff, double> debuffTickLengths = new Dictionary<DeBuff, double>()
        {
            {DeBuff.SerratedBleeding, 1000}
        };

        Dictionary<Buff, double> buffLengths = new Dictionary<Buff, double>()
        {
            {Buff.AdrenalineRush, 6000},
            {Buff.Homing, 6000},
            {Buff.SerratedBleeding, 6000},
        };

        Dictionary<Buff, double> buffTickLengths = new Dictionary<Buff, double>()
        {

        };

        protected void AddBuff(Buff b, GameEntity from)
        {
            double length = 0;
            double tickLength = double.MaxValue;

            if (buffLengths.ContainsKey(b))
            {
                length = buffLengths[b];
            }

            if (buffTickLengths.ContainsKey(b))
            {
                tickLength = buffTickLengths[b];
            }

            HandleBuff(b, BuffState.Starting);
            activeBuffs.Add(new PositiveEffect(b, length, from, tickLength));
        }

        protected void AddDebuff(DeBuff b, GameEntity from)
        {

            double length = 0;
            if (debuffLengths.ContainsKey(b))
            {
                length = debuffLengths[b];
            }
            AddDebuff(b, length, from);
        }

        private void AddDebuff(DeBuff b, double length, GameEntity from)
        {
            if (b == DeBuff.Stunned)
            {
                HandleStun();
            }

            double tickLength = double.MaxValue;

            if (debuffTickLengths.ContainsKey(b))
            {
                tickLength = debuffTickLengths[b];
            }

            activeDebuffs.Add(new NegativeEffect(b, length, from, tickLength));
            HandleDeBuff(b, BuffState.Starting);
        }

        public bool HasBuff(Buff b)
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

        public bool HasDeBuff(DeBuff b)
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
