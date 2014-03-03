﻿using System;
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

        protected class NegativeEffect
        {
            public DeBuff type { get; private set; }
            public double timeLeft;
            public double nextTick;
            public double tickLength { get; private set; }
            GameEntity from;
            public int stacks = 1;
            public NegativeEffect(DeBuff type, double time, GameEntity from, double tickLength)
            {
                this.type = type;
                this.timeLeft = time;
                this.nextTick = time - tickLength;
                this.from = from;
                this.tickLength = tickLength;
            }
        }

        protected class PositiveEffect
        {
            public Buff type { get; private set; }
            public double timeLeft;
            public double nextTick;
            public double tickLength { get; private set; }
            GameEntity from;
            public int stacks = 1;
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

        private Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

        protected float GetStat(StatType t)
        {
            float ret = stats[t];
            if (ret < 0)
            {
                return 0;
            }
            return ret;
        }

        protected void AddGearStats(Equippable gear)
        {
            foreach (KeyValuePair<StatType, float> k in gear.StatEffects)
            {
                stats[k.Key] += k.Value;
            }
        }

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
        protected SharedGraphicsParams modelParams;
        public AliveComponent(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity)
        {
            rand = game.rand;

            this.MaxHealth = 100 * level;
            this.Health = MaxHealth;
            this.level = level;
            this.Dead = false;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            modelParams = Entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
            RecalculateStats();
        }

        public void Target()
        {
            modelParams.lineColor = Color.Red;
        }

        public void UnTarget()
        {
            modelParams.lineColor = Color.Black;
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
            if (activeDebuffs.ContainsKey(DeBuff.MagneticImplant))
            {
                actualDamage *= 2;
            }
            Health -= actualDamage;
            if (Health <= 0)
            {
                Health = 0;
                KillAlive();
            }
            return actualDamage;
        }

        protected virtual void KillAlive()
        {
            Dead = true;
            if (pulling)
            {
                StopPull();
            }
        }

        public void Pull()
        {
            pulling = true;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
        }

        public virtual void StopPull()
        {
            pulling = false;
            physicalData.IsAffectedByGravity = true;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.Normal;
        }

        public int Damage(DeBuff db, int d, GameEntity from)
        {
            if (db == DeBuff.Igniting && activeDebuffs.ContainsKey(DeBuff.Tar))
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

        protected Dictionary<DeBuff, NegativeEffect> activeDebuffs = new Dictionary<DeBuff, NegativeEffect>();
        protected Dictionary<Buff, PositiveEffect> activeBuffs = new Dictionary<Buff, PositiveEffect>();
        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;

            List<DeBuff> toRemoveDebuffs = new List<DeBuff>();
            foreach (KeyValuePair<DeBuff, NegativeEffect> k in activeDebuffs)
            {
                NegativeEffect cur = k.Value;
                cur.timeLeft -= elapsed;
                if (cur.timeLeft < cur.nextTick)
                {
                    HandleDeBuff(cur.type, BuffState.Ticking, cur.stacks);
                    cur.nextTick -= cur.tickLength;
                }
                if (cur.timeLeft <= 0 || Dead)
                {
                    HandleDeBuff(cur.type, BuffState.Ending, cur.stacks);
                    toRemoveDebuffs.Add(cur.type);
                }
            }
            for (int i = toRemoveDebuffs.Count - 1; i >= 0; --i)
            {
                activeDebuffs.Remove(toRemoveDebuffs[i]);
            }

            List<Buff> toRemoveBuffs = new List<Buff>();
            foreach (KeyValuePair<Buff, PositiveEffect> k in activeBuffs)
            {
                PositiveEffect cur = k.Value;
                cur.timeLeft -= elapsed;
                if (cur.timeLeft < cur.nextTick)
                {
                    HandleBuff(cur.type, BuffState.Ticking, cur.stacks);
                    cur.nextTick -= cur.tickLength;
                }
                if (cur.timeLeft <= 0 || Dead)
                {
                    HandleBuff(cur.type, BuffState.Ending, cur.stacks);
                    toRemoveBuffs.Add(cur.type);
                }
            }
            for (int i = toRemoveBuffs.Count - 1; i >= 0; --i)
            {
                activeBuffs.Remove(toRemoveBuffs[i]);
            }


        }

        public virtual void HandleDamageDealt(int damageDealt)
        {

        }

        public virtual void HandleStun()
        {

        }

        private void HandleBuff(Buff b, BuffState state, int stacks)
        {
            switch (b)
            {
                case Buff.AdrenalineRush:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.RunSpeed] += originalBaseStats[StatType.RunSpeed] * .75f;
                        baseStats[StatType.AttackSpeed] += .6f;
                        RecalculateStats();
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] -= originalBaseStats[StatType.RunSpeed] * .75f * stacks;
                        baseStats[StatType.AttackSpeed] -= .6f * stacks;
                        RecalculateStats();
                    }
                    break;
            }
        }

        private void HandleDeBuff(DeBuff d, BuffState state, int stacks)
        {
            switch (d)
            {
                case DeBuff.Frost:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.RunSpeed] -= originalBaseStats[StatType.RunSpeed] * .1f;
                        RecalculateStats();
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] += originalBaseStats[StatType.RunSpeed] * .1f * stacks;
                        RecalculateStats();
                    }
                    break;
                case DeBuff.SerratedBleeding:
                    if (state == BuffState.Ticking)
                    {
                        Damage((int)Math.Ceiling(MaxHealth * .01f * stacks), true);
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
                        baseStats[StatType.RunSpeed] -= originalBaseStats[StatType.RunSpeed] * .75f;
                        RecalculateStats();
                    }
                    if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] += originalBaseStats[StatType.RunSpeed] * .75f * stacks;
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

            HandleBuff(b, BuffState.Starting, 1);
            if (activeBuffs.ContainsKey(b))
            {
                activeBuffs[b].stacks += 1;
                activeBuffs[b].nextTick = activeBuffs[b].timeLeft - activeBuffs[b].nextTick;
                activeBuffs[b].timeLeft = length;
            }
            else
            {
                activeBuffs.Add(b, new PositiveEffect(b, length, from, tickLength));
            }
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

            HandleDeBuff(b, BuffState.Starting, 1);

            if (activeDebuffs.ContainsKey(b))
            {
                activeDebuffs[b].stacks += 1;
                activeDebuffs[b].nextTick = activeDebuffs[b].timeLeft - activeDebuffs[b].nextTick;
                activeDebuffs[b].timeLeft = length;
            }
            else
            {
                activeDebuffs.Add(b, new NegativeEffect(b, length, from, tickLength));
            }
        }

    }
}
