using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.ResourceManagement;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Collidables;
using SkinnedModelLib;
using KazgarsRevenge.Libraries;

namespace KazgarsRevenge
{
    public enum AttackType
    {
        None,
        Melle,
        Ranged,
        Magic,
    }
    public enum GearSlot
    {
        None,
        Head,
        Chest,
        Legs,
        Feet,
        Righthand,
        Lefthand,
    }
    
    public abstract class PlayerController : AliveComponent
    {

        #region member variables
        //services
        protected Space physics;
        protected CameraComponent camera;
        protected SoundEffectLibrary soundEffects;
        protected AttackManager attacks;
        protected LootManager gearGenerator;
        protected NetworkMessageManager nmm;

        //data
        protected AnimationPlayer animations;
        protected Dictionary<string, AttachableModel> attached;
        protected Random rand = new Random();


        //variables for movement
        protected float stopRadius = 10;
        protected const float targetResetDistance = 1000;
        protected Vector3 mouseHoveredLocation = Vector3.Zero;
        protected Vector3 groundTargetLocation = Vector3.Zero;
        protected double millisRunningCounter = 2000;
        protected double millisRunTime = 2000;
        protected double millisCombatLength = 4000;
        protected double millisCombatCounter = 0;
        protected bool inCombat = false;
        protected bool looting = false;

        #endregion

        #region Abilities
        protected const float melleRange = 50;
        protected const float bowRange = 1000;
        protected Dictionary<string, Ability> allAbilities = new Dictionary<string, Ability>();
        protected KeyValuePair<Keys, Ability>[] boundAbilities = new KeyValuePair<Keys, Ability>[8];
        protected Dictionary<AbilityName, bool> abilityLearnedFlags = new Dictionary<AbilityName, bool>();
        #endregion

        #region inventory and gear
        protected int gold = 0;
        protected const int NUM_LOOT_SHOWN = 4;
        protected int maxInventorySlots = 16;
        protected LootSoulController lootingSoul = null;
        protected Dictionary<GearSlot, Equippable> gear = new Dictionary<GearSlot, Equippable>();
        //inventory
        protected List<Item> inventory = new List<Item>();
        protected virtual void EquipGear(Equippable equipMe, GearSlot slot)
        {
            float xRot = 0;
            //if the player is trying to equip a two-handed weapon to the offhand, unequip both current weapons and equip it to the main hand
            Weapon possWep = equipMe as Weapon;
            if (possWep != null)
            {
                if (possWep.TwoHanded)
                {
                    if (slot == GearSlot.Lefthand)
                    {
                        EquipGear(equipMe, GearSlot.Righthand);
                        return;
                    }
                    if (!UnequipGear(GearSlot.Lefthand))
                    {
                        return;
                    }
                }

                if (slot == GearSlot.Righthand)
                {
                    xRot = MathHelper.Pi;
                }
            }

            //otherwise, carry on
            if (UnequipGear(slot))
            {
                gear[slot] = equipMe;
                RecalculateStats();

                attached.Add(slot.ToString(), new AttachableModel(equipMe.GearModel, GearSlotToBoneName(slot), xRot, 0));
            }
        }
        /// <summary>
        /// tries to put equipped item into inventory. If there was no inventory space, returns false.
        /// </summary>
        protected bool UnequipGear(GearSlot slot)
        {
            Equippable oldEquipped = gear[slot];
            if (oldEquipped != null)
            {
                if (AddToInventory(oldEquipped))
                {
                    attached.Remove(slot.ToString());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //if there was nothing in there to start with, return true
                return true;
            }
        }
        /// <summary>
        /// tries to add item to inventory if there is space. returns false if no space left
        /// </summary>
        public bool AddToInventory(Item toAdd)
        {
            if (toAdd == null)
            {
                return true;
            }

            if (inventory.Count >= maxInventorySlots)
            {
                return false;
            }

            if (toAdd.Name == "gold")
            {
                gold += toAdd.Quantity;
                return true;
            }

            inventory.Add(toAdd);
            return true;
        }
        protected override void RecalculateStats()
        {
            base.RecalculateStats();

            //add stats for each piece of gear
            foreach (KeyValuePair<GearSlot, Equippable> k in gear)
            {
                if (k.Value != null)
                {
                    AddStats(k.Value.StatEffects);
                }
            }

            stateResetLength = 3000 * (1 - Math.Min(.95f, stats[StatType.AttackSpeed]));
        }
        protected void AddStats(Dictionary<StatType, float> statsToAdd)
        {
            foreach (KeyValuePair<StatType, float> k in statsToAdd)
            {
                stats[k.Key] += k.Value;
            }
        }
        protected AttackType GetMainhandType()
        {
            Equippable e = gear[GearSlot.Righthand];
            if (e != null)
            {
                return (e as Weapon).PrimaryAttackType;
            }
            else
            {
                return AttackType.None;
            }
        }
        protected AttackType GetOffhandType()
        {
            Equippable e = gear[GearSlot.Lefthand];
            if (e != null)
            {
                return (e as Weapon).PrimaryAttackType;
            }
            else
            {
                return AttackType.None;
            }
        }

        protected void UseItem(Item i)
        {
            switch (i.Type)
            {
                case ItemType.Equippable:
                    Equippable equip = i as Equippable;
                    if (equip != null)
                    {
                        EquipGear(equip, equip.Slot);
                    }
                    break;
                case ItemType.Potion:

                    break;
                case ItemType.Recipe:

                    break;
                case ItemType.Essence:

                    break;
            }
        }
        #endregion

        public PlayerController(KazgarsRevengeGame game, GameEntity entity, PlayerSave savefile)
            : base(game, entity, savefile.CharacterLevel)
        {
            //shared data
            this.animations = Entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.attached = Entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;

            InitGeneralFields();

            InitNewPlayer();

            //adding sword and bow for demo
            EquipGear(gearGenerator.GenerateSword(), GearSlot.Righthand);
            EquipGear(gearGenerator.GenerateBow(), GearSlot.Lefthand);
        }

        private void InitPlayerFromFile()
        {

        }

        private void InitGeneralFields()
        {
            #region members
            //services
            physics = Game.Services.GetService(typeof(Space)) as Space;
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            soundEffects = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;
            gearGenerator = Game.Services.GetService(typeof(LootManager)) as LootManager;
            nmm = Game.Services.GetService(typeof(NetworkMessageManager)) as NetworkMessageManager;
            #endregion

            #region animation setup
            PlayAnimation("k_idle1", MixType.None);

            SetUpActionSequences();
            #endregion
        }

        private void InitNewPlayer()
        {
            #region stats and inventory
            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats.Add((StatType)i, 0);
            }

            RecalculateStats();

            for (int i = 0; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
            {
                gear.Add((GearSlot)i, null);
            }
            #endregion

            #region ability initialization
            //create initial abilities

            boundAbilities[0] = new KeyValuePair<Keys, Ability>(Keys.Q, GetAbility(AbilityName.HeartStrike));
            boundAbilities[1] = new KeyValuePair<Keys, Ability>(Keys.W, GetAbility(AbilityName.Snipe));
            boundAbilities[2] = new KeyValuePair<Keys, Ability>(Keys.E, GetAbility(AbilityName.Omnishot));
            boundAbilities[3] = new KeyValuePair<Keys, Ability>(Keys.R, GetAbility(AbilityName.AdrenalineRush));
            boundAbilities[4] = new KeyValuePair<Keys, Ability>(Keys.A, GetAbility(AbilityName.HeartStrike));
            boundAbilities[5] = new KeyValuePair<Keys, Ability>(Keys.S, GetAbility(AbilityName.HeartStrike));
            boundAbilities[6] = new KeyValuePair<Keys, Ability>(Keys.D, GetAbility(AbilityName.HeartStrike));
            boundAbilities[7] = new KeyValuePair<Keys, Ability>(Keys.F, GetAbility(AbilityName.HeartStrike));


            for (int i = 0; i < Enum.GetNames(typeof(AbilityName)).Length; ++i)
            {
                abilityLearnedFlags[(AbilityName)i] = true;
            }


            #endregion
        }

        #region Action Sequences

        private string currentActionName = "";
        private Dictionary<string, List<Action>> actionSequences;
        protected bool needInterruptAction = true;
        protected bool canInterrupt = false;
        private Dictionary<string, Action> interruptActions;
        //the first item in the list is called immediately (when you call StartSequence)
        protected List<Action> currentSequence = null;
        protected int actionIndex = 0;

        protected void StartSequence(string name)
        {
            InterruptCurrentSequence();
            
            needInterruptAction = true;
            canInterrupt = false;
            currentSequence = actionSequences[name];
            actionIndex = 0;
            currentActionName = name;
            stateResetCounter = double.MaxValue;

            currentSequence[0]();
            millisActionCounter = 0;

        }
        private void InterruptCurrentSequence()
        {
            if (needInterruptAction && currentSequence != null && actionIndex < currentSequence.Count)
            {
                interruptActions[currentActionName]();
                actionIndex = int.MaxValue;
                currentSequence = null;
            }
        }

        private void SetUpActionSequences()
        {
            interruptActions = new Dictionary<string, Action>();
            actionSequences = new Dictionary<string, List<Action>>();
            //idles
            actionSequences.Add("idle", IdleActions());
            actionSequences.Add("fightingstance", FightingStanceActions());
            //primariy attacks
            actionSequences.Add("swing", SwingActions());
            actionSequences.Add("shoot", ShootActions());
            //actionSequences.Add("punch", PunchActions());
            //actionSequences.Add("magic", MagicActions());
            //loot
            actionSequences.Add("loot", LootActions());
            actionSequences.Add("loot_spin", LootSpinActions());
            actionSequences.Add("loot_smash", LootSmashActions());
            //abilities
            //ranged
            actionSequences.Add("snipe", SnipeActions());
            actionSequences.Add("omnishot", OmnishotActions());
            actionSequences.Add("buff1", Buff1Actions());

            //melee
            actionSequences.Add("flip", FlipActions());

            //magic


            //adding placeholder actions so that an exception is not thrown if this
            //is called for something that doesn't care about being interrupted
            //(and thus added nothing to the map)
            foreach (string s in actionSequences.Keys)
            {
                if (!interruptActions.ContainsKey(s))
                {
                    interruptActions.Add(s, () => { });
                }
            }
        }

        private List<Action> IdleActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_idle1", MixType.None);
                millisActionLength = rand.Next(2000, 4000);
            });
            sequence.Add(() =>
            {
                int randIdle = rand.Next(1, 7);
                string idleAni = "";
                if (randIdle < 4)
                {
                    idleAni = "k_idle2";
                }
                else
                {
                    switch (randIdle)
                    {
                        case 4:
                            idleAni = "k_idle3";
                            break;
                        case 5:
                            idleAni = "k_idle4";
                            break;
                        case 6:
                            idleAni = "k_idle5";
                            break;
                    }
                }

                PlayAnimation(idleAni, MixType.None);
                millisActionLength = animations.GetAniMillis(idleAni);
            });
            sequence.Add(() =>
            {
                StartSequence("idle");
            });

            return sequence;
        }
        private List<Action> FightingStanceActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_fighting_stance", MixType.None);
                millisActionLength = animations.GetAniMillis("k_fighting_stance");// *2;
            });
            sequence.Add(() =>
            {
                if (!inCombat)
                {
                    PlayAnimation("k_from_fighting_stance", MixType.None);
                    millisActionLength = animations.GetAniMillis("k_from_fighting_stance") - 20;
                }
                else
                {
                    StartSequence("k_fighting_stance");
                }
            });
            sequence.Add(() =>
            {
                StartSequence("idle");
            });

            return sequence;
        }
        private List<Action> ShootActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                //start playing shooting animation
                canInterrupt = true;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Attacking;
                stateResetCounter = 0;
                millisActionLength = 200;
            });
            sequence.Add(() =>
            {
                //attach arrow model to hand
                if (attachedArrow == null)
                {
                    attachedArrow = new AttachableModel(attacks.GetUnanimatedModel("Models\\Attachables\\arrow"), "Bone_001_L_004", 0, -MathHelper.PiOver2);
                }
                if (!attached.ContainsKey("handarrow"))
                {
                    attached.Add("handarrow", attachedArrow);
                }

                millisActionLength = animations.GetAniMillis("k_fire_arrow") / 2 - 200;
            });
            sequence.Add(() =>
            {
                //remove attached arrow, create arrow projectile, and finish animation
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateArrow(physicalData.Position + forward * 10, forward, 25, this, HasBuff(Buff.Homing), HasBuff(Buff.Penetrating), HasBuff(Buff.Leeching), HasBuff(Buff.SerratedBleeding));
                
                millisActionLength = animations.GetAniMillis("k_fire_arrow") - millisActionLength - 200;

                needInterruptAction = false;
            });

            sequence.Add(() =>
            {
                //done, go back to fighting stance
                StartSequence("fightingstance");
                attState = AttackState.None;
            });


            //if interrupted, this should still fire an arrow
            interruptActions.Add("shoot", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateArrow(physicalData.Position + forward * 10, forward, 25, this, false, false, false, false);
            });

            return sequence;
        }
        private List<Action> SwingActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                stateResetCounter = 0;
                canInterrupt = true;
                PlayAnimation("k_onehanded_swing" + aniSuffix, MixType.None);
                attState = AttackState.Attacking;
                stateResetCounter = 0;
                millisActionLength = animations.GetAniMillis("k_onehanded_swing") / 2;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 25, true, this);
                millisActionLength = animations.GetAniMillis("k_onehanded_swing") - millisActionLength;
                needInterruptAction = false;
            });
            sequence.Add(() =>
            {
                StartSequence("fightingstance");
                attState = AttackState.None;
            });

            //if interrupted, this should still create a melle attack
            interruptActions.Add("swing", () =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 25, true, this);
            });

            return sequence;
        }
        private List<Action> LootActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_loot", MixType.None);
                millisActionLength = animations.GetAniMillis("k_loot") - 20;

                if (attached[GearSlot.Righthand.ToString()] != null)
                {
                    attached[GearSlot.Righthand.ToString()].Draw = false;
                }
            });
            sequence.Add(() =>
            {
                StartSequence("loot_spin");
            });
            return sequence;
        }
        private List<Action> LootSpinActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_loot_spin", MixType.None);
                millisActionLength = animations.GetAniMillis("k_loot_spin");
            });
            sequence.Add(() =>
            {
                StartSequence("loot_spin");
            });
            return sequence;
        }
        private List<Action> LootSmashActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_loot_smash", MixType.MixInto);
                millisActionLength = animations.GetAniMillis("k_loot_smash");
            });
            sequence.Add(() =>
            {
                lootingSoul = null;
                looting = false;
                if (attached[GearSlot.Righthand.ToString()] != null)
                {
                    attached[GearSlot.Righthand.ToString()].Draw = true;
                }
                PlayAnimation("k_fighting_stance", MixType.None);
            });
            return sequence;
        }

        /*
         * Ranged abilities
         */
        private List<Action> SnipeActions()
        {
            //some of these actions are identical to "shoot", so just copying those Actions
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                canInterrupt = true;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Attacking;
                millisActionLength = 200;
                stateResetCounter = 0;
            });
            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateSnipe(physicalData.Position + forward * 10, forward, 100, this);

                millisActionLength = animations.GetAniMillis("k_fire_arrow") - millisActionLength - 200;

                needInterruptAction = false;
            });

            sequence.Add(actionSequences["shoot"][3]);

            interruptActions.Add("snipe", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateSnipe(physicalData.Position + forward * 10, forward, 100, this);
            });

            return sequence;
        }
        private List<Action> OmnishotActions()
        {//some of these actions are identical to "shoot", so just copying those Actions
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                canInterrupt = true;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Attacking;
                millisActionLength = 200;
                stateResetCounter = 0;
            });
            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateOmnishot(physicalData.Position + forward * 10, forward, 25, this, HasBuff(Buff.Homing), HasBuff(Buff.Penetrating), HasBuff(Buff.Leeching), HasBuff(Buff.SerratedBleeding));

                millisActionLength = animations.GetAniMillis("k_fire_arrow") - millisActionLength - 200;

                needInterruptAction = false;
            });

            sequence.Add(actionSequences["shoot"][3]);

            interruptActions.Add("omnishot", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateOmnishot(physicalData.Position + forward * 10, forward, 25, this, true, true, true, true);
            });

            return sequence;
        }
        private List<Action> Buff1Actions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                attacks.SpawnBuffParticles(physicalData.Position);
                AddBuff(Buff.AdrenalineRush, Entity);
                if (abilityLearnedFlags[AbilityName.Homing])
                {
                    AddBuff(Buff.Homing, Entity);
                }
                if (abilityLearnedFlags[AbilityName.Penetrating])
                {
                    AddBuff(Buff.Penetrating, Entity);
                }
            });

            return sequence;
        }

        /*
         * Melee abilities
         */
        private List<Action> FlipActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_flip", MixType.None);
                attState = AttackState.Attacking;
                millisActionLength = 400;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 50, true, this);
                millisActionLength = 500;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 50, true, this);
                millisActionLength = animations.GetAniMillis("k_flip") - 900;
            });
            sequence.Add(() =>
            {
                attState = AttackState.None;
                StartSequence("fightingstance");
            });

            return sequence;
        }


        /*
         * Magic Abilities
         */



        AttachableModel attachedArrow;
        protected void UpdateActionSequences()
        {
            if (millisActionCounter >= millisActionLength)
            {
                if (currentSequence != null && actionIndex < currentSequence.Count - 1)
                {
                    ++actionIndex;
                    currentSequence[actionIndex]();
                    millisActionCounter = 0;
                }
            }
        }
        #endregion

        #region animations
        protected enum AttackState
        {
            None,
            Attacking,
            CastingSpell,
        }

        protected double stateResetCounter = 0;
        //minimum milliseconds for an attack. decided in recalculatestats()
        protected double stateResetLength = 0;
        protected string aniSuffix = "";

        protected double millisActionCounter;
        protected double millisActionLength;

        protected AttackState attState = AttackState.None;

        protected string currentAniName;
        protected void PlayAnimationInterrupt(string animationName, MixType t)
        {
            InterruptCurrentSequence();
            PlayAnimation(animationName, t);

        }
        private void PlayAnimation(string animationName, MixType t)
        {
            animations.StartClip(animationName, t);
            currentAniName = animationName;
        }

        #endregion

        #region Damage
        //TODO: damage tracker and "in combat" status
        public override void HandleDamageDealt(int damageDealt)
        {

            millisCombatCounter = 0;
        }

        //TODO: particles for being hit / sound?
        protected override void TakeDamage(int damage, GameEntity from)
        {

        }
        #endregion

        #region helpers
        protected void CloseLoot()
        {
            StartSequence("loot_smash");
            if (lootingSoul != null)
            {
                lootingSoul.CloseLoot();
            }
        }
        protected void OpenLoot()
        {
            GameEntity possLoot = QueryNearEntity("loot", physicalData.Position + Vector3.Down * 18, 50);
            if (possLoot != null)
            {
                StartSequence("loot");
                lootingSoul = (possLoot.GetComponent(typeof(AIComponent)) as LootSoulController);
                lootingSoul.OpenLoot(physicalData.Position + Vector3.Down * 18, physicalData.Orientation);
                looting = true;

                groundTargetLocation = physicalData.Position;
            }
        }
        protected void SwapWeapons()
        {
            Equippable r = gear[GearSlot.Righthand];
            Equippable l = gear[GearSlot.Lefthand];

            if (attached.ContainsKey(GearSlot.Righthand.ToString()))
            {
                attached.Remove(GearSlot.Righthand.ToString());
            }
            if (attached.ContainsKey(GearSlot.Lefthand.ToString()))
            {
                attached.Remove(GearSlot.Lefthand.ToString());
            }

            if (r != null)
            {
                attached.Add(GearSlot.Lefthand.ToString(), new AttachableModel(r.GearModel, GearSlotToBoneName(GearSlot.Lefthand)));
            }

            if(l != null)
            {
                attached.Add(GearSlot.Righthand.ToString(), new AttachableModel(l.GearModel, GearSlotToBoneName(GearSlot.Righthand), MathHelper.Pi, 0));
            }


            gear[GearSlot.Righthand] = l;
            gear[GearSlot.Lefthand] = r;
        }
        public string GearSlotToBoneName(GearSlot s)
        {
            switch (s)
            {
                case GearSlot.Lefthand:
                    return "Bone_001_L_005";
                case GearSlot.Righthand:
                    return "Hand_R";
                default:
                    return "RootNode";
            }
        }
        /// <summary>
        /// rotates kazgar towards mouse
        /// </summary>
        /// <param name="move"></param>
        protected void UpdateRotation(Vector3 move)
        {
            //orientation
            float yaw = (float)Math.Atan(move.X / move.Z);
            if (move.Z < 0 && move.X >= 0
                || move.Z < 0 && move.X < 0)
            {
                yaw += MathHelper.Pi;
            }
            yaw += MathHelper.Pi;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
        }
        protected void ChangeVelocity(Vector3 newVel)
        {
            physicalData.LinearVelocity = newVel;

            // Here
            PlayerManager players = (PlayerManager)Game.Services.GetService(typeof(PlayerManager));

            //((MessageSender)Game.Services.GetService(typeof(MessageSender))).SendVelocityMessage(players.myId.id, newVel);
            
        }
        protected Matrix GetRotation()
        {
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            return new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
        }
        protected Vector3 GetForward()
        {
            return physicalData.OrientationMatrix.Forward;
        }
        protected GameEntity QueryNearEntity(string entityName, Vector3 position, float insideOfRadius)
        {
            Vector3 min = new Vector3(position.X - insideOfRadius, 0, position.Z - insideOfRadius);
            Vector3 max = new Vector3(position.X + insideOfRadius, 20, position.Z + insideOfRadius);
            BoundingBox b = new BoundingBox(min, max);

            var entries = Resources.GetBroadPhaseEntryList();
            (Game.Services.GetService(typeof(Space)) as Space).BroadPhase.QueryAccelerator.GetEntries(b, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == entityName)
                {
                    return other;
                }
            }
            return null;
        }
        protected bool RectContains(Rectangle rect, int x, int y)
        {
            if (x >= rect.X && x <= rect.X + rect.Width 
                && y >= rect.Y && y <= rect.Y + rect.Height)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Ability Definitions
        public enum AbilityName
        {
            Snipe,
            Omnishot,
            AdrenalineRush,
            Penetrating,
            Homing,

            HeartStrike,

            IceClawPrison,
        }
        protected Ability GetAbility(AbilityName ability)
        {
            switch (ability)
            {
                case AbilityName.Snipe:
                    return GetSnipe();
                case AbilityName.HeartStrike:
                    return GetHeartStrike();
                case AbilityName.IceClawPrison:
                    return GetIceClawPrison();
                case AbilityName.Omnishot:
                    return GetOmniShot();
                case AbilityName.AdrenalineRush:
                    return GetAdrenalineRush();
                default:
                    return null;
            }
        }

        protected Ability GetSnipe()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\LW"), 1, AttackType.Ranged, "snipe");
        }

        protected Ability GetHeartStrike()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\HS"), 6, AttackType.Ranged, "flip");
        }

        protected Ability GetIceClawPrison()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\whitePixel"), 6, AttackType.Ranged, "shoot");
        }

        protected Ability GetOmniShot()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I4"), 6, AttackType.Ranged, "omnishot");
        }

        protected Ability GetAdrenalineRush()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I4"), 6, AttackType.Ranged, "buff1");
        }
        #endregion


    }
}
