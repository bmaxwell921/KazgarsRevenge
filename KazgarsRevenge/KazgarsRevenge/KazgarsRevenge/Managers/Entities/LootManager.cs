﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public class LootManager : EntityManager
    {

        List<GameEntity> lootSouls = new List<GameEntity>();

        public LootManager(KazgarsRevengeGame game)
            : base(game)
        {

        }

        ParticleManager particles;
        public override void Initialize()
        {
            base.Initialize();
            this.particles = Game.Services.GetService(typeof(ParticleManager)) as ParticleManager;
            InitializeItems();
        }

        public void CreateLootSoul(Vector3 position, GameEntity entity)
        {
            CreateLootSoul(position, (List<Item>) GetLootFor(levelManager.currentLevel.currentFloor, entity), 1);
        }

        public void CreateLootSoul(Vector3 position, List<Item> first, List<Item> second, int totalSouls)
        {
            CreateLootSoul(position, CombineLoot(first, second), totalSouls);
        }

        public void CreateLootSoul(Vector3 position, List<Item> containedLoot, int totalSouls)
        {
            position.Y = 10;
            GameEntity lootSoul = new GameEntity("loot", FactionType.Neutral, EntityType.Misc);
            float size = 3 + totalSouls;

            Entity lootPhysicalData = new Box(position, size, size, size, 1);
            lootPhysicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            lootPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            lootPhysicalData.IsAffectedByGravity = false;
            lootSoul.AddSharedData(typeof(Entity), lootPhysicalData);

            Model lootModel = GetAnimatedModel("Models\\soul_idle");
            AnimationPlayer lootAnimations = new AnimationPlayer(lootModel.Tag as SkinningData);
            lootSoul.AddSharedData(typeof(AnimationPlayer), lootAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            lootSoul.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            PhysicsComponent lootPhysics = new PhysicsComponent(mainGame, lootSoul);
            AnimatedModelComponent lootGraphics = new AnimatedModelComponent(mainGame, lootSoul, lootModel, size, Vector3.Zero);
            LootSoulController lootController = new LootSoulController(mainGame, lootSoul, 10, containedLoot, totalSouls);
            BlobShadowDecal lootShadow = new BlobShadowDecal(mainGame, lootSoul, size);

            lootGraphics.AddEmitter(typeof(SoulTrailParticleSystem), "trail", 8, Math.Max(1, (int)size / 2), Vector3.Up * 5, 6);

            lootSoul.AddComponent(typeof(PhysicsComponent), lootPhysics);
            genComponentManager.AddComponent(lootPhysics);

            lootSoul.AddComponent(typeof(UnanimatedModelComponent), lootGraphics);
            modelManager.AddComponent(lootGraphics);

            lootSoul.AddComponent(typeof(AIComponent), lootController);
            genComponentManager.AddComponent(lootController);

            lootSoul.AddComponent(typeof(BlobShadowDecal), lootShadow);
            billboardManager.AddComponent(lootShadow);

            SpawnSoulPoof(position);
        }

        public void SpawnSoulPoof(Vector3 position)
        {
            for (int i = 0; i < 20; ++i)
            {
                particles.GetSystem(typeof(SoulCreationParticleSystem)).AddParticle(position, Vector3.Zero);
            }
        }

        public List<Item> CombineLoot(List<Item> first, List<Item> second)
        {
            List<Item> retItems = new List<Item>();

            //adding all first items
            for (int i = 0; i < first.Count; ++i)
            {
                retItems.Add(first[i]);
            }

            //merging second list with first (checking for stackables)
            int prevMaxIndex = retItems.Count;
            for (int i = 0; i < second.Count; ++i)
            {
                //merge stacks
                if (second[i].Stackable)
                {
                    for (int j = 0; j < prevMaxIndex; ++j)
                    {
                        if (retItems[j].Name == second[i].Name)
                        {
                            retItems[j].AddQuantity(second[i].Quantity);
                            //if it's gold, make sure the icon is appropriate for the amount of gold
                            if (retItems[j].Name == "gold")
                            {
                                int quantity = retItems[j].Quantity;
                                retItems[j] = new Item(ItemType.Gold, GetGoldIcon(quantity), "gold", quantity, 0);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    retItems.Add(second[i]);
                }
            }

            return retItems;
        }

        #region Loot
        public Dictionary<int, Item> AllItems = new Dictionary<int, Item>();

        private IList<Item> GetLootFor(FloorName floor, GameEntity deadEntity)
        {
            DropTable table = deadEntity.GetComponent(typeof(DropTable)) as DropTable;
            if (table == null)
            {
                throw new Exception("UH OH SOMEONE DOESN'T HAVE A DROP TABLE!: " + deadEntity.Name);
            }

            // TODO we need to pass in the entity that killed it so we can properly
            // apply some of the boosts. Ie Potion of Luck
            AliveComponent killer = null;
            AliveComponent deadguy = deadEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
            if (deadguy != null)
            {
                killer = deadguy.Killer;
            }
            return table.GetDrops(floor, killer);
        }

        public DropTable CreateNormalDropTableFor(GameEntity enemy, AttackType type1, AttackType type2)
        {
            FloorName currentFloor = levelManager.currentLevel.currentFloor;
            DropTable dt = new DropTable(Game as KazgarsRevengeGame, enemy, DropTable.GetNormalAddItemLevel);
            switch (type1)
            {
                case AttackType.None:
                    break;
                case AttackType.Melee:
                    AddMeleeDrops(dt, currentFloor);
                    break;
                case AttackType.Ranged:
                    AddRangedDrops(dt, currentFloor);
                    break;
                case AttackType.Magic:
                    AddMagicDrops(dt, currentFloor);
                    break;
            }

            //possibility to make an enemy drop 2 types of gear (like enchanted armor; kinda both melee and magic)
            //this can be used for balancing how much of what type drops on a floor
            switch (type2)
            {
                case AttackType.None:
                    break;
                case AttackType.Melee:
                    AddMeleeDrops(dt, currentFloor);
                    break;
                case AttackType.Ranged:
                    AddRangedDrops(dt, currentFloor);
                    break;
                case AttackType.Magic:
                    AddMagicDrops(dt, currentFloor);
                    break;
            }
            dt.AddDrop(ItemType.Potion, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, 1), 24, 5);
            return dt;
        }

        public DropTable CreateBossDropTable(GameEntity entity, FloorName floor)
        {
            DropTable dt = new DropTable(Game as KazgarsRevengeGame, entity, DropTable.GetBossAddItemLevel);
            switch (floor)
            {
                default:
                case FloorName.Dungeon:

                    break;
            }

            return dt;
        }
        
        #region Helpers
        
        private void AddCommonDrops(DropTable dt)
        {
            //TODO any common drops
        }

        public void AddMeleeDrops(DropTable dt, FloorName currentFloor)
        {
            dt.AddDrop(ItemType.Equippable, GetMeleeWeapon(currentFloor), 4);
            dt.AddDrop(ItemType.Equippable, GetMeleeHelm(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMeleeChest(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMeleeLegs(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMeleeFeet(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMeleeShoulders(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMeleeWrist(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMeleeBling(currentFloor), 3);

            dt.AddDrop(ItemType.Equippable, null, 75);
        }

        public void AddRangedDrops(DropTable dt, FloorName currentFloor)
        {
            dt.AddDrop(ItemType.Equippable, GetRangedWeapon(currentFloor), 4);
            dt.AddDrop(ItemType.Equippable, GetRangedHelm(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetRangedChest(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetRangedLegs(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetRangedFeet(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetRangedShoulders(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetRangedWrist(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetRangedBling(currentFloor), 3);

            dt.AddDrop(ItemType.Equippable, null, 75);
        }

        public void AddMagicDrops(DropTable dt, FloorName currentFloor)
        {
            dt.AddDrop(ItemType.Equippable, GetMagicWeapon(currentFloor), 4);
            dt.AddDrop(ItemType.Equippable, GetMagicHelm(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMagicChest(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMagicLegs(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMagicFeet(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMagicShoulders(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMagicWrist(currentFloor), 3);
            dt.AddDrop(ItemType.Equippable, GetMagicBling(currentFloor), 3);

            dt.AddDrop(ItemType.Equippable, null, 75);
        }

        #region Item IDs and Such
        string weaponDir = "Models\\Weapons\\";
        string armorDir = "Models\\Armor\\";
        #region Get Melee Items
        public Equippable GetMeleeWeapon(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[RandSingleton.U_Instance.Next(3001, 3003)];
            }
        }

        public Equippable GetMeleeHelm(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3003];
            }
        }

        public Equippable GetMeleeChest(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3004];
            }
        }

        public Equippable GetMeleeLegs(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3005];
            }
        }

        public Equippable GetMeleeFeet(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3006];
            }
        }

        public Equippable GetMeleeShoulders(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3007];
            }
        }

        public Equippable GetMeleeWrist(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3008];
            }
        }

        public Equippable GetMeleeBling(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3009];
            }
        }
        #endregion


        #region Get Ranged Items
        public Equippable GetRangedWeapon(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3102];
            }
        }

        public Equippable GetRangedHelm(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3103];
            }
        }

        public Equippable GetRangedChest(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3104];
            }
        }

        public Equippable GetRangedLegs(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3105];
            }
        }

        public Equippable GetRangedFeet(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3106];
            }
        }

        public Equippable GetRangedShoulders(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3107];
            }
        }

        public Equippable GetRangedWrist(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3108];
            }
        }

        public Equippable GetRangedBling(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3109];
            }
        }
        #endregion


        #region GetMagicItems
        public Equippable GetMagicWeapon(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3202];
            }
        }

        public Equippable GetMagicHelm(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3203];
            }
        }

        public Equippable GetMagicChest(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3204];
            }
        }

        public Equippable GetMagicLegs(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3205];
            }
        }

        public Equippable GetMagicFeet(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3206];
            }
        }

        public Equippable GetMagicShoulders(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3207];
            }
        }

        public Equippable GetMagicWrist(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3208];
            }
        }

        public Equippable GetMagicBling(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Equippable)AllItems[3209];
            }
        }
        #endregion

        private void InitializeItems()
        {
            /*
             * 9900-10000: test gear
             * 
             * 9001 - 9300: shared epic drops
             * 
             * 3001 - 3100: dungeon melee gear
             * 
             * 
             * 3101-3200: dungeon ranged gear
             * 
             * 
             * 3201-3300: dungeon magic gear
             * 
             * 
             */
            int id;
            #region Test Gear
            id = 9900;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_RHINO),
                "Boots of Testing",
                new Dictionary<StatType, float>() { { StatType.RunSpeed, .3f }, { StatType.Vitality, 3 } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            #endregion

            #region Shared Epic Drops
            //rhino set
            id = 9001;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_HEAD_RHINO),
                "Rhino Helm",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_head_rino"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 9002;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_CHEST_RHINO),
                "Rhino Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_chest_rino"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 9003;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_LEGS_RHINO),
                "Rhino Legguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_legs_rino"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 9004;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_RHINO),
                "Rhino Boots",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9005;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_SHOULDERS_RHINO),
                "Rhino Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_shoulders_rino"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 9006;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_WRIST_RHINO),
                "Rhino Wristguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_wrist_rino"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 9007;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_BLING),
                "Rhino Ring",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));

            //ranged set?
            id = 9011;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_HEAD_SKELETON),
                "Balking Helm",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_head_rino"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 9012;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_CHEST_SKELETON),
                "Balking Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_chest_rino"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 9013;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_LEGS_SKELETON),
                "Balking Legguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_legs_rino"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 9014;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_FEET_SKELETON),
                "Balking Boots",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9015;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_SHOULDERS_SKELETON),
                "Balking Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_shoulders_rino"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 9016;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_WRIST_SKELETON),
                "Balking Wristguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_wrist_rino"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 9017;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_BLING),
                "Balking Ring",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));


            //cardinal set
            id = 9021;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_HEAD_RHINO),
                "Cardinal Helm",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_head_cardinal"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 9022;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_CHEST_RHINO),
                "Cardinal Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_chest_cardinal"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 9023;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_LEGS_RHINO),
                "Cardinal Legguards",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_legs_cardinal"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 9024;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_RHINO),
                "Cardinal Boots",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_boots_cardinal"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9025;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_SHOULDERS_RHINO),
                "Cardinal Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_shoulders_cardinal"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 9026;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_WRIST_RHINO),
                "Cardinal Wristguards",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_wrist_cardinal"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 9027;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_BLING),
                "Cardinal Ring",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));
            #endregion

            #region Dungeon Level
            #region Melee Gear
            id = 3001;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.BRUTISH_AXE),
                "Brutish Axe",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetUnanimatedModel(weaponDir + "axe"),
                AttackType.Melee,
                true,
                id));
            id = 3002;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_SWORD),
                "Typical Sword",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetUnanimatedModel(weaponDir + "sword01"),
                AttackType.Melee,
                false,
                id));
            id = 3003;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_HEAD_RHINO),
                "Underwhelming Helm",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_head_rino"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 3004;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_CHEST_RHINO),
                "Underwhelming Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_chest_rino"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 3005;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_LEGS_RHINO),
                "Underwhelming Legguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_legs_rino"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 3006;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_RHINO),
                "Underwhelming Boots",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 3007;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_SHOULDERS_RHINO),
                "Underwhelming Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_shoulders_rino"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 3008;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_WRIST_RHINO),
                "Underwhelming Wristguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_wrist_rino"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 3009;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_BLING),
                "Underwhelming Ring",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));
            #endregion

            #region Ranged Gear
            id = 3102;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_BOW),
                "Skeleton Crossbow",
                new Dictionary<StatType, float>() { { StatType.Agility, .75f }, { StatType.CritChance, .25f } },
                GetUnanimatedModel(weaponDir + "crossbow"),
                AttackType.Ranged,
                false,
                id));
            id = 3103;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_HEAD_SKELETON),
                "Balking Helm",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "Barrel\\armor_head_barrel"),
                GearSlot.Head,
                GearSlot.None, 
                id));
            id = 3104;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_CHEST_SKELETON),
                "Balking Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "Barrel\\armor_chest_barrel"),
                GearSlot.Chest,
                GearSlot.None, 
                id));
            id = 3105;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_LEGS_SKELETON),
                "Balking Legguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "Barrel\\armor_legs_barrel"),
                GearSlot.Legs,
                GearSlot.None, 
                id));
            id = 3106;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_FEET_SKELETON),
                "Balking Boots",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "Barrel\\armor_boots_barrel"),
                GearSlot.Feet,
                GearSlot.None, 
                id));
            id = 3107;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_SHOULDERS_SKELETON),
                "Balking Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "Barrel\\armor_shoulders_barrel"),
                GearSlot.Shoulders,
                GearSlot.None, 
                id));
            id = 3108;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_WRIST_SKELETON),
                "Balking Wristguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "Barrel\\armor_wrist_barrel"),
                GearSlot.Wrist,
                GearSlot.None, 
                id));
            id = 3109;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_BLING),
                "Balking Ring",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                null,
                GearSlot.Bling,
                GearSlot.None, 
                id));
            #endregion

            #region Magic Gear
            id = 3202;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_STAFF),
                "Typical Staff",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, {StatType.CooldownReduction, .5f} },
                GetUnanimatedModel(weaponDir + "staff"),
                AttackType.Magic,
                true,
                id));
            id = 3203;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_HEAD_SMART),
                "Depreciating Helm",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_head_cardinal"),
                GearSlot.Head,
                GearSlot.None, 
                id));
            id = 3204;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_CHEST_SMART),
                "Depreciating Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_chest_cardinal"),
                GearSlot.Chest,
                GearSlot.None, 
                id));
            id = 3205;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_LEGS_SMART),
                "Depreciating Legguards",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_legs_cardinal"),
                GearSlot.Legs,
                GearSlot.None, 
                id));
            id = 3206;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_FEET_SMART),
                "Depreciating Boots",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_boots_cardinal"),
                GearSlot.Feet,
                GearSlot.None, 
                id));
            id = 3207;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_SHOULDERS_SMART),
                "Depreciating Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_shoulders_cardinal"),
                GearSlot.Shoulders,
                GearSlot.None, 
                id));
            id = 3208;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_WRIST_SMART),
                "Depreciating Wristguards",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_wrist_cardinal"),
                GearSlot.Wrist,
                GearSlot.None, 
                id));
            id = 3209;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_BLING),
                "Depreciating Ring",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                null,
                GearSlot.Bling,
                GearSlot.None, 
                id));
            #endregion
            #endregion
        }

        #endregion

        #endregion

        #region Icons
        Dictionary<string, Texture2D> equippableIcons = new Dictionary<string, Texture2D>();
        private Texture2D GetIcon(string name)
        {
            string texName = "Textures\\UI\\";
            switch (name)
            {
                    // TODO give the texName values meaningful names in the TextureStrings file
                case "sword":
                    texName += "Abilities\\HS";
                    break;
                case "potion":
                    texName += "Items\\HP";
                    break;
                case "bow":
                    texName += "Abilities\\LW";
                    break;
                case "helm":
                    texName += "Frames\\helmetIcon";
                    break;
                default:
                    texName += "Abilities\\I2";
                    break;
            }

            Texture2D retTex;
            if (!equippableIcons.TryGetValue(texName, out retTex))
            {
                retTex = Texture2DUtil.Instance.GetTexture(texName);
                equippableIcons.Add(texName, retTex);
            }
            return retTex;
        }

        private Texture2D GetGoldIcon(int quantity)
        {
            Texture2D goldIcon;
            if (quantity <= 10)
            {
                goldIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Coins.FEW);
            }
            else if (quantity <= 30)
            {
                goldIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Coins.SOME);
            }
            else
            {
                goldIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Coins.LOTS);
            }
            return goldIcon;
        }
        #endregion

        #endregion
    }
}
