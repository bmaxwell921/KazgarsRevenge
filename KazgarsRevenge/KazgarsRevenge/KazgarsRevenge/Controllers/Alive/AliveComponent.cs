using System;
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
        Vitality,
        Armor,
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
        Headbutt,
        Garrote,
        ForcefulThrow,
        Burning,
        Frozen,
        Execute,
        Charge,
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
        SadisticFrenzy,
        Berserk,
        Berserk2,
        Berserk3,
        Invincibility,
        Unstoppable,
        Swordnado,
    }
    public enum BuffState
    {
        Starting,
        Ticking,
        Ending,
    }
    public class AliveComponent : AIComponent
    {
        //adds power to any entities that use resources
        public virtual void AddPower(int power)
        {

        }

        public long Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float HealthPercent
        {
            get
            {
                if (MaxHealth > 0)
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

        #region Experience
        protected virtual void HandleLevelUp()
        {

        }
        private int level;
        public int Level { get { return Math.Max(1, level); } private set { level = value; } }
        protected float expMultiplier = 1;
        protected int experience { get; private set; }
        public int NextLevelXP { get { return 200 * Level; } }
        public void AddEXP(int level, EntityType entityType)
        {
            int exp = (int)(level * 25 * expMultiplier);
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

        /// <summary>
        /// does level up graphics/sounds, increments level, recalculates stats
        /// </summary>
        public void LevelUp()
        {
            ++Level;
            RecalculateStats();
            HandleLevelUp();

            attacks.CreateLevelUpGraphics(physicalData);

            sounds.PlaySound("levelup");

            (Game as MainGame).AddFloatingText(new FloatingText(physicalData.Position + Vector3.Left * 200, "Level Up", Color.Gold, 4, 90, .45f));

            Health = MaxHealth;
        }
        #endregion

        #region stats
        //used for the pulling state (for chain spear or grappling hook pulls)
        protected bool pulling = false;

        //passive health regen
        protected float percentHPRegenPer5 = 0;

        //these values are multiplied by Level and added to stats
        private Dictionary<StatType, float> statsPerLevel = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 0},
            {StatType.AttackSpeed, 0},
            {StatType.Strength, 1},
            {StatType.Agility, 1},
            {StatType.Intellect, 1},
            {StatType.CooldownReduction, 0},
            {StatType.CritChance, 0},
            {StatType.Vitality, 1},
            {StatType.Armor, 0},
        };

        //shouldn't be modified; used to recalculate stats
        protected Dictionary<StatType, float> originalBaseStats = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 120},
            {StatType.AttackSpeed, 0f},
            {StatType.Strength, 1},
            {StatType.Agility, 1},
            {StatType.Intellect, 1},
            {StatType.CooldownReduction, 0},
            {StatType.CritChance, 0},
            {StatType.Vitality, 10},
            {StatType.Armor, 0}
        };

        //can be modified by buffs to increase stats
        protected Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>()
        {
            {StatType.RunSpeed, 120},
            {StatType.AttackSpeed, 0f},
            {StatType.Strength, 1},
            {StatType.Agility, 1},
            {StatType.Intellect, 1},
            {StatType.CooldownReduction, 0},
            {StatType.CritChance, 0},
            {StatType.Vitality, 10},
            {StatType.Armor, 0}
        };

        //holds all current stats
        private Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

        /// <summary>
        /// Used to access current stats in the controller
        /// </summary>
        protected float GetStat(StatType t)
        {
            return Math.Max(0, stats[t]);
        }

        /// <summary>
        /// Helper to add the stats from an Equippable
        /// </summary>
        protected void AddGearStats(Equippable gear)
        {
            if (gear.StatEffects != null)
            {
                foreach (KeyValuePair<StatType, float> k in gear.StatEffects)
                {
                    if (k.Key == StatType.RunSpeed)
                    {
                        stats[k.Key] += stats[k.Key] * k.Value;
                    }
                    else
                    {
                        stats[k.Key] += k.Value;
                    }
                }
            }
            if (gear.AppliedEssence != null)
            {
                stats[gear.AppliedEssence.BoostedStat] += gear.AppliedEssence.StatIncrease;
            }
        }

        //alter these to make an AliveComponent relatively 
        //stronger/weaker than other things its level
        protected float baseStatsMultiplier = 1;
        protected float statsPerLevelMultiplier = 1;
        /// <summary>
        /// Called whenever gear/buffs are added or level is changed
        /// </summary>
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
                StatType s = (StatType)i;
                stats[s] = baseStats[(StatType)i];
                if (s == StatType.Agility || s == StatType.Strength || s == StatType.Intellect)
                {
                    stats[s] *= baseStatsMultiplier;
                }
            }

            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats[(StatType)i] += statsPerLevel[(StatType)i] * Level * statsPerLevelMultiplier;
            }

            //keep same health percentage when you change gear
            float curHealthPerc = HealthPercent;
            MaxHealth = (int)(stats[StatType.Vitality] * 10);
            Health = (int)(MaxHealth * curHealthPerc);
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

            if (level <= 0)
            {
                level = 1;
            }
            this.Level = level;
            this.Dead = false;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            modelParams = Entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
            sounds = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;

            RecalculateStats();
            this.Health = MaxHealth;
        }

        public override void Start()
        {
            model = (AnimatedModelComponent)Entity.GetComponent(typeof(AnimatedModelComponent));
            base.Start();
        }

        //methods called between controllers
        #region Inter-Controller
        //the entity that killed this one (set after the killing blow happens)
        public AliveComponent Killer { get; protected set; }

        /// <summary>
        /// Called when resurrected or respawning
        /// </summary>
        public void ReviveAlive()
        {
            Killer = null;
            Dead = false;
            Health = (int)(MaxHealth * .25f);
        }

        /// <summary>
        /// Called when player mouses over / targets this entity
        /// </summary>
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

        /// <summary>
        /// Called when killed. Enemies will use this to give the player XP
        /// </summary>
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

        //used by grappling hook and chain spear controllers to move entities
        BEPUphysics.CollisionRuleManagement.CollisionRule prevRule = BEPUphysics.CollisionRuleManagement.CollisionRule.Normal;
        public void Pull()
        {
            prevRule = physicalData.CollisionInformation.CollisionRules.Personal;
            pulling = true;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
        }
        public virtual void StopPull()
        {
            pulling = false;
            physicalData.IsAffectedByGravity = true;
            physicalData.CollisionInformation.CollisionRules.Personal = prevRule;
        }

        /// <summary>
        /// knocks an entity back
        /// </summary>
        /// <param name="origin">origin of the impulse</param>
        /// <param name="impulse">strength of the knockback</param>
        public void KnockBack(Vector3 origin, float impulse)
        {
            Vector3 newVel = physicalData.Position - origin;
            if (newVel != Vector3.Zero)
            {
                newVel.Normalize();
            }
            this.physicalData.LinearVelocity = newVel * impulse;
        }
        #endregion

        #region Damage and Health

        /// <summary>
        /// Damage strenth calculation
        /// </summary>
        protected int GeneratePrimaryDamage(StatType s)
        {
            float ret = stats[s];
            if ((float)rand.Next(0, 1000) / 10f < stats[StatType.CritChance] * 100.0f)
            {
                ret *= 1.35f;
            }

            return (int)ret;
        }

        /// <summary>
        /// calculates the actual amount of damage and then returns what was subtracted
        /// </summary>
        protected int Damage(int d, GameEntity from, bool dodgeable)
        {
            if (dodgeable && activeBuffs.ContainsKey(Buff.Elusiveness))
            {
                //dodge
                return 0;
            }
            int actualDamage = 0;
            if (!activeBuffs.ContainsKey(Buff.Invincibility))
            {
                actualDamage = d;
                actualDamage = Math.Max(0, 
                    actualDamage - (int)(actualDamage * GetStat(StatType.Armor) / (20 * (Level)))
                    );

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
                            Killer = from.GetComponent(typeof(AliveComponent)) as AliveComponent;
                            DealWithKiller();
                        }
                    }
                }
            }

            return actualDamage;
        }

        /// <summary>
        /// Try to damage this alivecomponent
        /// </summary>
        public int Damage(DeBuff db, int d, GameEntity from, AttackType type, bool dodgeable)
        {
            if (db == DeBuff.Burning && activeDebuffs.ContainsKey(DeBuff.Tar))
            {
                d *= 4;
                attacks.SpawnFireExplosionParticles(physicalData.Position, .5f);
            }
            else if (db == DeBuff.Execute && Health < MaxHealth * .2f)
            {
                d *= 15;
            }

            int actualDamage = Damage(d, from, dodgeable);
            TakeDamage(actualDamage, from);

            //handle negative effect
            if (db != DeBuff.None)
            {
                AddDebuff(db, from);
            }

            PlayHitSound(type);

            return actualDamage;
        }

        SoundEffectLibrary sounds;
        protected virtual void PlayHitSound(AttackType t)
        {
            if (t == AttackType.Melee)
            {
                sounds.playMeleeHitSound();
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

        protected AnimatedModelComponent model;
        public void LifeSteal(int h)
        {
            Heal(h);
            model.AddEmitter(typeof(LifestealParticleSystem), "lifesteal", 10, 15, Vector3.Zero);
            model.AddParticleTimer("lifesteal", 1000);
        }

        /// <summary>
        /// Called when damage has already been applied. Used for threat levels 
        /// and graphics associated with damage
        /// </summary>
        /// <param name="damageDealt"></param>
        public virtual void HandleDamageDealt(int damageDealt)
        {

        }
        #endregion

        
        private double regenTimer;
        protected bool showHealthWithOutline = true;
        protected Dictionary<DeBuff, NegativeEffect> activeDebuffs = new Dictionary<DeBuff, NegativeEffect>();
        protected Dictionary<Buff, PositiveEffect> activeBuffs = new Dictionary<Buff, PositiveEffect>();
        public override void Update(GameTime gameTime)
        {
            //adjust model's outline color to reflect health
            if (showHealthWithOutline &&( modelParams.lineColor != Color.Black || HealthPercent != 1))
            {
                modelParams.lineColor = Color.Lerp(Color.Red, Color.Green, HealthPercent);
            }

            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;

            //update buffs/debuffs
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

            regenTimer -= elapsed;
            if (regenTimer <= 0)
            {
                Heal((int)Math.Ceiling(MaxHealth * percentHPRegenPer5));
                regenTimer = 5000;
            }
        }

        #region Buffs and Debuffs
        //structure to hold active debuff information
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
                this.nextTick = timeLeft - tickLength;
                this.from = from;
                this.tickLength = tickLength;
            }
        }

        //called when a stun is applied
        public virtual void HandleStun(double length)
        {
            model.AddEmitter(typeof(StunnedParticleSystem), "stun", 5, 5, Vector3.Up * 15);
            model.AddParticleTimer("stun", length);
        }

        /// <summary>
        /// Definition of Buff behavior
        /// </summary>
        /// <returns>true if the buff was applied, false otherwise</returns>
        private bool HandleBuff(Buff b, BuffState state, int stacks, GameEntity from)
        {
            switch (b)
            {
                case Buff.HealthPotion:
                    if (state == BuffState.Ticking)
                    {
                        Heal((int)Math.Ceiling(MaxHealth * .01f));
                    }
                    break;
                case Buff.SuperHealthPotion:
                    if (state == BuffState.Ticking)
                    {
                        Heal((int)Math.Ceiling(MaxHealth * .02f));
                    }
                    break;
                case Buff.AdrenalineRush:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.RunSpeed] += originalBaseStats[StatType.RunSpeed] * .75f;
                        baseStats[StatType.AttackSpeed] += .6f;
                        RecalculateStats();

                        if(Entity.GetComponent(typeof(AdrenalineRushBillboard)) == null)
                        {
                            AdrenalineRushBillboard billboard = new AdrenalineRushBillboard(Game, Entity, physicalData);
                            Entity.AddComponent(typeof(AdrenalineRushBillboard), billboard);
                            (Game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).AddComponent(billboard);
                        }
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.RunSpeed] -= originalBaseStats[StatType.RunSpeed] * .75f * stacks;
                        baseStats[StatType.AttackSpeed] -= .6f * stacks;
                        RecalculateStats();

                        Entity.RemoveComponent(typeof(AdrenalineRushBillboard));
                    }
                    break;
                case Buff.SadisticFrenzy:
                    if (state == BuffState.Starting)
                    {
                        baseStats[StatType.AttackSpeed] += originalBaseStats[StatType.AttackSpeed] * .2f;
                        RecalculateStats();
                    }
                    else if (state == BuffState.Ending)
                    {
                        baseStats[StatType.AttackSpeed] -= originalBaseStats[StatType.AttackSpeed] * .2f * stacks;
                        RecalculateStats();
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// Definition of Debuff behavior
        /// </summary>
        /// <returns>true if the buff was applied, false otherwise (to limit certain debuffs, like slows)</returns>
        private bool HandleDeBuff(DeBuff d, BuffState state, int stacks, GameEntity from)
        {
            switch (d)
            {
                case DeBuff.Burning:
                    if (state == BuffState.Ticking)
                    {
                        Damage((int)Math.Ceiling(MaxHealth * .035f * stacks), from, true);
                        attacks.SpawnLittleBloodSpurt(physicalData.Position);
                    }
                    break;
                case DeBuff.Frozen:
                    if (state == BuffState.Starting)
                    {
                        AddDebuff(DeBuff.Stunned, debuffLengths[DeBuff.Frozen], from);
                        attacks.CreateIceBlock(physicalData.Position, debuffLengths[DeBuff.Frozen]);
                    }
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
                        Damage((int)Math.Ceiling(MaxHealth * .02f * stacks), from, true);
                        attacks.SpawnLittleBloodSpurt(physicalData.Position);
                    }
                    break;
                case DeBuff.Garrote:
                    if (state == BuffState.Ticking)
                    {
                        Damage((int)Math.Ceiling(MaxHealth * .05f * stacks), from, true);
                        attacks.SpawnLittleBloodSpurt(physicalData.Position);
                    }
                    break;
                case DeBuff.FlashBomb:
                    if (state == BuffState.Starting)
                    {
                        AddDebuff(DeBuff.Stunned, debuffLengths[DeBuff.FlashBomb], null);
                    }
                    break;
                case DeBuff.Headbutt:
                    if (state == BuffState.Starting)
                    {
                        AddDebuff(DeBuff.Stunned, debuffLengths[DeBuff.Headbutt], null);
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
                case DeBuff.Charge:
                    if (state == BuffState.Starting)
                    {
                        pulling = true;
                    }
                    else if (state == BuffState.Ending)
                    {
                        pulling = false;
                        physicalData.LinearVelocity = Vector3.Zero;
                    }
                    break;
            }

            return true;
        }

        Dictionary<DeBuff, double> debuffLengths = new Dictionary<DeBuff, double>()
        {
            {DeBuff.SerratedBleeding, 5000},
            {DeBuff.Garrote, 2000},
            {DeBuff.Frost, 1500},
            {DeBuff.FlashBomb, 3000},
            {DeBuff.Headbutt, 5000},
            {DeBuff.Tar, 9000},
            {DeBuff.MagneticImplant, 2000},
            {DeBuff.ForcefulThrow, 3000},
            {DeBuff.Burning, 0},
            {DeBuff.Frozen, 1500},
            {DeBuff.Charge, 100}
        };

        Dictionary<DeBuff, double> debuffTickLengths = new Dictionary<DeBuff, double>()
        {
            {DeBuff.SerratedBleeding, 1000},
            {DeBuff.Garrote, 1000},
            {DeBuff.Burning, 1500}
        };

        Dictionary<Buff, double> buffLengths = new Dictionary<Buff, double>()
        {
            {Buff.AdrenalineRush, 6000},
            {Buff.SadisticFrenzy, 4000},
            {Buff.Homing, 6000},
            {Buff.SerratedBleeding, 6000},
            {Buff.Elusiveness, 3000},
            {Buff.HealthPotion, 5000},
            {Buff.SuperHealthPotion, 5000},
            {Buff.LuckPotion, 120000},
            {Buff.Invincibility, 5000},
        };

        Dictionary<Buff, double> buffTickLengths = new Dictionary<Buff, double>()
        {
            {Buff.HealthPotion, 250},
            {Buff.SuperHealthPotion, 250}
        };

        /// <summary>
        /// Add the given buff to activeBuffs, using the length value from buffLengths
        /// </summary>
        protected void AddBuff(Buff b, GameEntity from)
        {
            double length = 0;
            if (buffLengths.ContainsKey(b))
            {
                length = buffLengths[b];
            }
            AddBuff(b, length, from);
        }

        /// <summary>
        /// Add the given buff to activeBuffs, specifying a length
        /// </summary>
        protected void AddBuff(Buff b, double length, GameEntity from)
        {
            double tickLength = double.MaxValue;

            if (buffTickLengths.ContainsKey(b))
            {
                tickLength = buffTickLengths[b];
            }

            if (HandleBuff(b, BuffState.Starting, 1, from))
            {
                if (activeBuffs.ContainsKey(b))
                {
                    if (b != Buff.Invincibility)
                    {
                        activeBuffs[b].stacks += 1;
                    }
                    activeBuffs[b].nextTick = length - (activeBuffs[b].timeLeft - activeBuffs[b].nextTick);
                    activeBuffs[b].timeLeft = length;
                }
                else
                {
                    activeBuffs.Add(b, new PositiveEffect(b, length, from, tickLength));
                }
            }
        }

        /// <summary>
        /// Add the given buff to activeBuffs, specifying a length
        /// </summary>
        protected void AddDebuff(DeBuff b, GameEntity from)
        {
            double length = 0;
            if (debuffLengths.ContainsKey(b))
            {
                length = debuffLengths[b];
            }
            AddDebuff(b, length, from);
        }

        /// <summary>
        /// Add the given buff to activeBuffs, using the length value from buffLengths
        /// </summary>
        private void AddDebuff(DeBuff b, double length, GameEntity from)
        {
            if (b == DeBuff.Stunned)
            {
                if (activeBuffs.ContainsKey(Buff.Unstoppable))
                {
                    return;
                }
                HandleStun(length);
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
                    activeDebuffs[b].nextTick = length - (activeDebuffs[b].timeLeft - activeDebuffs[b].nextTick);
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
        #endregion
    }
}
