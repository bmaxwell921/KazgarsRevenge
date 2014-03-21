﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModelLib;

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
        Burning,
        Frozen,
    }
    public enum Buff
    {
        None,
        HealthPotion,
        SuperHealthPotion,
        LuckPotion,
        AdrenalineRush,
        Homing,
        SerratedBleeding,
        Elusiveness,
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
            public GameEntity from { get; private set; }
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
            public GameEntity from { get; private set; }
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

        public virtual void AddPower(int power)
        {

        }

        protected int experience = 0;
        public int NextLevelXP { get { return 100 * level * level; } }
        public void AddEXP(int level, EntityType entityType)
        {
            int exp = level * 25;
            if (entityType == EntityType.EliteEnemy)
            {
                exp *= 2;
            }
            if (entityType == EntityType.Boss)
            {
                exp *= 10;
            }

            experience += exp;
            while (experience >= NextLevelXP)
            {
                experience = experience - NextLevelXP;
                LevelUp();
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

        public GameEntity Killer { get; protected set; }

        public void ReviveAlive()
        {
            Killer = null;
            Dead = false;
            Health = (int)(MaxHealth * .25f);
        }

        #region stats
        protected int level = 0;
        protected bool pulling = false;
        
        private Dictionary<StatType, float> statsPerLevel = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 2},
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
            if (gear.StatEffects != null)
            {
                foreach (KeyValuePair<StatType, float> k in gear.StatEffects)
                {
                    stats[k.Key] += k.Value;
                }
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

        public void LevelUp()
        {
            ++level;
            RecalculateStats();
        }
        #endregion

        protected AnimationPlayer animations;
        protected AttackManager attacks;
        protected Random rand;
        protected SharedGraphicsParams modelParams;
        public AliveComponent(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity)
        {
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            rand = game.rand;

            this.MaxHealth = 100 * level;
            this.Health = MaxHealth;
            this.level = level;
            this.Dead = false;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            modelParams = Entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
            
            RecalculateStats();
        }

        public override void Start()
        {
            model = (AnimatedModelComponent)Entity.GetComponent(typeof(AnimatedModelComponent));
            base.Start();
        }

        public void Target()
        {
            if (showHealthWithOutline)
            {
                modelParams.lineColor = Color.Lerp(Color.Red, Color.Green, HealthPercent);
            }
            else
            {
                modelParams.lineColor = Color.LightBlue;
            }
        }

        public void UnTarget()
        {
            if (HealthPercent == 1 || !showHealthWithOutline)
            {
                modelParams.lineColor = Color.Black;
            }
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
        protected int Damage(int d, GameEntity from)
        {
            //TODO: armor and resistance calculations
            int actualDamage = d;
            if (!Dead)
            {
                if (activeDebuffs.ContainsKey(DeBuff.MagneticImplant))
                {
                    actualDamage *= 2;
                }
                Health -= actualDamage;
                if (Health <= 0)
                {
                    Health = 0;
                    KillAlive();
                    if (Dead)
                    {
                        Killer = from;
                        DealWithKiller();
                    }
                }
            }
            return actualDamage;
        }

        protected virtual void DealWithKiller()
        {

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

        public int DamageDodgeable(DeBuff db, int d, GameEntity from)
        {
            if (activeBuffs.ContainsKey(Buff.Elusiveness))
            {
                //dodge
                return 0;
            }
            else
            {
                return Damage(db, d, from);
            }
        }

        public int Damage(DeBuff db, int d, GameEntity from)
        {
            if (db == DeBuff.Burning && activeDebuffs.ContainsKey(DeBuff.Tar))
            {
                d *= 4;
                attacks.SpawnFireExplosionParticles(physicalData.Position, .5f);
            }

            int actualDamage = Damage(d, from);
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

        protected AnimatedModelComponent model;
        public void LifeSteal(int h)
        {
            Heal(h);
            model.AddEmitter(typeof(LifestealParticleSystem), "lifesteal", 10, 15, Vector3.Zero);
            model.AddParticleTimer("lifesteal", 1000);
        }

        protected bool showHealthWithOutline = true;
        protected Dictionary<DeBuff, NegativeEffect> activeDebuffs = new Dictionary<DeBuff, NegativeEffect>();
        protected Dictionary<Buff, PositiveEffect> activeBuffs = new Dictionary<Buff, PositiveEffect>();
        public override void Update(GameTime gameTime)
        {
            if (showHealthWithOutline &&( modelParams.lineColor != Color.Black || HealthPercent != 1))
            {
                modelParams.lineColor = Color.Lerp(Color.Red, Color.Green, HealthPercent);
            }

            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;

            List<DeBuff> toRemoveDebuffs = new List<DeBuff>();
            foreach (KeyValuePair<DeBuff, NegativeEffect> k in activeDebuffs)
            {
                NegativeEffect cur = k.Value;
                cur.timeLeft -= elapsed;
                if (cur.timeLeft < cur.nextTick)
                {
                    HandleDeBuff(cur.type, BuffState.Ticking, cur.stacks, cur.from);
                    cur.nextTick -= cur.tickLength;
                }
                if (cur.timeLeft <= 0 || Dead)
                {
                    HandleDeBuff(cur.type, BuffState.Ending, cur.stacks, cur.from);
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
                    HandleBuff(cur.type, BuffState.Ticking, cur.stacks, cur.from);
                    cur.nextTick -= cur.tickLength;
                }
                if (cur.timeLeft <= 0 || Dead)
                {
                    HandleBuff(cur.type, BuffState.Ending, cur.stacks, cur.from);
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

        private bool HandleBuff(Buff b, BuffState state, int stacks, GameEntity from)
        {
            switch (b)
            {
                case Buff.HealthPotion:
                    if (state == BuffState.Ticking)
                    {
                        Heal((int)(MaxHealth * .1f));
                    }
                    break;
                case Buff.SuperHealthPotion:
                    if (state == BuffState.Ticking)
                    {
                        Heal((int)(MaxHealth * .2f));
                    }
                    break;
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
            return true;
        }

        /// <summary>
        /// returns true if it applied the debuff, false if no (to limit certain debuffs, like slows)
        /// </summary>
        private bool HandleDeBuff(DeBuff d, BuffState state, int stacks, GameEntity from)
        {
            switch (d)
            {
                case DeBuff.Burning:
                    if (state == BuffState.Ticking)
                    {
                        Damage((int)Math.Ceiling(MaxHealth * .035f * stacks), from);
                        attacks.SpawnLittleBloodSpurt(physicalData.Position);
                    }
                    break;
                case DeBuff.Frozen:

                    break;
                case DeBuff.Frost:
                    if (state == BuffState.Starting)
                    {
                        if (activeDebuffs.ContainsKey(DeBuff.Frost) && activeDebuffs[DeBuff.Frost].stacks > 7)
                        {
                            AddDebuff(DeBuff.Frozen, null);
                            return false;
                        }
                        model.AddEmitter(typeof(FrostDebuffParticleSystem), "frosttrail", 10, 3, Vector3.Zero);
                        model.AddParticleTimer("frosttrail", debuffLengths[DeBuff.Frost]);
                        baseStats[StatType.RunSpeed] -= originalBaseStats[StatType.RunSpeed] * .15f;
                        RecalculateStats();
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] += originalBaseStats[StatType.RunSpeed] * .15f * stacks;
                        RecalculateStats();
                    }
                    break;
                case DeBuff.SerratedBleeding:
                    if (state == BuffState.Ticking)
                    {
                        Damage((int)Math.Ceiling(MaxHealth * .02f * stacks), from);
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
                        model.AddEmitter(typeof(TarDebuffParticleSystem), "tar", 10, 10, Vector3.Zero);
                        model.AddParticleTimer("tar", debuffLengths[DeBuff.Tar]);
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

            return true;
        }

        Dictionary<DeBuff, double> debuffLengths = new Dictionary<DeBuff, double>()
        {
            {DeBuff.SerratedBleeding, 5000},
            {DeBuff.Frost, 1500},
            {DeBuff.FlashBomb, 3000},
            {DeBuff.Tar, 6000},
            {DeBuff.MagneticImplant, 2000},
            {DeBuff.ForcefulThrow, 3000},
            {DeBuff.Burning, 0},
            {DeBuff.Frozen, 1500}
        };
        Dictionary<DeBuff, double> debuffTickLengths = new Dictionary<DeBuff, double>()
        {
            {DeBuff.SerratedBleeding, 1000},
            {DeBuff.Burning, 1500}
        };

        Dictionary<Buff, double> buffLengths = new Dictionary<Buff, double>()
        {
            {Buff.AdrenalineRush, 6000},
            {Buff.Homing, 6000},
            {Buff.SerratedBleeding, 6000},
            {Buff.Elusiveness, 6000},
            {Buff.HealthPotion, 5000},
            {Buff.SuperHealthPotion, 5000},
            {Buff.LuckPotion, 120000}
        };

        Dictionary<Buff, double> buffTickLengths = new Dictionary<Buff, double>()
        {
            {Buff.HealthPotion, 250},
            {Buff.SuperHealthPotion, 250}
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

            if (HandleBuff(b, BuffState.Starting, 1, from))
            {
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
                model.AddEmitter(typeof(StunnedParticleSystem), "stun", 5, 5, Vector3.Up * 15);
                model.AddParticleTimer("stun", length);
            }

            double tickLength = double.MaxValue;

            if (debuffTickLengths.ContainsKey(b))
            {
                tickLength = debuffTickLengths[b];
            }


            if (activeDebuffs.ContainsKey(b))
            {
                if (HandleDeBuff(b, BuffState.Starting, activeDebuffs[b].stacks, from))
                {
                    activeDebuffs[b].stacks += 1;
                    activeDebuffs[b].nextTick = activeDebuffs[b].timeLeft - activeDebuffs[b].nextTick;
                    activeDebuffs[b].timeLeft = length;
                }
            }
            else
            {
                if (HandleDeBuff(b, BuffState.Starting, 1, from))
                {
                    activeDebuffs.Add(b, new NegativeEffect(b, length, from, tickLength));
                }
            }

        }

    }
}
