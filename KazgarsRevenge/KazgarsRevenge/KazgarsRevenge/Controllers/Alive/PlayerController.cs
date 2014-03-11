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
        Melee,
        Ranged,
        Magic,
    }
    public enum GearSlot
    {
        None,
        Head,
        Chest,
        Codpiece,
        Legs,
        Feet,
        Shoulders,
        Wrist,
        Righthand,
        Lefthand,
        Bling,
    }
    
    public abstract class PlayerController : AliveComponent
    {

        #region member variables
        //services
        protected Space physics;
        protected CameraComponent camera;
        protected SoundEffectLibrary soundEffects;
        protected LootManager lewtz;
        protected NetworkMessageManager nmm;

        //data
        protected Dictionary<string, AttachableModel> attached;
        protected Dictionary<string, Model> syncedModels;


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
        protected int currentPower = 0;
        protected int totalPower = 100;
        protected int spentTalentPoints = 0;
        protected int totalTalentPoints = 100;
        protected const float melleRange = 50;
        protected const float bowRange = 1000;
        protected Dictionary<string, Ability> allAbilities = new Dictionary<string, Ability>();
        //array of key-ability pairs
        protected KeyValuePair<Keys, Ability>[] boundAbilities = new KeyValuePair<Keys, Ability>[8];
        protected KeyValuePair<ButtonState, Ability>[] mouseBoundAbility = new KeyValuePair<ButtonState, Ability>[1];
        protected Dictionary<AbilityName, bool> abilityLearnedFlags = new Dictionary<AbilityName, bool>();
        #endregion

        #region inventory and gear
        protected int gold = 0;
        protected const int NUM_LOOT_SHOWN = 4;
        protected int maxInventorySlots = 16;
        protected LootSoulController lootingSoul = null;
        protected Dictionary<GearSlot, Equippable> gear = new Dictionary<GearSlot, Equippable>();
        //inventory
        protected Item[] inventory = new Item[16];
        //#Nate
        protected virtual void EquipGear(Equippable equipMe, GearSlot slot)
        {
            float xRot = 0;
            bool weapon = false;
            //if the player is trying to equip a two-handed weapon to the offhand, unequip both current weapons and equip it to the main hand
            Weapon possWep = equipMe as Weapon;
            if (possWep != null)
            {
                Weapon possRightHand = gear[GearSlot.Righthand] as Weapon;

                if (possRightHand != null && possRightHand.TwoHanded)
                {
                    UnequipGear(GearSlot.Righthand);
                }

                weapon = true;
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

                if (weapon)
                {
                    attached.Add(slot.ToString(), new AttachableModel(equipMe.GearModel, GearSlotToBoneName(slot), xRot, 0));
                }
                else
                {
                    syncedModels.Add(slot.ToString(), equipMe.GearModel);
                }
            }
        }
        /// <summary>
        /// tries to put equipped item into inventory. If there was no inventory space, returns false.
        /// </summary>
        protected bool UnequipGear(GearSlot slot, int inventoryPos)
        {
            Equippable toRemove = gear[slot];
            if (toRemove == null)
            {
                //if there was nothing in there to start with, return true
                return true;
            }
            if (AddToInventory(toRemove, inventoryPos))
            {
                gear[slot] = null;
                if (toRemove is Weapon)
                {
                    attached.Remove(slot.ToString());
                }
                else
                {
                    syncedModels.Remove(slot.ToString());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool UnequipGear(GearSlot slot)
        {
            Equippable toRemove = gear[slot];
            if (toRemove == null)
            {
                //if there was nothing in there to start with, return true
                return true;
            }
            if (AddToInventory(toRemove))
            {
                gear[slot] = null;
                if (toRemove is Weapon)
                {
                    attached.Remove(slot.ToString());
                }
                else
                {
                    syncedModels.Remove(slot.ToString());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddToInventory(Item toAdd, int inventoryIndex)
        {
            if (toAdd == null)
            {
                return true;
            }

            //check if full
            if (inventory[inventoryIndex] != null)
            {
                return false;
            }

            if (toAdd.Name == "gold")
            {
                gold += toAdd.Quantity;
                return true;
            }

            inventory[inventoryIndex] = toAdd;

            return true;
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

            //check if full
            bool full = true;
            for (int i = 0; i < inventory.Length; ++i)
            {
                if (inventory[i] == null)
                {
                    full = false;
                    break;
                }
            }
            if (full)
            {
                return false;
            }

            if (toAdd.Name == "gold")
            {
                gold += toAdd.Quantity;
                return true;
            }

            for (int i = 0; i < inventory.Length; ++i)
            {
                if (inventory[i] == null)
                {
                    inventory[i] = toAdd;
                    break;
                }
            }

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
                    AddGearStats(k.Value);
                }
            }

            stateResetLength = 3000 * (1 - Math.Min(.95f, GetStat(StatType.AttackSpeed)));
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

        public PlayerController(KazgarsRevengeGame game, GameEntity entity, Account account)
            : base(game, entity, account.CharacterLevel)
        {
            //shared data
            this.attached = Entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;
            this.syncedModels = Entity.GetSharedData(typeof(Dictionary<string, Model>)) as Dictionary<string, Model>;

            InitGeneralFields();

            InitNewPlayer();

            //adding sword and bow for demo
            EquipGear(lewtz.GenerateSword(), GearSlot.Righthand);
            EquipGear(lewtz.GenerateBow(), GearSlot.Lefthand);

            EquipGear(lewtz.GetBoots(), GearSlot.Feet);
            EquipGear(lewtz.GetChest(), GearSlot.Chest);
            EquipGear(lewtz.GetHelm(), GearSlot.Head);
            EquipGear(lewtz.GetLegs(), GearSlot.Legs);
            EquipGear(lewtz.GetShoulders(), GearSlot.Shoulders);
            EquipGear(lewtz.GetWrist(), GearSlot.Wrist);
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
            soundEffects = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;
            lewtz = Game.Services.GetService(typeof(LootManager)) as LootManager;
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
            RecalculateStats();

            for (int i = 0; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
            {
                gear.Add((GearSlot)i, null);
            }
            #endregion

            #region ability initialization
            //create initial abilities

            boundAbilities[0] = new KeyValuePair<Keys, Ability>(Keys.Q, GetAbility(AbilityName.TarBomb));
            boundAbilities[1] = new KeyValuePair<Keys, Ability>(Keys.W, GetAbility(AbilityName.GrapplingHook));
            boundAbilities[2] = new KeyValuePair<Keys, Ability>(Keys.E, GetAbility(AbilityName.Snipe));
            boundAbilities[3] = new KeyValuePair<Keys, Ability>(Keys.R, GetAbility(AbilityName.AdrenalineRush));
            boundAbilities[4] = new KeyValuePair<Keys, Ability>(Keys.A, GetAbility(AbilityName.MoltenBolt));
            boundAbilities[5] = new KeyValuePair<Keys, Ability>(Keys.S, GetAbility(AbilityName.LooseCannon));
            boundAbilities[6] = new KeyValuePair<Keys, Ability>(Keys.D, GetAbility(AbilityName.FlashBomb));
            boundAbilities[7] = new KeyValuePair<Keys, Ability>(Keys.F, GetAbility(AbilityName.ChainSpear));

            mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(ButtonState.Pressed, GetAbility(AbilityName.Omnishot));

            for (int i = 0; i < Enum.GetNames(typeof(AbilityName)).Length; ++i)
            {
                abilityLearnedFlags[(AbilityName)i] = false;
            }

            #endregion
        }

        #region Action Sequences

        protected bool targetingGroundLocation = false;
        protected Vector3 groundAbilityTarget = Vector3.Zero;
        protected float targetedGroundSize = 0;
        protected Ability lastUsedAbility = null;

        protected string currentActionName = "";
        private Dictionary<string, List<Action>> actionSequences;
        protected bool needInterruptAction = true;
        protected bool canInterrupt = false;
        private Dictionary<string, Action> interruptActions;

        //the first item in the list is called immediately (when StartSequence is called)
        protected List<Action> currentSequence = null;
        protected int actionIndex = 0;

        protected float GetPercentCharged()
        {
            if (attState != AttackState.Charging)
            {
                return -1;
            }
            else
            {
                return (float)(millisActionCounter / millisActionLength);
            }
        }
        //to be used for charging/casting abilities; when you
        //interrupt them, you don't want to use it
        //and you want to go back to fighting stance
        protected void CancelFinishSequence()
        {
            animations.UnpauseAnimation();
            currentSequence = null;
            currentActionName = "";
            abilityFinishedAction();
        }
        protected void StartAbilitySequence(Ability ability)
        {
            if (ability == null)
            {
                lastUsedAbility = null;
                targetingGroundLocation = false;
            }
            if (ability.AbilityType == AbilityType.GroundTarget)
            {
                StartGroundTargetSequence(ability.ActionName);
            }
            else
            {
                StartSequence(ability.ActionName);
            }
            lastUsedAbility = ability;
        }

        protected void StartSequence(string name)
        {
            if (!name.Equals("PlaceHolder"))
            {
                InterruptReset(name);

                currentSequence[0]();
                millisActionCounter = 0;

                if (name != "fightingstance" && name != "idle")
                {
                    groundTargetLocation = physicalData.Position;
                    targetingGroundLocation = false;
                    lastUsedAbility = null;
                }
            }
        }

        private void StartGroundTargetSequence(string name)
        {
            InterruptReset(name);

            currentSequence[0]();
            millisActionCounter = 0;
        }

        //for buffs, so as to not disrupt running / whatever else
        protected void InterruptReset(string name)
        {
            if (!name.Equals("PlaceHolder"))
            {
                InterruptCurrentSequence();

                needInterruptAction = true;
                canInterrupt = false;
                currentSequence = actionSequences[name];        //#jared TODO punch
                actionIndex = 0;
                currentActionName = name;
                stateResetCounter = double.MaxValue;
            }
        }
        //calls the interrupt handler for the current sequence
        protected void InterruptCurrentSequence()
        {
            if (needInterruptAction && currentSequence != null && actionIndex < currentSequence.Count)
            {
                interruptActions[currentActionName]();
                actionIndex = int.MaxValue;
                currentSequence = null;
            }
        }
        private void TriggerCooldown()
        {
            for (int i = 0; i < boundAbilities.Length; ++i)
            {
                if (boundAbilities[i].Value.ActionName == currentActionName)
                {
                    boundAbilities[i].Value.Use();
                }
            }
        }
        private void SetUpHelperActions()
        {
            abilityFinishedAction = () =>
            {
                //done, go back to fighting stance
                StartSequence("fightingstance");
                attState = AttackState.None;
            };
        }
        private void SetUpActionSequences()
        {
            SetUpHelperActions();

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
            actionSequences.Add("buffrush", BuffRushActions());
            actionSequences.Add("loosecannon", LooseCannonActions());
            actionSequences.Add("makeitrain", MakeItRainActions());
            actionSequences.Add("flashbomb", FlashBombActions());
            actionSequences.Add("tarbomb", TarBombActions());
            actionSequences.Add("grapplinghook", GrapplingHookActions());
            actionSequences.Add("chainspear", ChainSpearActions());
            actionSequences.Add("moltenbolt", MoltenBoltActions());

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
                    millisActionLength = animations.GetAniMillis("k_from_fighting_stance") - 40;
                }
                else
                {
                    StartSequence("fightingstance");
                }
            });
            sequence.Add(() =>
            {
                StartSequence("idle");
            });

            return sequence;
        }
        private const int arrowDrawMillis = 200;
        private const int arrowReleaseMillis = 300;
        private List<Action> ShootActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                //start playing shooting animation
                canInterrupt = true;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = arrowDrawMillis;
            });
            sequence.Add(() =>
            {
                //attach arrow model to hand
                if (attachedArrow == null)
                {
                    attachedArrow = new AttachableModel(attacks.GetUnanimatedModel("Models\\Attachables\\arrow"), "Bone_001_L_005", 0, -MathHelper.PiOver2);
                }
                if (!attached.ContainsKey("handarrow"))
                {
                    attached.Add("handarrow", attachedArrow);
                }

                millisActionLength = arrowReleaseMillis;
            });
            sequence.Add(() =>
            {
                //remove attached arrow, create arrow projectile, and finish animation
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateArrow(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Agility), this as AliveComponent, activeBuffs.ContainsKey(Buff.Homing), abilityLearnedFlags[AbilityName.Penetrating], abilityLearnedFlags[AbilityName.Leeching], activeBuffs.ContainsKey(Buff.SerratedBleeding));
                
                millisActionLength = 1000 - arrowReleaseMillis - arrowDrawMillis;

                needInterruptAction = false;
            });
            sequence.Add(abilityFinishedAction);


            //if interrupted, this should still fire an arrow
            interruptActions.Add("shoot", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateArrow(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Agility), this as AliveComponent, activeBuffs.ContainsKey(Buff.Homing), abilityLearnedFlags[AbilityName.Penetrating], abilityLearnedFlags[AbilityName.Leeching], activeBuffs.ContainsKey(Buff.SerratedBleeding));
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
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = animations.GetAniMillis("k_onehanded_swing") / 2;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 25, true, this as AliveComponent);
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
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 25, true, this as AliveComponent);
            });

            return sequence;
        }
        private List<Action> LootActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_loot", MixType.PauseAtEnd);
                millisActionLength = animations.GetAniMillis("k_loot");

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
                PlayAnimation("k_loot_spin", MixType.PauseAtEnd);
                millisActionLength = animations.GetAniMillis("k_loot_spin");
                if (lootingSoul != null)
                {
                    lootingSoul.StartSpin();
                }
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
                attState = AttackState.Locked;
                millisActionLength = arrowDrawMillis;
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
                int damage = GeneratePrimaryDamage(StatType.Agility);
                if (abilityLearnedFlags[AbilityName.Headshot] && rand.Next(0, 101) < 25)
                {
                    damage *= 2;
                }
                attacks.CreateSnipe(physicalData.Position + forward * 10, forward, damage, this as AliveComponent, abilityLearnedFlags[AbilityName.MagneticImplant]);

                millisActionLength = 1000 - arrowDrawMillis - arrowReleaseMillis;

                needInterruptAction = false;
            });

            sequence.Add(abilityFinishedAction);

            interruptActions.Add("snipe", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                int damage = GeneratePrimaryDamage(StatType.Agility);
                if (abilityLearnedFlags[AbilityName.Headshot] && rand.Next(0, 101) < 25)
                {
                    damage *= 2;
                }
                attacks.CreateSnipe(physicalData.Position + forward * 10, forward, damage, this as AliveComponent, abilityLearnedFlags[AbilityName.MagneticImplant]);
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
                attState = AttackState.Locked;
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
                attacks.CreateOmnishot(physicalData.Position + forward * 10, forward, 25, this as AliveComponent, activeBuffs.ContainsKey(Buff.Homing), abilityLearnedFlags[AbilityName.Penetrating], abilityLearnedFlags[AbilityName.Leeching], activeBuffs.ContainsKey(Buff.SerratedBleeding));

                millisActionLength = animations.GetAniMillis("k_fire_arrow") - millisActionLength - 200;

                needInterruptAction = false;
            });

            sequence.Add(abilityFinishedAction);

            interruptActions.Add("omnishot", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateOmnishot(physicalData.Position + forward * 10, forward, 25, this as AliveComponent, true, true, true, true);
            });

            return sequence;
        }
        private List<Action> BuffRushActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                attacks.SpawnBuffParticles(physicalData.Position);
                AddBuff(Buff.AdrenalineRush, Entity);
                if (abilityLearnedFlags[AbilityName.Serrated])
                {
                    AddBuff(Buff.SerratedBleeding, Entity);
                }
                if (abilityLearnedFlags[AbilityName.Homing])
                {
                    AddBuff(Buff.Homing, Entity);
                }
                if (currentAniName == "k_fighting_stance" || currentAniName == "k_from_fighting_stance")
                {
                    InterruptReset("fightingstance");
                    millisActionCounter = aniCounter;
                    millisActionLength = aniLength;
                }

                attState = AttackState.None;
            });

            return sequence;
        }
        private List<Action> LooseCannonActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(actionSequences["snipe"][0]);


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

                millisActionLength = arrowReleaseMillis;
            });

            sequence.Add(() =>
            {
                attState = AttackState.Charging;
                animations.PauseAnimation();
                millisActionLength = 2000;
            });

            sequence.Add(() =>
            {
                animations.UnpauseAnimation();
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                int damage = GeneratePrimaryDamage(StatType.Agility) * 5;
                attacks.CreateLooseCannon(physicalData.Position + forward * 10, forward, damage, this as AliveComponent, 1);

                millisActionLength = 1000 - arrowDrawMillis - arrowReleaseMillis;
                needInterruptAction = false;
                attState = AttackState.None;

                lastUsedAbility.Use();
            });

            sequence.Add(abilityFinishedAction);

            interruptActions.Add("loosecannon", () =>
            {
                animations.UnpauseAnimation();
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                float percentCharged = (float)(millisActionCounter + 200) / 4200;
                int damage = GeneratePrimaryDamage(StatType.Agility);
                damage += (int)(4 * percentCharged);
                attacks.CreateLooseCannon(physicalData.Position + forward * 10, forward, damage, this as AliveComponent, percentCharged);

                
            });

            return sequence;
        }
        private const float makeItRainSize = 50;
        private List<Action> MakeItRainActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (!targetingGroundLocation)
                {
                    //set targeting to true, set target size, go back to no attack state and fighting stance
                    targetingGroundLocation = true;
                    targetedGroundSize = makeItRainSize;
                    if (abilityLearnedFlags[AbilityName.StrongWinds])
                    {
                        targetedGroundSize *= 5;
                    }
                    //skip other actions
                    actionIndex = 10;

                    if (currentAniName == "k_fighting_stance" || currentAniName == "k_from_fighting_stance")
                    {
                        InterruptReset("fightingstance");
                        millisActionCounter = aniCounter;
                        millisActionLength = aniLength;
                    }

                    attState = AttackState.None;
                }
                else
                {
                    targetingGroundLocation = false;
                    //if button has been pushed once and we're targeting already, use ability at mouse location
                    actionSequences["snipe"][0]();
                    groundAbilityTarget = mouseHoveredLocation;
                    groundTargetLocation = physicalData.Position;
                }
            });

            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                //looks like kazgar releases arrow
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }

                int damage = GeneratePrimaryDamage(StatType.Agility);
                if (abilityLearnedFlags[AbilityName.MakeItHail])
                {
                    damage = (int)(damage * 1.5f);
                }

                float radius = makeItRainSize;
                if (abilityLearnedFlags[AbilityName.StrongWinds])
                {
                    radius *= 5;
                }
                attacks.CreateMakeItRain(groundAbilityTarget, damage, radius, this as AliveComponent);

                millisActionLength = 1000 - arrowReleaseMillis - arrowDrawMillis;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private const float bombSize = 50;
        private List<Action> FlashBombActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (!targetingGroundLocation)
                {
                    targetingGroundLocation = true;
                    targetedGroundSize = bombSize;
                    if (abilityLearnedFlags[AbilityName.BiggerBombs])
                    {
                        targetedGroundSize *= 2.5f;
                    }
                    actionIndex = 10;

                    if (currentAniName == "k_fighting_stance" || currentAniName == "k_from_fighting_stance")
                    {
                        InterruptReset("fightingstance");
                        millisActionCounter = aniCounter;
                        millisActionLength = aniLength;
                    }

                    attState = AttackState.None;
                }
                else
                {
                    targetingGroundLocation = false;
                    actionSequences["snipe"][0]();
                    groundAbilityTarget = mouseHoveredLocation;
                    groundTargetLocation = physicalData.Position;
                }
            });

            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                //looks like kazgar releases arrow
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }

                float radius = bombSize;
                if (abilityLearnedFlags[AbilityName.BiggerBombs])
                {
                    radius *= 2.5f;
                }
                attacks.CreateFlashBomb(physicalData.Position, groundAbilityTarget, radius, this as AliveComponent);

                millisActionLength = 1000 - arrowReleaseMillis - arrowDrawMillis;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> TarBombActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (!targetingGroundLocation)
                {
                    targetingGroundLocation = true;
                    targetedGroundSize = bombSize;
                    if (abilityLearnedFlags[AbilityName.BiggerBombs])
                    {
                        targetedGroundSize *= 2.5f;
                    }
                    actionIndex = 10;

                    if (currentAniName == "k_fighting_stance" || currentAniName == "k_from_fighting_stance")
                    {
                        InterruptReset("fightingstance");
                        millisActionCounter = aniCounter;
                        millisActionLength = aniLength;
                    }

                    attState = AttackState.None;
                }
                else
                {
                    targetingGroundLocation = false;
                    actionSequences["snipe"][0]();
                    groundAbilityTarget = mouseHoveredLocation;
                    groundTargetLocation = physicalData.Position;
                }
            });

            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                //looks like kazgar releases arrow
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }

                float radius = bombSize;
                if (abilityLearnedFlags[AbilityName.BiggerBombs])
                {
                    radius *= 2.5f;
                }
                attacks.CreateTarBomb(physicalData.Position, groundAbilityTarget, radius, this as AliveComponent);

                millisActionLength = 1000 - arrowReleaseMillis - arrowDrawMillis;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private const float grapplingHookSpeed = 400;
        private List<Action> GrapplingHookActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                canInterrupt = false;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                millisActionLength = arrowDrawMillis;
                stateResetCounter = 0;
            });
            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                float speed = grapplingHookSpeed;
                if (abilityLearnedFlags[AbilityName.SpeedyGrapple])
                {
                    speed *= 2.5f;
                }

                Vector3 forward = GetForward();
                attacks.CreateGrapplingHook(physicalData.Position + forward * 10, forward, this as AliveComponent, speed);

                millisActionLength = 1000 - arrowDrawMillis - arrowReleaseMillis;

            });

            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> ChainSpearActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                canInterrupt = false;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                millisActionLength = arrowDrawMillis;
                stateResetCounter = 0;
            });
            sequence.Add(actionSequences["shoot"][1]);

            sequence.Add(() =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                float speed = grapplingHookSpeed;
                if (abilityLearnedFlags[AbilityName.SpeedyGrapple])
                {
                    speed *= 2.5f;
                }

                Vector3 forward = GetForward();
                attacks.CreateChainSpear(physicalData.Position + forward * 10, forward, this as AliveComponent, speed, abilityLearnedFlags[AbilityName.ForcefulThrow]);

                millisActionLength = 1000 - arrowDrawMillis - arrowReleaseMillis;

            });

            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> MoltenBoltActions()
        {
            //some of these actions are identical to "shoot", so just copying those Actions
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                canInterrupt = true;
                PlayAnimation("k_fire_arrow" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                millisActionLength = arrowDrawMillis;
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
                attacks.CreateMoltenBolt(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Agility), this as AliveComponent);

                millisActionLength = 1000 - arrowDrawMillis - arrowReleaseMillis;

                needInterruptAction = false;
            });

            sequence.Add(abilityFinishedAction);

            interruptActions.Add("moltenbolt", () =>
            {
                if (attached.ContainsKey("handarrow"))
                {
                    attached.Remove("handarrow");
                }
                Vector3 forward = GetForward();
                attacks.CreateMoltenBolt(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Agility), this as AliveComponent);
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
                attState = AttackState.Locked;
                millisActionLength = 400;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 50, true, this as AliveComponent);
                millisActionLength = 500;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, 50, true, this as AliveComponent);
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



        int fightingStanceLoops = 0;
        AttachableModel attachedArrow;
        protected void UpdateActionSequences(double elapsed)
        {
            millisActionCounter += elapsed;
            if (millisActionCounter >= millisActionLength)
            {
                if (currentSequence != null && actionIndex < currentSequence.Count - 1)
                {
                    ++actionIndex;
                    currentSequence[actionIndex]();
                    millisActionCounter = 0;
                }
            }

            aniCounter += elapsed;
            if (aniCounter >= aniLength)
            {
                if (currentAniName == "k_fighting_stance")
                {
                    ++fightingStanceLoops;
                    if (fightingStanceLoops > 3)
                    {
                        StartSequence("fightingstance");
                        fightingStanceLoops = 0;
                    }
                }
                else
                {
                    fightingStanceLoops = 0;
                }
                if (currentAniName == "k_from_fighting_stance")
                {
                    StartSequence("idle");
                }
            }
        }



        
        Action abilityFinishedAction;
        #endregion

        #region animations
        protected enum AttackState
        {
            None,
            Locked,
            Charging,
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
        double aniCounter = 0;
        double aniLength = 0;
        private void PlayAnimation(string animationName, MixType t)
        {
            animations.StartClip(animationName, t);
            currentAniName = animationName;
            aniCounter = 0;
            aniLength = animations.GetAniMillis(animationName);
        }

        #endregion

        #region Damage

        //TODO: damage tracker and "in combat" status  #resolved?
        public override void HandleDamageDealt(int damageDealt)
        {
            inCombat = true;
            millisCombatCounter = 0;
        }

        //TODO: particles for being hit / sound?
        protected override void TakeDamage(int damage, GameEntity from)
        {
            inCombat = true;
            millisCombatCounter = 0;
            if (currentActionName == "idle")
            {
                StartSequence("fightingstance");
            }
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
                    return "Armature";
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
            if (!pulling)
            {
                physicalData.LinearVelocity = newVel;
            }

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
        public override void StopPull()
        {
            groundTargetLocation = physicalData.Position;
            base.StopPull();
        }
        #endregion

        #region Ability Definitions
        protected Ability GetAbility(AbilityName ability)
        {
            switch (ability)
            {
                case AbilityName.None:
                    return GetNone();
                case AbilityName.Snipe:
                    return GetSnipe();
                case AbilityName.Garrote:
                    return GetHeartStrike();
                case AbilityName.IceClawPrison:
                    return GetIceClawPrison();
                case AbilityName.Omnishot:
                    return GetOmniShot();
                case AbilityName.AdrenalineRush:
                    return GetAdrenalineRush();
                case AbilityName.Leeching:
                    return GetLeechingArrows();
                case AbilityName.LooseCannon:
                    return GetLooseCannon();
                case AbilityName.MakeItRain:
                    return GetMakeItRain();
                case AbilityName.FlashBomb:
                    return GetFlashBomb();
                case AbilityName.TarBomb:
                    return GetTarBomb();
                case AbilityName.GrapplingHook:
                    return GetGrapplingHook();
                case AbilityName.ChainSpear:
                    return GetChainSpear();
                case AbilityName.MoltenBolt:
                    return GetMoltenBolt();
                default:
                    return GetNone();
                    //throw new Exception("That ability hasn't been implemented.");
            }
        }

        protected Ability GetPotionAbility(AbilityName potionType)
        {
            switch (potionType)
            {
                case AbilityName.HealthPotion:
                    return GetHealthPotion();
                default:
                    throw new Exception("That ability hasn't been implemented.");
            }
        }

        //Placeholder Ability
        protected Ability GetNone()
        {
            return new Ability(AbilityName.None, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Place_Holder), 0, AttackType.Ranged, "PlaceHolder", AbilityType.Instant);
        }

        protected Ability GetSnipe()
        {
            return new Ability(AbilityName.Snipe, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.SNIPE), 1000, AttackType.Ranged, "snipe", AbilityType.Instant);
        }
        protected Ability GetHeartStrike()
        {
            return new Ability(AbilityName.Garrote, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.HEART_STRIKE), 6000, AttackType.Ranged, "flip", AbilityType.Instant);
        }
        protected Ability GetIceClawPrison()
        {
            return new Ability(AbilityName.IceClawPrison, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.ICE_CLAW_PRI), 6000, AttackType.Ranged, "shoot", AbilityType.Instant);
        }
        protected Ability GetOmniShot()
        {
            return new Ability(AbilityName.Omnishot, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.OMNI_SHOT), 6000, AttackType.Ranged, "omnishot", AbilityType.Instant);
        }
        protected Ability GetAdrenalineRush()
        {
            return new Ability(AbilityName.AdrenalineRush, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.ADREN_RUSH), 6000, AttackType.Ranged, "buffrush", AbilityType.Instant);
        }
        protected Ability GetLeechingArrows()
        {
            return new Ability(AbilityName.Leeching, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.LEECH_ARROWS), 6000, AttackType.Ranged, "buffleech", AbilityType.Instant);
        }
        protected Ability GetLooseCannon()
        {
            return new Ability(AbilityName.LooseCannon, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.LOOSE_CANNON), 2000, AttackType.Ranged, "loosecannon", AbilityType.Charge);
        }
        protected Ability GetMakeItRain()
        {
            return new Ability(AbilityName.MakeItRain, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.MAKE_IT_RAIN), 2000, AttackType.Ranged, "makeitrain", AbilityType.GroundTarget);
        }
        protected Ability GetFlashBomb()
        {
            return new Ability(AbilityName.FlashBomb, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.FLASH_BOMB), 2000, AttackType.Ranged, "flashbomb", AbilityType.GroundTarget);
        }
        protected Ability GetTarBomb()
        {
            return new Ability(AbilityName.TarBomb, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.TAR_BOMB), 2000, AttackType.Ranged, "tarbomb", AbilityType.GroundTarget);
        }
        protected Ability GetGrapplingHook()
        {
            return new Ability(AbilityName.GrapplingHook, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.GRAP_HOOK), 2000, AttackType.Ranged, "grapplinghook", AbilityType.Instant);
        }
        protected Ability GetChainSpear()
        {
            return new Ability(AbilityName.ChainSpear, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.CHAIN_SPEAR), 2000, AttackType.Ranged, "chainspear", AbilityType.Instant);
        }
        protected Ability GetMoltenBolt()
        {
            return new Ability(AbilityName.MoltenBolt, 1, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.MOLT_BOLT), 2000, AttackType.Ranged, "moltenbolt", AbilityType.Instant);
        }
        #endregion

        #region Potion Definitions
        protected Ability GetHealthPotion()
        {
            return new Ability(AbilityName.HealthPotion, 0, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.GRAP_HOOK), 2000, AttackType.None, "grapplinghook", AbilityType.Instant);
        }
        #endregion


    }
    public enum AbilityName
    {
        None,


        Snipe,
        AdrenalineRush,
        Penetrating,
        Homing,
        Leeching,
        Serrated,
        Headshot,
        MagneticImplant,
        LooseCannon,
        MakeItRain,
        MakeItHail,
        StrongWinds,
        GrapplingHook,
        SpeedyGrapple,
        TarBomb,
        MoltenBolt,
        FlashBomb,
        BiggerBombs,
        Omnishot,



        Garrote,
        ChainSpear,
        ForcefulThrow,

        IceClawPrison,

        HealthPotion,
        PotionOfLuck,
        InivisibilityPotion,

        //#Jared TODO Nate added these, need support for them
        Elusiveness,
        Tumble,

    }
}
