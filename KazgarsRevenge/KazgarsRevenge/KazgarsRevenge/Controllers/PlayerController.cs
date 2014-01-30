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
        protected const float stopRadius = 10;
        protected const float targetResetDistance = 1000;
        protected Vector3 mouseHoveredLocation = Vector3.Zero;
        protected Vector3 groundTargetLocation = Vector3.Zero;
        protected double millisRunningCounter = 2000;
        protected double millisRunTime = 2000;
        protected double millisCombatLength = 4000;
        protected double millisCombatCounter = 0;
        protected bool inCombat;
        protected bool looting = false;

        #endregion

        #region Abilities
        protected const float melleRange = 50;
        protected const float bowRange = 1000;
        protected Dictionary<string, Ability> allAbilities = new Dictionary<string, Ability>();
        protected KeyValuePair<Keys, Ability>[] boundAbilities = new KeyValuePair<Keys, Ability>[8];
        #endregion

        #region inventory and gear
        protected int gold = 0;
        protected const int NUM_LOOT_SHOWN = 4;
        protected int maxInventorySlots = 16;
        protected LootSoulController lootingSoul = null;
        protected Dictionary<GearSlot, Equippable> gear = new Dictionary<GearSlot, Equippable>();
        //inventory
        protected List<Item> inventory = new List<Item>();
        protected void EquipGear(Equippable equipMe, GearSlot slot)
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

                attached.Add(slot.ToString(), new AttachableModel(equipMe.GearModel, GearSlotToBoneName(slot), xRot));
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
        protected void RecalculateStats()
        {
            //add base stats, accounting for level
            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats[(StatType)i] = baseStats[i];
            }

            //add stats for each piece of gear
            foreach (KeyValuePair<GearSlot, Equippable> k in gear)
            {
                if (k.Value != null)
                {
                    AddStats(k.Value.StatEffects);
                }
            }
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
            aniDurations = new Dictionary<string, double>();
            aniDurations["shotRelease"] = animations.skinningDataValue.AnimationClips["k_fire_arrow"].Duration.TotalMilliseconds / 2;
            aniDurations["melleDamage"] = animations.skinningDataValue.AnimationClips["k_onehanded_swing"].Duration.TotalMilliseconds / 2;
            foreach (KeyValuePair<string, AnimationClip> k in animations.skinningDataValue.AnimationClips)
            {
                string key = k.Key;
                double length = k.Value.Duration.TotalMilliseconds;

                if (key == "k_fighting_stance")
                {
                    length *= 2;
                }

                aniDurations.Add(key, length - 20);
            }

            PlayAnimation("k_idle1", MixType.None);

            animationActions = new Dictionary<string, List<Action>>();
            List<Action> fireArrowSequence = new List<Action>();
            
            fireArrowSequence.Add(() =>
            {
                //start drawing; basically do nothing until next action

            });
            fireArrowSequence.Add(() =>
            {
                //attach arrow model to hand

            });
            fireArrowSequence.Add(() =>
            {
                //draw arrow back
            });
            fireArrowSequence.Add(() =>
            {
                //remove attached arrow, create arrow projectile, and finish animation
            });

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

            boundAbilities[0] = new KeyValuePair<Keys, Ability>(Keys.Q, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[1] = new KeyValuePair<Keys, Ability>(Keys.W, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[2] = new KeyValuePair<Keys, Ability>(Keys.E, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[3] = new KeyValuePair<Keys, Ability>(Keys.R, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[4] = new KeyValuePair<Keys, Ability>(Keys.A, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[5] = new KeyValuePair<Keys, Ability>(Keys.S, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[6] = new KeyValuePair<Keys, Ability>(Keys.D, attacks.GetAbility(AbilityName.Snipe));
            boundAbilities[7] = new KeyValuePair<Keys, Ability>(Keys.F, attacks.GetAbility(AbilityName.Snipe));
            #endregion
        }

        #region animations
        Dictionary<string, List<Action>> animationActions;
        protected enum AttackState
        {
            None,
            GrabbingArrow,
            DrawingString,
            LettingGo,
            InitialSwing,
            FinishSwing,
            CastingSpell,
        }

        Dictionary<string, double> aniDurations;

        protected double millisAniCounter;
        protected double millisAniDuration;

        protected AttackState attState = AttackState.None;
        protected double millisShotAniCounter;
        protected double millisShotArrowAttachLength = 200;
        protected double millisShotArrowAttachCounter;

        protected double millisMelleCounter;

        protected string currentAniName;
        protected void PlayAnimation(string animationName, MixType t)
        {
            animations.StartClip(animationName, t);
            if (animationName == "k_idle1")
            {
                millisAniDuration = rand.Next(2000, 4000);
            }
            else
            {
                millisAniDuration = aniDurations[animationName];
            }

            currentAniName = animationName;
            millisAniCounter = 0;
            millisShotAniCounter = 0;

            //TODO: send network signal

        }

        AttachableModel attachedArrow;
        protected void CheckAnimations()
        {
            //if the animation has played through its duration, do stuff
            //(mainly to handle idle animations and fighting stance to idle)
            if (millisAniCounter > millisAniDuration)
            {
                switch (currentAniName)
                {
                    case "k_loot":
                        PlayAnimation("k_loot_spin", MixType.None);
                        break;
                    case "k_loot_spin":
                        break;
                    case "k_loot_smash":
                        lootingSoul = null;
                        looting = false;
                        if (attached[GearSlot.Righthand.ToString()] != null)
                        {
                            attached[GearSlot.Righthand.ToString()].Draw = true;
                        }
                        PlayAnimation("k_fighting_stance", MixType.None);
                        break;
                    case "k_idle1":
                        int aniRand = rand.Next(1, 7);
                        if (aniRand < 4)
                        {
                            PlayAnimation("k_idle2", MixType.None);
                        }
                        else
                        {
                            switch (aniRand)
                            {
                                case 4:
                                    PlayAnimation("k_idle3", MixType.None);
                                    break;
                                case 5:
                                    PlayAnimation("k_idle4", MixType.None);
                                    break;
                                case 6:
                                    PlayAnimation("k_idle5", MixType.None);
                                    soundEffects.playScratch();
                                    break;
                            }
                        }
                        break;
                    default:
                        PlayAnimation("k_fighting_stance", MixType.None);
                        attState = AttackState.None;
                        break;
                    case "k_fighting_stance":
                        PlayAnimation("k_from_fighting_stance", MixType.None);
                        break;
                    case "k_from_fighting_stance":
                    case "k_idle2":
                    case "k_idle3":
                    case "k_idle4":
                    case "k_idle5":
                        PlayAnimation("k_idle1", MixType.None);
                        break;
                    case "k_run":
                        break;
                }
                millisAniCounter = 0;
            }

            //shooting animation / arrow attachment / projectile creation
            if (currentAniName == "k_fire_arrow")
            {
                if (attState == AttackState.GrabbingArrow
                    && millisShotArrowAttachCounter >= millisShotArrowAttachLength)
                {
                    if (attachedArrow == null)
                    {
                        attachedArrow = new AttachableModel(attacks.GetUnanimatedModel("Models\\Attachables\\arrow"), "Bone_001_R_004", 0);
                    }
                    if (!attached.ContainsKey("arrow"))
                    {
                        attached.Add("arrow", attachedArrow);
                    }
                    attState = AttackState.DrawingString;
                    millisShotArrowAttachCounter = 0;
                }
                else if (attState == AttackState.DrawingString && millisShotAniCounter >= aniDurations["shotRelease"])
                {
                    Vector3 forward = GetForward();
                    attached.Remove("arrow");
                    attacks.CreateArrow(physicalData.Position + forward * 10, forward, 25, this);
                    attState = AttackState.LettingGo;
                    millisShotAniCounter = 0;
                }
                else if (attState == AttackState.LettingGo && millisShotAniCounter >= aniDurations["shotRelease"] * stats[StatType.AttackSpeed])
                {
                    attState = AttackState.None;
                    millisShotAniCounter = 0;
                }
            }

            //swinging animation
            if (currentAniName == "k_onehanded_swing" || currentAniName == "k_flip")
            {
                if (attState == AttackState.InitialSwing && millisMelleCounter >= aniDurations["melleDamage"])
                {
                    Vector3 forward = GetForward();
                    attacks.CreateMelleAttack(physicalData.Position + forward * 35, 25, true, this);
                    attState = AttackState.FinishSwing;
                    millisMelleCounter = 0;
                }
            }
        }
        #endregion


        #region Damage
        //TODO: damage tracker and "in combat" status
        public override void HandleDamageDealt(int damageDealt)
        {
            
        }

        //TODO: particles for being hit / sound?
        protected override void TakeDamage(int damage, GameEntity from)
        {

        }
        #endregion

        #region helpers
        protected void CloseLoot()
        {

            PlayAnimation("k_loot_smash", MixType.MixInto);
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
                PlayAnimation("k_loot", MixType.None);
                if (attached[GearSlot.Righthand.ToString()] != null)
                {
                    attached[GearSlot.Righthand.ToString()].Draw = false;
                }
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
                attached.Add(GearSlot.Lefthand.ToString(), new AttachableModel(r.GearModel, GearSlotToBoneName(GearSlot.Lefthand), 0));
            }

            if(l != null)
            {
                attached.Add(GearSlot.Righthand.ToString(), new AttachableModel(l.GearModel, GearSlotToBoneName(GearSlot.Righthand), MathHelper.Pi));
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
            ((MessageSender)Game.Services.GetService(typeof(MessageSender))).SendVelocityMessage(players.myId.id, newVel);
            
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


    }
}
