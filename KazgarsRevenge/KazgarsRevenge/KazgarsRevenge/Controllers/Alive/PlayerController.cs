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
        protected double millisCombatLength = 500;
        protected double millisCombatCounter = 0;
        protected bool inCombat = false;
        protected bool looting = false;

        #endregion

        #region Abilities


        protected int currentPower = 0;
        protected const int maxPower = 30;
        public override void AddPower(int power)
        {
            currentPower = Math.Min(currentPower + power, maxPower);
        }
        public void UsePower(int power)
        {
            currentPower = Math.Max(0, currentPower - power);
        }
        
        protected int spentTalentPoints = 0;
        protected int totalTalentPoints = 1000;
        protected const float meleeRange = 50;
        protected const float bowRange = 1000;
        protected Dictionary<string, Ability> allAbilities = new Dictionary<string, Ability>();
        //array of key-ability pairs
        protected KeyValuePair<Keys, Ability>[] boundAbilities = new KeyValuePair<Keys, Ability>[12];
        protected KeyValuePair<ButtonState, Ability>[] mouseBoundAbility = new KeyValuePair<ButtonState, Ability>[1];
        protected Dictionary<AbilityName, bool> abilityLearnedFlags = new Dictionary<AbilityName, bool>();

        //Buffs 
        protected Dictionary<Buff, Texture2D> buffIcons = new Dictionary<Buff,Texture2D>();
        protected Dictionary<Buff, Tooltip> buffTooltips = new Dictionary<Buff,Tooltip>();

        //debuffs
        protected Dictionary<DeBuff, Texture2D> debuffIcons = new Dictionary<DeBuff, Texture2D>();
        protected Dictionary<DeBuff, Tooltip> debuffTooltips = new Dictionary<DeBuff, Tooltip>();
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
            if (equipMe.ItemLevel > Level)
            {
                AddToInventory(equipMe);
                (Game as MainGame).AddAlert("That item's level is too high");
                return;
            }

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
            if (equipMe.Slot == slot || equipMe.Slot2 == slot)
            {
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
            else
            {
                AddToInventory(equipMe);
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
                RecalculateStats();
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
                RecalculateStats();
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

            if (toAdd.Name == "gold")
            {
                gold += toAdd.Quantity;
                return true;
            }

            //seeing if it can stack with something already in the inventory
            if (toAdd.Stackable)
            {
                for (int i = 0; i < inventory.Length; ++i)
                {
                    if (inventory[i] != null && inventory[i].ItemID == toAdd.ItemID)
                    {
                        inventory[i].AddQuantity(toAdd.Quantity);
                        return true;
                    }
                }
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

            //inventory is not full and there was nothing to stack with; adding to the first empty slot
            for (int i = 0; i < inventory.Length; ++i)
            {
                if (inventory[i] == null)
                {
                    inventory[i] = toAdd;
                    return true;
                }
            }

            //this won't happen
            //...probably
            //awkward if it does
            return false;
        }
        protected bool RemoveOneFromInventory(int itemID)
        {
            for (int i = 0; i < inventory.Length; ++i)
            {
                if (inventory[i] != null && inventory[i].ItemID == itemID)
                {
                    inventory[i].Quantity--;
                    if (inventory[i].Quantity <= 0)
                    {
                        inventory[i] = null;
                    }
                    return true;
                }
            }
            return false;
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
            statsPerLevelMultiplier = 2;
            baseStatsMultiplier = 10;
            //shared data
            this.attached = Entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;
            this.syncedModels = Entity.GetSharedData(typeof(Dictionary<string, Model>)) as Dictionary<string, Model>;

            InitGeneralFields();

            InitNewPlayer();

            //adding gear for testing
            Equippable wep = (Equippable)lewtz.GetItem(9901);
            wep.SetStats(GearQuality.Legendary, 70);
            EquipGear(wep, GearSlot.Righthand);
            
            Equippable boots = (Equippable)lewtz.GetItem(9900);
            boots.SetStats(GearQuality.Legendary, 70);
            EquipGear(boots, GearSlot.Feet);


            EquipGear((Equippable)lewtz.GetItem(3002), GearSlot.Righthand);
            EquipGear((Equippable)lewtz.GetItem(3102), GearSlot.Righthand);

            Equippable g = (Equippable)lewtz.GetItem(3107);
            g.SetStats(GearQuality.Standard, 2);
            EquipGear(g, GearSlot.Righthand);
        }

        public override void Start()
        {
            attachedArrowL = new AttachableModel(attacks.GetUnanimatedModel("Models\\Projectiles\\Arrow"), GearSlotToBoneName(GearSlot.Lefthand), 0, -MathHelper.PiOver2);
            attachedArrowR = new AttachableModel(attacks.GetUnanimatedModel("Models\\Projectiles\\Arrow"), GearSlotToBoneName(GearSlot.Righthand), 0, -MathHelper.PiOver2);
            base.Start();
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

            boundAbilities[0] = new KeyValuePair<Keys, Ability>(Keys.Q, GetCachedAbility(AbilityName.None));
            boundAbilities[1] = new KeyValuePair<Keys, Ability>(Keys.W, GetCachedAbility(AbilityName.None));
            boundAbilities[2] = new KeyValuePair<Keys, Ability>(Keys.E, GetCachedAbility(AbilityName.None));
            boundAbilities[3] = new KeyValuePair<Keys, Ability>(Keys.R, GetCachedAbility(AbilityName.None));
            boundAbilities[4] = new KeyValuePair<Keys, Ability>(Keys.A, GetCachedAbility(AbilityName.None));
            boundAbilities[5] = new KeyValuePair<Keys, Ability>(Keys.S, GetCachedAbility(AbilityName.None));
            boundAbilities[6] = new KeyValuePair<Keys, Ability>(Keys.D, GetCachedAbility(AbilityName.None));
            boundAbilities[7] = new KeyValuePair<Keys, Ability>(Keys.F, GetCachedAbility(AbilityName.None));
            //added item slot abilities
            boundAbilities[8] = new KeyValuePair<Keys, Ability>(Keys.D1, GetCachedAbility(AbilityName.None));
            boundAbilities[9] = new KeyValuePair<Keys, Ability>(Keys.D2, GetCachedAbility(AbilityName.None));
            boundAbilities[10] = new KeyValuePair<Keys, Ability>(Keys.D3, GetCachedAbility(AbilityName.None));
            boundAbilities[11] = new KeyValuePair<Keys, Ability>(Keys.D4, GetCachedAbility(AbilityName.None));

            mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(ButtonState.Pressed, GetCachedAbility(AbilityName.None));
            for (int i = 0; i < Enum.GetNames(typeof(AbilityName)).Length; ++i)
            {
                abilityLearnedFlags[(AbilityName)i] = false;
            }

            abilityLearnedFlags[AbilityName.HealthPotion] = true;
            abilityLearnedFlags[AbilityName.SuperHealthPotion] = true;
            abilityLearnedFlags[AbilityName.InstaHealthPotion] = true;
            abilityLearnedFlags[AbilityName.PotionOfLuck] = true;
            abilityLearnedFlags[AbilityName.InivisibilityPotion] = true;
            #endregion
            
            #region de/buff definitions

            buffIcons.Add(Buff.AdrenalineRush, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.AdrenalineRush));
            buffIcons.Add(Buff.Berserk, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Berserk));
            buffIcons.Add(Buff.Berserk2, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Berserk2));
            buffIcons.Add(Buff.Berserk3, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Berserk3));
            buffIcons.Add(Buff.Elusiveness, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Elusiveness));
            buffIcons.Add(Buff.HealthPotion, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.HealthPotion));
            buffIcons.Add(Buff.Homing, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Homing));
            buffIcons.Add(Buff.Invincibility, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Invincibility));
            buffIcons.Add(Buff.LuckPotion, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.LuckPotion));
            buffIcons.Add(Buff.None, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.None));
            buffIcons.Add(Buff.SadisticFrenzy, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.SadistivFrenzy));
            buffIcons.Add(Buff.SerratedBleeding, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.SerratedBleeding));
            buffIcons.Add(Buff.SuperHealthPotion, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.SuperHealthPotion));
            buffIcons.Add(Buff.Swordnado, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Swordnado));
            buffIcons.Add(Buff.Unstoppable, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Buffs.Unstoppable));

            buffTooltips.Add(Buff.AdrenalineRush, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Adrenaline Rush", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.Berserk, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Berserk", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.Berserk2, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Berserk", .65f), new TooltipLine(Color.Gold, "todo2", .4f) }));
            buffTooltips.Add(Buff.Berserk3, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Berserk", .65f), new TooltipLine(Color.Gold, "todo3", .4f) }));
            buffTooltips.Add(Buff.Elusiveness, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Elusiveness", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.HealthPotion, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Health Potion", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.Homing, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Homing", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.Invincibility, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Invincibility", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.LuckPotion, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Luck Potion", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.None, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "None", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.SadisticFrenzy, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Sadistic Frenzy", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.SerratedBleeding, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Serrated Bleeding", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.SuperHealthPotion, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Super Health Potion", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.Swordnado, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Swordnado", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            buffTooltips.Add(Buff.Unstoppable, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Unstoppable", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));

            debuffIcons.Add(DeBuff.Burning, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Burning));
            debuffIcons.Add(DeBuff.Charge, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Charge));
            debuffIcons.Add(DeBuff.Execute, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Execute));
            debuffIcons.Add(DeBuff.FlashBomb, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.FlashBomb));
            debuffIcons.Add(DeBuff.ForcefulThrow, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.ForcefulThrow));
            debuffIcons.Add(DeBuff.Frost, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Frost));
            debuffIcons.Add(DeBuff.Frozen, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Frozen));
            debuffIcons.Add(DeBuff.Garrote, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Garrote));
            debuffIcons.Add(DeBuff.Headbutt, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Headbutt));
            debuffIcons.Add(DeBuff.MagneticImplant, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.MagneticImplant));
            debuffIcons.Add(DeBuff.None, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.None));
            debuffIcons.Add(DeBuff.SerratedBleeding, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.SerratedBleeding));
            debuffIcons.Add(DeBuff.Stunned, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Stunned));
            debuffIcons.Add(DeBuff.Tar, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.DeBuffs.Tar));

            debuffTooltips.Add(DeBuff.Burning, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Burning", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Charge, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Charge", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Execute, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Execute", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.FlashBomb, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Flash Bomb", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.ForcefulThrow, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Forceful Throw", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Frost, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Frost", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Frozen, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Frozen", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Garrote, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Garrote", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Headbutt, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Headbutt", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.MagneticImplant, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Magnetic Implant", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.None, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "None", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.SerratedBleeding, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Serrated Bleeding", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Stunned, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Stunned", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));
            debuffTooltips.Add(DeBuff.Tar, new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Tar", .65f), new TooltipLine(Color.Gold, "todo", .4f) }));

            #endregion
        }

        #region Action Sequences

        protected bool usingPrimary = false;
        protected Vector3 velDir = Vector3.Zero;
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
        protected void UseSequenceParallel(string name)
        {
            actionSequences[name][0]();
        }

        protected void StartSequence(string name)
        {
            if (name != "PlaceHolder" && name != "")
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
                currentSequence = actionSequences[name];
                actionIndex = 0;
                currentActionName = name;
                stateResetCounter = double.MaxValue;
                usingPrimary = false;
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
            //misc
            actionSequences.Add("loot", LootActions());
            actionSequences.Add("loot_spin", LootSpinActions());
            actionSequences.Add("loot_smash", LootSmashActions());
            actionSequences.Add("death", DeathActions());
            //potions
            actionSequences.Add(AbilityName.HealthPotion.ToString(), HealthPotActions());
            actionSequences.Add(AbilityName.SuperHealthPotion.ToString(), SuperHealthPotActions());
            actionSequences.Add(AbilityName.PotionOfLuck.ToString(), LuckPotActions());
            actionSequences.Add(AbilityName.InstaHealthPotion.ToString(), InstaHealthPotActions());

            //primary attacks
            actionSequences.Add("swing", SwingActions());
            actionSequences.Add("shoot", ShootActions());
            actionSequences.Add("punch", PunchActions());
            actionSequences.Add("magic", MagicActions());
            //abilities
            //ranged
            actionSequences.Add("snipe", SnipeActions());
            actionSequences.Add("omnishot", OmnishotActions());
            actionSequences.Add("buffrush", AdrenalineRushActions());
            actionSequences.Add("loosecannon", LooseCannonActions());
            actionSequences.Add("makeitrain", MakeItRainActions());
            actionSequences.Add("flashbomb", FlashBombActions());
            actionSequences.Add("grapplinghook", GrapplingHookActions());
            actionSequences.Add("moltenbolt", MoltenBoltActions());
            actionSequences.Add("tumble", TumbleActions());

            //melee
            actionSequences.Add("cleave", CleaveActions());
            actionSequences.Add("devastatingstrike", DevastatingStrikeActions());
            actionSequences.Add("reflect", ReflectActions());
            actionSequences.Add("charge", ChargeActions());
            actionSequences.Add("garrote", GarroteActions());
            actionSequences.Add("berserk", BerserkActions());
            actionSequences.Add("headbutt", HeadbuttActions());
            actionSequences.Add("execute", ExecuteActions());
            actionSequences.Add("chainspear", ChainSpearActions());
            actionSequences.Add("swordnado", SwordnadoActions());

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
        private const int arrowDrawMillis = 50;
        private const int arrowReleaseMillis = 150;

        //Misc actions
        private List<Action> LootActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                attState = AttackState.Locked;
                PlayAnimation("k_loot", MixType.PauseAtEnd);
                millisActionLength = animations.GetAniMillis("k_loot");

                string attstr = GearSlot.Righthand.ToString();
                if (attached.ContainsKey(attstr) && attached[attstr] != null)
                {
                    attached[attstr].Draw = false;
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
                canInterrupt = false;
                PlayAnimation("k_loot_smash", MixType.MixInto);
                millisActionLength = 500;
            });
            sequence.Add(() =>
            {
                canInterrupt = true;
                attState = AttackState.None;
                millisActionLength = animations.GetAniMillis("k_loot_smash") - 500;
            });
            sequence.Add(() =>
            {
                string attstr = GearSlot.Righthand.ToString();
                if (attached.ContainsKey(attstr) && attached[attstr] != null)
                {
                    attached[attstr].Draw = true;
                }
                PlayAnimation("k_fighting_stance", MixType.None);
            });

            interruptActions.Add("loot_smash", () =>
            {
                string attstr = GearSlot.Righthand.ToString();
                if (attached.ContainsKey(attstr) && attached[attstr] != null)
                {
                    attached[attstr].Draw = true;
                }
                PlayAnimation("k_fighting_stance", MixType.None);
            });
            return sequence;
        }
        private List<Action> DeathActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                attState = AttackState.Locked;
                canInterrupt = false;
                Entity.GetComponent(typeof(PhysicsComponent)).End();
                PlayAnimation("k_death", MixType.PauseAtEnd);
                millisActionLength = animations.GetAniMillis("k_death");
            });

            sequence.Add(() =>
            {
                attState = AttackState.None;
                Entity.GetComponent(typeof(PhysicsComponent)).Start();
                physicalData.Position = (Game.Services.GetService(typeof(LevelManager)) as LevelManager).GetPlayerSpawnLocation();
                ReviveAlive();
                StartSequence("fightingstance");
                AddBuff(Buff.Invincibility, Entity);
            });

            return sequence;
        }

        //potion actions
        private List<Action> HealthPotActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (RemoveOneFromInventory(1))
                {
                    model.AddEmitter(typeof(LifestealParticleSystem), "potion", 10, 15, Vector3.Zero);
                    model.AddParticleTimer("potion", 5000);
                    AddBuff(Buff.HealthPotion, Entity);
                }
                else
                {
                    (Game as MainGame).AddAlert("No potions left!");
                }
                abilityFinishedAction();
            });

            return sequence;
        }
        private List<Action> SuperHealthPotActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (RemoveOneFromInventory(2))
                {
                    model.AddEmitter(typeof(LifestealParticleSystem), "potion", 10, 15, Vector3.Zero);
                    model.AddParticleTimer("potion", 5000);
                    AddBuff(Buff.SuperHealthPotion, Entity);
                }
                else
                {
                    (Game as MainGame).AddAlert("No potions left!");
                }
                abilityFinishedAction();
            });

            return sequence;
        }
        private List<Action> LuckPotActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (RemoveOneFromInventory(4))
                {
                    AddBuff(Buff.LuckPotion, Entity);
                    //attacks.SpawnLuckyParticles(physicalData.Position);
                }
                else
                {
                    (Game as MainGame).AddAlert("No potions left!");
                }
                abilityFinishedAction();
            });

            return sequence;
        }
        private List<Action> InstaHealthPotActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                if (RemoveOneFromInventory(3))
                {
                    LifeSteal((int)Math.Ceiling(MaxHealth * .3f));
                }
                else
                {
                    (Game as MainGame).AddAlert("No potions left!");
                }
                abilityFinishedAction();
            });

            return sequence;
        }

        //Primary attacks
        private List<Action> PunchActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                stateResetCounter = 0;
                canInterrupt = true;
                PlayAnimation("k_punch", MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = 200;
                usingPrimary = true;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent, false);
                millisActionLength = animations.GetAniMillis("k_punch") - millisActionLength;
                needInterruptAction = false;
            });
            sequence.Add(() =>
            {
                StartSequence("fightingstance");
                attState = AttackState.None;
            });

            interruptActions.Add("k_punch", () =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent, false);
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
                PlayAnimation("k_shoot" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = arrowDrawMillis;
                usingPrimary = true;
            });
            sequence.Add(() =>
            {
                //attach arrow model to hand
                if (!attached.ContainsKey("handarrow"))
                {
                    if (aniSuffix == "_r")
                    {
                        attached.Add("handarrow", attachedArrowR);
                    }
                    else
                    {
                        attached.Add("handarrow", attachedArrowL);
                    }
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
                
                millisActionLength = animations.GetAniMillis("k_shoot" + aniSuffix) - arrowReleaseMillis - arrowDrawMillis;

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

                PlayAnimation("k_fighting_stance", MixType.None);
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
                PlayAnimation("k_swing" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = 350;

                usingPrimary = true;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                Vector3 pos = physicalData.Position + forward * 35;
                attacks.CreateMeleeAttack(pos, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent, abilityLearnedFlags[AbilityName.RejuvenatingStrikes]? .1f : 0, aniSuffix == "_twohand");

                pos.Y = 3;
                if (aniSuffix != "_twohand")
                {
                    attacks.SpawnWeaponSparks(pos);
                }
                millisActionLength = animations.GetAniMillis("k_swing" + aniSuffix) - millisActionLength;
                needInterruptAction = false;
            });
            sequence.Add(() =>
            {
                StartSequence("fightingstance");
                attState = AttackState.None;
            });

            //if interrupted, this should still create a melee attack
            interruptActions.Add("swing", () =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMeleeAttack(physicalData.Position + forward * 35, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent, abilityLearnedFlags[AbilityName.RejuvenatingStrikes] ? .1f : 0, aniSuffix == "_twohand");
            });

            return sequence;
        }
        private List<Action> MagicActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                stateResetCounter = 0;
                canInterrupt = true;
                PlayAnimation("k_magic" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = 200;
                if (aniSuffix == "_twohand")
                {
                    millisActionLength *= 2;
                }
                usingPrimary = true;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMagicAttack(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Intellect), this as AliveComponent);
                millisActionLength = animations.GetAniMillis("k_magic" + aniSuffix) - millisActionLength;
                needInterruptAction = false;
            });
            sequence.Add(() =>
            {
                StartSequence("fightingstance");
                attState = AttackState.None;
            });

            //if interrupted, this should still create a melee attack
            interruptActions.Add("magic", () =>
            {
                Vector3 forward = GetForward();
                attacks.CreateMagicAttack(physicalData.Position + forward * 10, forward, GeneratePrimaryDamage(StatType.Intellect), this as AliveComponent);
            });

            return sequence;
        }


        //Ranged abilities
        private List<Action> SnipeActions()
        {
            //some of these actions are identical to "shoot", so just copying those Actions
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                canInterrupt = true;
                PlayAnimation("k_shoot" + aniSuffix, MixType.None);
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
                bool headshot = false;
                if (abilityLearnedFlags[AbilityName.Headshot] && rand.Next(0, 100) < 25)
                {
                    damage *= 2;
                    headshot = true;
                }
                attacks.CreateSnipe(physicalData.Position + forward * 10, forward, damage, this as AliveComponent, abilityLearnedFlags[AbilityName.MagneticImplant] && headshot);

                millisActionLength = animations.GetAniMillis("k_shoot" + aniSuffix) - arrowReleaseMillis - arrowDrawMillis;

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
                PlayAnimation("k_shoot" + aniSuffix, MixType.None);
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
                attacks.CreateOmnishot(physicalData.Position + forward * 10, forward, 25, this as AliveComponent, activeBuffs.ContainsKey(Buff.Homing), abilityLearnedFlags[AbilityName.Penetrating], abilityLearnedFlags[AbilityName.Leeching], activeBuffs.ContainsKey(Buff.SerratedBleeding));

                millisActionLength = animations.GetAniMillis("k_shoot" + aniSuffix) - arrowReleaseMillis - arrowDrawMillis;

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
        private List<Action> AdrenalineRushActions()
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
                needInterruptAction = true;
                //attach arrow model to hand
                if (!attached.ContainsKey("handarrow"))
                {
                    if (aniSuffix == "_r")
                    {
                        attached.Add("handarrow", attachedArrowR);
                    }
                    else
                    {
                        attached.Add("handarrow", attachedArrowL);
                    }
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

                millisActionLength = animations.GetAniMillis("k_shoot" + aniSuffix) - arrowReleaseMillis - arrowDrawMillis;
                needInterruptAction = false;
                attState = AttackState.None;

                lastUsedAbility.Use(GetStat(StatType.CooldownReduction));
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
                        targetedGroundSize *= 4f;
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

                millisActionLength = animations.GetAniMillis("k_shoot" + aniSuffix) - arrowReleaseMillis - arrowDrawMillis;
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

                    canInterrupt = true;
                    PlayAnimation("k_throw", MixType.None);
                    attState = AttackState.Locked;
                    millisActionLength = 300;
                    stateResetCounter = 0;

                    groundAbilityTarget = mouseHoveredLocation;
                    groundTargetLocation = physicalData.Position;
                }
            });

            sequence.Add(() =>
            {
                float radius = bombSize;
                if (abilityLearnedFlags[AbilityName.BiggerBombs])
                {
                    radius *= 2.5f;
                }
                attacks.CreateFlashBomb(physicalData.Position, groundAbilityTarget, radius, abilityLearnedFlags[AbilityName.TarBomb],this as AliveComponent);

                millisActionLength = animations.GetAniMillis("k_throw") - millisActionLength;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private const float grapplingHookSpeed = 600;
        private List<Action> GrapplingHookActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                attState = AttackState.Locked;
                PlayAnimation("k_throw", MixType.PauseAtEnd);
                millisActionLength = 300;
                stateResetCounter = 0;
            });

            sequence.Add(() =>
            {
                attState = AttackState.None;
                float speed = grapplingHookSpeed;
                if (abilityLearnedFlags[AbilityName.SpeedyGrapple])
                {
                    speed *= 2.5f;
                }

                Vector3 forward = GetForward();
                attacks.CreateGrapplingHook(physicalData.Position + forward * 10, forward, this as AliveComponent, speed);

                millisActionLength = animations.GetAniMillis("k_throw") - 300;

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
                PlayAnimation("k_shoot" + aniSuffix, MixType.None);
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

                millisActionLength = animations.GetAniMillis("k_shoot" + aniSuffix) - arrowReleaseMillis - arrowDrawMillis;

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
        private List<Action> TumbleActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                physicalData.CollisionInformation.CollisionRules.Group = Game.UntouchableCollisionGroup;
                canInterrupt = false;
                PlayAnimation("k_tumble", MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                velDir = GetForward() * 500;
                ChangeVelocity(velDir);
                millisActionLength = 600;
                double elusiveLength = 600;
                if (abilityLearnedFlags[AbilityName.Elusiveness])
                {
                    elusiveLength += 3000;
                }
                AddBuff(Buff.Elusiveness, elusiveLength, Entity);

            });

            sequence.Add(() =>
            {
                physicalData.CollisionInformation.CollisionRules.Group = Game.PlayerCollisionGroup;
                velDir = Vector3.Zero;
                ChangeVelocity(Vector3.Zero);
                millisActionLength = 0;
                groundTargetLocation = physicalData.Position;
            });

            sequence.Add(abilityFinishedAction);

            return sequence;
        }

        //Melee abilities
        double spinCreateMillis = 100;
        private List<Action> CleaveActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_spin", MixType.None);
                attState = AttackState.Locked;
                millisActionLength = spinCreateMillis;

                model.AddEmitter(typeof(FireSwipeSystem), "cleavetrail", 150, 0, Vector3.Zero, GearSlotToBoneName(GearSlot.Righthand));
                model.SetEmitterUp("cleavetrail", -4);
                model.AddParticleTimer("cleavetrail", animations.GetAniMillis("k_spin"));
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateCleave(physicalData.Position, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent,
                    abilityLearnedFlags[AbilityName.Decapitation],
                    abilityLearnedFlags[AbilityName.Invigoration]);

                millisActionLength = animations.GetAniMillis("k_spin") - spinCreateMillis;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> ReflectActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_spin", MixType.None);
                attState = AttackState.Locked;
                millisActionLength = spinCreateMillis;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateReflect(physicalData.Position + forward * 35, GetPhysicsYaw(forward), this as AliveComponent);

                millisActionLength = 300;
            });
            sequence.Add(() =>
            {
                model.AddEmitter(typeof(FrostSwipeSystem), "cleavetrail", 150, 0, Vector3.Zero, GearSlotToBoneName(GearSlot.Righthand));
                model.SetEmitterUp("cleavetrail", -4);
                model.AddParticleTimer("cleavetrail", 100);

                millisActionLength = animations.GetAniMillis("k_spin") - spinCreateMillis - millisActionLength;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> BerserkActions()
        {
            List<Action> sequence = new List<Action>();

            sequence.Add(() =>
            {
                attacks.SpawnBuffParticles(physicalData.Position);
                if (abilityLearnedFlags[AbilityName.SecondWind])
                {
                    AddBuff(Buff.Berserk2, Entity);
                }
                else if (abilityLearnedFlags[AbilityName.RiskyRegeneration])
                {
                    AddBuff(Buff.Berserk3, Entity);
                }
                else
                {
                    AddBuff(Buff.Berserk, Entity);
                }

                attState = AttackState.None;
            });

            return sequence;
        }
        private List<Action> HeadbuttActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                //TODO: replace with k_headbutt
                PlayAnimation("k_headbutt", MixType.None);
                attState = AttackState.Locked;
                millisActionLength = 200;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateHeadbutt(physicalData.Position + forward * 35, GetPhysicsYaw(forward), this as AliveComponent);

                millisActionLength = animations.GetAniMillis("k_headbutt") - 200;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> ChargeActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                physicalData.CollisionInformation.CollisionRules.Group = Game.UntouchableCollisionGroup;
                canInterrupt = false;
                PlayAnimation("k_charge", MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                velDir = GetForward() * 400;
                ChangeVelocity(velDir);
                millisActionLength = 800;
                AddBuff(Buff.Unstoppable, 800, Entity);
                attacks.CreateCharge(physicalData.Position, this as AliveComponent, millisActionLength);
            });

            sequence.Add(() =>
            {
                physicalData.CollisionInformation.CollisionRules.Group = Game.PlayerCollisionGroup;
                velDir = Vector3.Zero;
                ChangeVelocity(Vector3.Zero);
                millisActionLength = 0;
                groundTargetLocation = physicalData.Position;
            });

            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> GarroteActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                if (aniSuffix == "_twohand")
                {
                    aniSuffix = "_r";
                }
                PlayAnimation("k_thrust" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                millisActionLength = 350;
                stateResetCounter = 0;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.CreateGarrote(physicalData.Position + forward * 35, GetPhysicsYaw(forward), this as AliveComponent, GeneratePrimaryDamage(StatType.Strength) * 5, abilityLearnedFlags[AbilityName.ExcruciatingTwist]);

                if (abilityLearnedFlags[AbilityName.SadisticFrenzy])
                {
                    AddBuff(Buff.SadisticFrenzy, Entity);
                }
                millisActionLength = animations.GetAniMillis("k_thrust" + aniSuffix) - 350;
            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> DevastatingStrikeActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                PlayAnimation("k_flip", MixType.None);
                attState = AttackState.Locked;
                millisActionLength = 700;
            });
            sequence.Add(() =>
            {
                millisActionLength = 200;
                model.AddEmitter(typeof(DevastateTrailSystem), "cleavetrail", abilityLearnedFlags[AbilityName.DevastatingReach] ? 220 : 150, 0, Vector3.Zero, GearSlotToBoneName(GearSlot.Righthand));
                model.SetEmitterUp("cleavetrail", abilityLearnedFlags[AbilityName.DevastatingReach] ? -6 : -2);
                model.AddParticleTimer("cleavetrail", millisActionLength);
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                attacks.DevastingStrike(physicalData.Position, forward, GeneratePrimaryDamage(StatType.Strength) * 10, this as AliveComponent, abilityLearnedFlags[AbilityName.DevastatingReach]);
                millisActionLength = animations.GetAniMillis("k_flip") - 900;
            });
            sequence.Add(() =>
            {
                attState = AttackState.None;
                StartSequence("fightingstance");
            });

            return sequence;
        }
        private List<Action> ExecuteActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                if (aniSuffix == "_twohand")
                {
                    aniSuffix = "_r";
                }
                PlayAnimation("k_thrust" + aniSuffix, MixType.None);
                attState = AttackState.Locked;
                stateResetCounter = 0;
                millisActionLength = 350;
            });
            sequence.Add(() =>
            {
                Vector3 forward = GetForward();
                Vector3 pos = physicalData.Position + forward * 35;
                attacks.CreateExecute(pos, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent);
                millisActionLength = animations.GetAniMillis("k_thrust" + aniSuffix) - millisActionLength;
            });
            sequence.Add(abilityFinishedAction);
            return sequence;
        }
        private List<Action> ChainSpearActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(actionSequences["grapplinghook"][0]);
            sequence.Add(() =>
            {
                attState = AttackState.None;
                float speed = grapplingHookSpeed;
                if (abilityLearnedFlags[AbilityName.ForcefulThrow])
                {
                    speed *= 2f;
                }

                Vector3 forward = GetForward();
                attacks.CreateChainSpear(physicalData.Position + forward * 10, forward, this as AliveComponent, speed, abilityLearnedFlags[AbilityName.ForcefulThrow]);

                millisActionLength = animations.GetAniMillis("k_throw") - 300;

            });
            sequence.Add(abilityFinishedAction);

            return sequence;
        }
        private List<Action> SwordnadoActions()
        {
            List<Action> sequence = new List<Action>();
            sequence.Add(() =>
            {
                attState = AttackState.LockedMoving;
                PlayAnimation("k_whirlwind", MixType.None, 2f);
                millisActionLength = animations.GetAniMillis("k_whirlwind") * 20 * .5f;

                model.AddEmitter(typeof(FrostSwipeSystem), "cleavetrail", 100, 0, Vector3.Zero, GearSlotToBoneName(GearSlot.Righthand));
                model.SetEmitterUp("cleavetrail", -3);
                model.AddParticleTimer("cleavetrail", millisActionLength);

                AddBuff(Buff.Unstoppable, millisActionLength, Entity);
                attacks.CreateSwordnado(physicalData.Position, millisActionLength, GeneratePrimaryDamage(StatType.Strength), this as AliveComponent);
            });

            sequence.Add(abilityFinishedAction);


            return sequence;
        }

        //Magic Abilities
        



        int fightingStanceLoops = 0;
        AttachableModel attachedArrowL;
        AttachableModel attachedArrowR;
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
            LockedMoving,
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
        private void PlayAnimation(string animationName, MixType t, float playbackRate)
        {
            animations.StartClip(animationName, t, playbackRate);
            currentAniName = animationName;
            aniCounter = 0;
            aniLength = animations.GetAniMillis(animationName);
        }
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

            if (damage > 0)
            {
                (Game as MainGame).AddFloatingText(physicalData.Position, "" + damage, Color.Red, .5f);
            }
        }
        #endregion

        #region helpers
        protected bool inSoulevator = false;
        public void EnterSoulevator()
        {
            StartSequence("idle");
            attState = AttackState.Locked;
            inSoulevator = true;
        }
        protected void ExitSoulevator()
        {
            attState = AttackState.None;
            inSoulevator = false;
        }
        protected void CloseLoot()
        {
            looting = false;
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
                lootingSoul = (possLoot.GetComponent(typeof(AIComponent)) as LootSoulController);
                if (lootingSoul.soulState != LootSoulController.LootSoulState.BeingLooted && lootingSoul.soulState != LootSoulController.LootSoulState.Dying)
                {
                    StartSequence("loot");
                    lootingSoul.OpenLoot(physicalData.Position + Vector3.Down * 18, physicalData.Orientation);
                    looting = true;

                    groundTargetLocation = physicalData.Position;
                }
                else
                {
                    lootingSoul = null;
                }
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
            abilityFinishedAction();
            attState = AttackState.None;
            base.StopPull();
        }
        #endregion

        #region Ability Definitions
        private Dictionary<AbilityName, Ability> cachedAbilities = new Dictionary<AbilityName, Ability>();

        protected Ability GetCachedAbility(AbilityName ability)
        {
            if (!cachedAbilities.ContainsKey(ability))
            {
                cachedAbilities[ability] = GetAbility(ability);
            }

            return cachedAbilities[ability];
        }
        private Ability GetAbility(AbilityName ability)
        {
            switch (ability)
            {
                case AbilityName.None:
                    return GetNone();
                case AbilityName.Snipe://ranged actives
                    return GetSnipe();
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
                case AbilityName.GrapplingHook:
                    return GetGrapplingHook();
                case AbilityName.MoltenBolt:
                    return GetMoltenBolt();
                case AbilityName.Tumble:
                    return GetTumble();
                case AbilityName.Penetrating://ranged passives
                    return GetPenetrating();
                case AbilityName.TarBomb:
                    return GetTarBomb();
                case AbilityName.Homing:
                    return GetHoming();
                case AbilityName.Serrated:
                    return GetSerrated();
                case AbilityName.Headshot:
                    return GetHeadshot();
                case AbilityName.MagneticImplant:
                    return GetMagneticImplant();
                case AbilityName.MakeItHail:
                    return GetMakeItHail();
                case AbilityName.StrongWinds:
                    return GetStrongWinds();
                case AbilityName.SpeedyGrapple:
                    return GetSpeedyGrapple();
                case AbilityName.BiggerBombs:
                    return GetBiggerBombs();
                case AbilityName.Elusiveness:
                    return GetElusiveness();

                case AbilityName.Cleave://melee actives
                    return GetCleave();
                case AbilityName.DevastatingStrike:
                    return GetDevastatingStrike();
                case AbilityName.Reflect:
                    return GetReflect();
                case AbilityName.Charge:
                    return GetCharge();
                case AbilityName.Garrote:
                    return GetGarrote();
                case AbilityName.Berserk:
                    return GetBerserk();
                case AbilityName.Headbutt:
                    return GetHeadbutt();
                case AbilityName.Execute:
                    return GetExecute();
                case AbilityName.Swordnado:
                    return GetSwordnado();
                case AbilityName.ChainSpear:
                    return GetChainSpear();
                case AbilityName.Decapitation://melee passives
                    return GetDecapitation();
                case AbilityName.Invigoration:
                    return GetInvigoration();
                case AbilityName.ObsidianCoagulation:
                    return GetObsidianCoagulation();
                case AbilityName.DevastatingReach:
                    return GetDevastatingReach();
                case AbilityName.ExcruciatingTwist:
                    return GetExcruciatingTwist();
                case AbilityName.SadisticFrenzy:
                    return GetSadisticFrenzy();
                case AbilityName.Bash:
                    return GetBash();
                case AbilityName.SecondWind:
                    return GetSecondWind();
                case AbilityName.RiskyRegeneration:
                    return GetRiskyRegeneration();
                case AbilityName.RejuvenatingStrikes:
                    return GetRejuvenatingStrikes();
                case AbilityName.ForcefulThrow:
                    return GetForcefulThrow();

                case AbilityName.IceClawPrison://magic actives
                    return GetIceClawPrison();

                case AbilityName.HealthPotion://potions
                    return GetHealthPotion();
                case AbilityName.SuperHealthPotion:
                    return GetSuperHealthPotion();
                case AbilityName.InstaHealthPotion:
                    return GetInstaHealthPotion();
                case AbilityName.PotionOfLuck:
                    return GetPotionOfLuck();


                default:
                    return GetNone();
            }
        }

        //Placeholder Ability
        protected Ability GetNone()
        {
            return new Ability(AbilityName.None, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Place_Holder), 0, AttackType.None, "PlaceHolder", AbilityType.Instant, 0, "");
        }


        //ranged
        protected Ability GetSnipe()
        {
            return new Ability(AbilityName.Snipe, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.SNIPE), 
                500, AttackType.Ranged, "snipe", AbilityType.Instant,
                4, "Fire a killshot that gains\n"
                    +"increased damage with range.");
        }
        protected Ability GetOmniShot()
        {
            return new Ability(AbilityName.Omnishot, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.OMNI_SHOT), 
                4000, AttackType.Ranged, "omnishot", AbilityType.Instant,
                10, "Fire an arrow that splits into\n"
                    +"20 arrows (which gain all other\n"
                    +"ranged primary attack bonuses).");
        }
        protected Ability GetAdrenalineRush()
        {
            return new Ability(AbilityName.AdrenalineRush, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.ADREN_RUSH), 
                10000, AttackType.Ranged, "buffrush", AbilityType.Instant,
                2, "Increases attack speed for 60%\n"
                    +"and run speed by 75% for 3sec.");
        }
        protected Ability GetLeechingArrows()
        {
            return new Ability(AbilityName.Leeching, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.LEECH_ARROWS), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Kazgar's primary ranged attacks\n"
                    +"heal for 10% of their damage.");
        }
        protected Ability GetLooseCannon()
        {
            return new Ability(AbilityName.LooseCannon, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.LOOSE_CANNON), 
                6000, AttackType.Ranged, "loosecannon", AbilityType.Charge,
                5, "Charge and release an aimed\n"
                    +"shot that explodes on impact,\n"
                    +"dealing up to 500% damage.");
        }
        protected Ability GetMakeItRain()
        {
            return new Ability(AbilityName.MakeItRain, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.MAKE_IT_RAIN), 
                500, AttackType.Ranged, "makeitrain", AbilityType.GroundTarget,
                5, "Fire bolts into the air, causing a\n"
                    +"Rain of projectiles at target\n"
                    +"location.");
        }
        protected Ability GetFlashBomb()
        {
            return new Ability(AbilityName.FlashBomb, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.FLASH_BOMB), 
                10000, AttackType.Ranged, "flashbomb", AbilityType.GroundTarget,
                2, "Toss a bomb to the target location,\n"
                    +"exploding and stunning nearby\n"
                    +"enemies for 3 seconds.");
        }
        protected Ability GetTarBomb()
        {
            return new Ability(AbilityName.TarBomb, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.TAR_BOMB), 
                0, AttackType.Ranged, "tarbomb", AbilityType.Passive, 
                0, "Slows enemies hit by Flash Bomb\n"
                    +"by 75%  for 9sec.");
        }
        protected Ability GetGrapplingHook()
        {
            return new Ability(AbilityName.GrapplingHook, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.GRAP_HOOK), 
                8000, AttackType.Ranged, "grapplinghook", AbilityType.Instant,
                0, "Throw a harpoon that will pull to\n"
                    +"the first wall it collides with.");
        }
        protected Ability GetMoltenBolt()
        {
            return new Ability(AbilityName.MoltenBolt, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.MOLT_BOLT), 
                3000, AttackType.Ranged, "moltenbolt", AbilityType.Instant,
                1, "Pierces. Enemies hit will burn\n"
                    +"for 7% of their life over 3sec.\n"
                    +"Tarred enemies will combust for\n"
                    +"4x normal damage.");
        }
        protected Ability GetTumble()
        {
            return new Ability(AbilityName.Tumble, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.TUMBLE), 
                4000, AttackType.Ranged, "tumble", AbilityType.Instant,
                0, "Quickly dodge toward the targeted\n"
                    +"direction.");
        }
        protected Ability GetPenetrating()
        {
            return new Ability(AbilityName.Penetrating, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.PENETRATING), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "When Adrenaline Rush is active,\n"
                    +"ranged primary attacks pierce\n"
                    +"through targets.");
        }
        protected Ability GetHoming()
        {
            return new Ability(AbilityName.Homing, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.HOMING), 
                0, AttackType.Ranged, "", AbilityType.Passive, 
                0, "When Adrenaline Rush is active,\n"
                    +"ranged primary attacks home to\n"
                    +"the nearest target.");
        }
        protected Ability GetSerrated()
        {
            return new Ability(AbilityName.Serrated, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.SERRATED), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Your ranged primary attacks make\n"
                    +"enemies bleed for .5% of their\n"
                    +"life over 5 seconds. Stacks.");
        }
        protected Ability GetHeadshot()
        {
            return new Ability(AbilityName.Headshot, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.HEADSHOT), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Gives Snipe a 25% chance to hit\n"
                    +"the target in the head, dealing\n"
                    +"100% increased damage.");
        }
        protected Ability GetMagneticImplant()
        {
            return new Ability(AbilityName.MagneticImplant, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.MAGNETIC_IMPLANT),
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "When Snipe headshots a target,\n"
                    + "Increases damage dealt to that\n"
                    + "target by 100% for 2 seconds.");
        }
        protected Ability GetMakeItHail()
        {
            return new Ability(AbilityName.MakeItHail, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.MAKE_IT_HAIL), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Increases the damage of\n"
                    +"Make it Rain by 150%");
        }
        protected Ability GetStrongWinds()
        {
            return new Ability(AbilityName.StrongWinds, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.STRONG_WINDS), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Increases Make it Rain's area\n"
                    +"of effect by 400%.");
        }
        protected Ability GetSpeedyGrapple()
        {
            return new Ability(AbilityName.SpeedyGrapple, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.SPEEDY_GRAPPLE), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Increases the projectile speed of\n"
                    +"Grappling Hook by 150%.");
        }
        protected Ability GetBiggerBombs()
        {
            return new Ability(AbilityName.BiggerBombs, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.BIGGER_BOMBS), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Increases Flash Bomb's area of\n"
                    +"effect by 150%.");
        }
        protected Ability GetElusiveness()
        {
            return new Ability(AbilityName.Elusiveness, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.ELUSIVENESS), 
                0, AttackType.Ranged, "", AbilityType.Passive,
                0, "Makes Kazgar invulnerable to\n"
                    +"projectiles and attacks for\n"
                    +"3sec after using Tumble.");
        }


        //melee
        protected Ability GetCleave()
        {
            return new Ability(AbilityName.Cleave, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.CLEAVE),
                500, AttackType.Melee, "cleave", AbilityType.Instant,
                3, "Swing a wide arc with your weapon,\n"
                    +"dealing normal damage to all\n"
                    +"enemies around you.");
        }
        protected Ability GetDecapitation()
        {
            return new Ability(AbilityName.Decapitation, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Decapitation),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Gives Cleave a 30% chance to deal\n"
                    +"4x damage to each enemy hit");
        }
        protected Ability GetInvigoration()
        {
            return new Ability(AbilityName.Invigoration, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Invigoration),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "20% of Cleave's damage is\n"
                    +"returned as health.");
        }
        protected Ability GetObsidianCoagulation()
        {
            return new Ability(AbilityName.ObsidianCoagulation, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.ObsidianCoagulation),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Each time you are hit, you take 2%\n"
                    +"less damage for 5 seconds. Stacks\n"
                    +"up to 25 times.");
        }
        protected Ability GetDevastatingStrike()
        {
            return new Ability(AbilityName.DevastatingStrike, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.DevastatingStrike),
                5000, AttackType.Melee, "devastatingstrike", AbilityType.Instant,
                5, "Bring your sword crashing to the\n"
                    +"ground, dealing 10x damage to one\n"
                    +"enemy in front of you.");
        }
        protected Ability GetDevastatingReach()
        {
            return new Ability(AbilityName.DevastatingReach, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.DevastatingReach),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Extends the reach of Devastating\n"
                    +"Strike and allows it to hit\n"
                    +"multiple enemies.");
        }
        protected Ability GetReflect()
        {
            return new Ability(AbilityName.Reflect, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Reflect),
                500, AttackType.Melee, "reflect", AbilityType.Instant,
                1, "Swing a weapon to bounce any\n"
                    +"projectiles back at your enemies.\n"
                    +"Generates one power for each\n"
                    +"projectile reflected.");
        }
        protected Ability GetCharge()
        {
            return new Ability(AbilityName.Charge, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Charge),
                4000, AttackType.Melee, "charge", AbilityType.Instant,
                0, "Rush forward to the target\n"
                        +"location, knocking back\n"
                        +"enemies along the way.");
        }
        protected Ability GetGarrote()
        {
            return new Ability(AbilityName.Garrote, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Garrote),
                3000, AttackType.Melee, "garrote", AbilityType.Instant,
                2, "Deal 5x normal damage to a single\n"
                    +"target.");
        }
        protected Ability GetExcruciatingTwist()
        {
            return new Ability(AbilityName.ExcruciatingTwist, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.ExcruciatingTwist),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Garrote makes enemies bleed for\n"
                    + "10% of their health over 2sec.");
        }
        protected Ability GetSadisticFrenzy()
        {
            return new Ability(AbilityName.SadisticFrenzy, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.SadisticFrenzy),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Gain 20% atack speed for\n"
                    +"4sec after using Garrote.");
        }
        protected Ability GetBash()
        {
            return new Ability(AbilityName.Bash, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Bash),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Gives your melee primary atacks\n"
                    +"a 15% chance to stun for 1sec.");
        }
        protected Ability GetBerserk()
        {
            return new Ability(AbilityName.Berserk, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Berserk),
                10000, AttackType.Melee, "berserk", AbilityType.Instant,
                10, "Gain +70% attack speed for 10sec\n"
                    +"and deal 5% of your max health in\n"
                    +"damage to yourself ever second.");
        }
        protected Ability GetSecondWind()
        {
            return new Ability(AbilityName.SecondWind, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.SecondWind),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "when Berserk ends, gain 40% of\n"
                    +"your life back over 10sec.");
        }
        protected Ability GetRiskyRegeneration()
        {
            return new Ability(AbilityName.RiskyRegeneration, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.RiskyRegeneration),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Increases Berserk's self damage\n"
                    +"to 7% per second, but increases\n"
                    +"Second Wind's healing to 80%.");
        }
        protected Ability GetRejuvenatingStrikes()
        {
            return new Ability(AbilityName.RejuvenatingStrikes, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.RejuvenatingStrikes),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Basic attacks heal for 10% of\n"
                    +"the damage they deal.");
        }
        protected Ability GetHeadbutt()
        {
            return new Ability(AbilityName.Headbutt, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Headbutt),
                0, AttackType.Melee, "headbutt", AbilityType.Instant,
                0, "Headbutt enemies in front of you,\n"
                    +"stunning them for 5sec.");
        }
        protected Ability GetChainSpear()
        {
            return new Ability(AbilityName.ChainSpear, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.ChainSpear),
                4000, AttackType.Melee, "chainspear", AbilityType.Instant,
                1, "Hurl a spear with a chain attached,\n"
                    +"pulling back the first enemy hit.");
        }
        protected Ability GetForcefulThrow()
        {
            return new Ability(AbilityName.ForcefulThrow, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.ForcefulThrow),
                0, AttackType.Melee, "", AbilityType.Passive,
                0, "Increases the projectile speed of\n"
                    + "Chain Spear by 100%, and stuns\n"
                    + "enemies hit by it for 3 seconds.");
        }
        protected Ability GetExecute()
        {
            return new Ability(AbilityName.Execute, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Execute),
                500, AttackType.Melee, "execute", AbilityType.Instant,
                3, "Hits a single target for 15x\n"
                    +"damage if they are below 20% hp");
        }
        protected Ability GetSwordnado()
        {
            return new Ability(AbilityName.Swordnado, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Melee.Swordnado),
                15000, AttackType.Melee, "swordnado", AbilityType.Instant,
                10, "Spin your weapons around in a\n"
                    +"whirlwind, constantly damaging all\n"
                    +"nearby enemies and reflecting all\n"
                    +"projectiles.");
        }


        //magic
        protected Ability GetIceClawPrison()
        {
            return new Ability(AbilityName.IceClawPrison, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Abilities.Range.ICE_CLAW_PRI), 
                6000, AttackType.Ranged, "shoot", AbilityType.Instant,
                5, "");
        }

        
        #endregion

        #region Potion Definitions
        protected Ability GetHealthPotion()
        {
            return new Ability(AbilityName.HealthPotion, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, AttackType.None, AbilityName.HealthPotion.ToString(), AbilityType.Instant, 0, 
                "Heals for 20% health over 5 seconds");
        }
        protected Ability GetSuperHealthPotion()
        {
            return new Ability(AbilityName.PotionOfLuck, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.SUPER_HEALTH), 0, AttackType.None, AbilityName.SuperHealthPotion.ToString(), AbilityType.Instant, 0, 
                "Heals for 40% health over 5 seconds");
        }
        protected Ability GetPotionOfLuck()
        {
            return new Ability(AbilityName.PotionOfLuck, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.LUCK), 0, AttackType.None, AbilityName.PotionOfLuck.ToString(), AbilityType.Instant, 0, 
                "Increases the quality of loot\ndropped for 2 minutes");
        }
        protected Ability GetInstaHealthPotion()
        {
            return new Ability(AbilityName.PotionOfLuck, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.INSTA_HEALTH), 0, AttackType.None, AbilityName.InstaHealthPotion.ToString(), AbilityType.Instant, 0, 
                "Heals for 30% health instantly");
        }

        #endregion


    }
    public enum AbilityName
    {
        None,


        Snipe,//ranged
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
        Elusiveness,
        Tumble,
        Omnishot,


        Cleave,//melee
        Decapitation,
        Invigoration,
        ObsidianCoagulation,
        DevastatingStrike,
        DevastatingReach,
        Reflect,
        Charge,
        Garrote,
        ExcruciatingTwist,
        SadisticFrenzy,
        Bash,
        Berserk,
        SecondWind,
        RiskyRegeneration,
        RejuvenatingStrikes,
        Headbutt,
        ChainSpear,
        ForcefulThrow,
        Execute,
        Swordnado,




        IceClawPrison,//magic



        HealthPotion,
        SuperHealthPotion,
        InstaHealthPotion,
        PotionOfLuck,
        InivisibilityPotion,
    }
}
