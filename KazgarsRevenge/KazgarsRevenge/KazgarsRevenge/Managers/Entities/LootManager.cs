using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.MathExtensions;
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
    /// <summary>
    /// Manages loot souls and loot generation
    /// </summary>
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

        public void ClearLevel()
        {
            foreach (GameEntity ent in lootSouls)
            {
                ent.KillEntity();
            }
            lootSouls.Clear();
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
            GameEntity lootSoul = new GameEntity("loot", FactionType.Neutral, EntityType.Interactive);
            float size = 3 + totalSouls;

            Entity lootPhysicalData = new Box(position, size + 10, size, size + 10, 1);
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

            lootSoul.AddComponent(typeof(IPlayerInteractiveController), lootController);
            genComponentManager.AddComponent(lootController);

            lootSoul.AddComponent(typeof(BlobShadowDecal), lootShadow);
            billboardManager.AddComponent(lootShadow);

            SpawnSoulPoof(position);
        }

        public void CreateTreasureGopher(Vector3 position)
        {
            position.Y = 4;
            GameEntity entity = new GameEntity("treasure gopher", FactionType.Neutral, EntityType.Interactive);

            Entity data = new Box(position, 15, 10, 15, 5);
            data.CollisionInformation.CollisionRules.Group = mainGame.UntouchableCollisionGroup;
            data.IsAffectedByGravity = false;
            data.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            entity.AddSharedData(typeof(Entity), data);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            Model model = GetAnimatedModel("Models\\Gopher\\g_idle");
            AnimationPlayer anims = new AnimationPlayer(model.Tag as SkinningData);
            entity.AddSharedData(typeof(AnimationPlayer), anims);

            AnimatedModelComponent graphics = new AnimatedModelComponent(mainGame, entity, model, 10, Vector3.Zero);
            graphics.AddEmitter(typeof(GopherGlowSystem), "glow", 100, 0, Vector3.Zero);
            entity.AddComponent(typeof(AnimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            BlobShadowDecal shadow = new BlobShadowDecal(mainGame, entity, 15);
            entity.AddComponent(typeof(BlobShadowDecal), shadow);
            billboardManager.AddComponent(shadow);

            TreasureGopherController controller = new TreasureGopherController(mainGame, entity, GetTreasureGopherLoot(levelManager.currentLevel.currentFloor, entity));
            entity.AddComponent(typeof(IPlayerInteractiveController), controller);
            genComponentManager.AddComponent(controller);

            lootSouls.Add(entity);

        }

        public void SpawnSoulPoof(Vector3 position)
        {
            for (int i = 0; i < 20; ++i)
            {
                particles.GetSystem(typeof(SoulCreationParticleSystem)).AddParticle(position, Vector3.Zero);
            }
        }


        /// <summary>
        /// Combines the loot contained in two loot souls into one list
        /// </summary>
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





        /*
         * methods for generating loot and droptables
         */

        private Dictionary<int, Item> AllItems = new Dictionary<int, Item>();
        public Item GetItem(int id)
        {
            return (Item)AllItems[id].Clone();
        }

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

        private List<Item> GetTreasureGopherLoot(FloorName floor, GameEntity gopherEntity)
        {
            List<Item> retList = new List<Item>();
            switch (floor)
            {
                default:
                case FloorName.Dungeon:
                    Item g = new Item(ItemType.Gold, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Coins.FEW), "gold", (int)floor * 50 + 5, 0);
                    retList.Add(g);
                    Potion pot = GetItem(4) as Potion;
                    retList.Add(pot);
                    Equippable e = AllItems[RandSingleton.U_Instance.Next(9001, 9022)] as Equippable;
                    e.SetStats(GearQuality.Epic, RandSingleton.U_Instance.Next(1, 10) + (int)floor);
                    retList.Add(e);
                    break;
            }
            return retList;
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
            
            //25% chance to drop (up to 5) health potions
            dt.AddDrop(ItemType.Potion, GetItem(1), 25, 5);
            //10% chance to drop (up to 2) portal potions
            dt.AddDrop(ItemType.Potion, GetItem(6), 5, 2);
            dt.AddDrop(ItemType.Potion, null, 70);

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

        public List<Item> GetShopItems()
        {
            FloorName floor = levelManager.currentLevel.currentFloor;
            List<Item> retItems = new List<Item>();

            Weapon sword = GetItem(3002) as Weapon;
            sword.SetStats(GearQuality.Standard, 1);
            retItems.Add(sword);

            Potion p = GetItem(1) as Potion;
            p.AddQuantity(5);
            retItems.Add(p);

            return retItems;
        }
        
        #region Helpers
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

            dt.AddDrop(ItemType.Essence, GetMeleeEssence(currentFloor), 5);
            dt.AddDrop(ItemType.Essence, null, 95);
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

            dt.AddDrop(ItemType.Essence, GetRangedEssence(currentFloor), 5);
            dt.AddDrop(ItemType.Essence, null, 95);
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

            dt.AddDrop(ItemType.Essence, GetMagicEssence(currentFloor), 5);
            dt.AddDrop(ItemType.Essence, null, 95);
        }

        public void AddEpicDrops(DropTable dt)
        {
            int numEpics = 21;
            for (int i = 1; i <= numEpics; ++i)
            {
                int id = i + 9000;
                dt.AddDrop(ItemType.Equippable, (Equippable)AllItems[id], 100 / numEpics);
            }
        }

        public void AddDungeonBossDrops(DropTable dt)
        {
            //TODO
        }
        #region Item ID helpers
        string weaponDir = "Models\\Weapons\\";
        string armorDir = "Models\\Armor\\";

        #region Get Melee Items
        public Essence GetMeleeEssence(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Essence)AllItems[901];
                case FloorName.Library:
                    return (Essence)AllItems[904];
                case FloorName.TortureChamber:
                    return (Essence)AllItems[907];
                case FloorName.Lab:
                    return (Essence)AllItems[910];
                case FloorName.GrandHall:
                    return (Essence)AllItems[913];
            }
        }

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
        public Essence GetRangedEssence(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Essence)AllItems[902];
                case FloorName.Library:
                    return (Essence)AllItems[905];
                case FloorName.TortureChamber:
                    return (Essence)AllItems[908];
                case FloorName.Lab:
                    return (Essence)AllItems[911];
                case FloorName.GrandHall:
                    return (Essence)AllItems[914];
            }
        }

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
        public Essence GetMagicEssence(FloorName currentFloor)
        {
            switch (currentFloor)
            {
                default:
                case FloorName.Dungeon:
                    return (Essence)AllItems[903];
                case FloorName.Library:
                    return (Essence)AllItems[906];
                case FloorName.TortureChamber:
                    return (Essence)AllItems[909];
                case FloorName.Lab:
                    return (Essence)AllItems[912];
                case FloorName.GrandHall:
                    return (Essence)AllItems[915];
            }
        }

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

        /// <summary>
        /// Creates an initial copy of every item and adds it to AllItems
        /// </summary>
        private void InitializeItems()
        {
            int id;
            #region Potions
            /* Potion IDs:
             * 1: health pot
             * 2: super health pot
             * 3: insta health pot
             * 4: luck potion
             * 5: invis potion
             * 6: portal potion
             */
            id = 1;
            AllItems.Add(id, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, id));
            id = 2;
            AllItems.Add(id, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, id));
            id = 3;
            AllItems.Add(id, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, id));
            id = 4;
            AllItems.Add(id, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, id));
            id = 5;
            AllItems.Add(id, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, id));
            id = 6;
            AllItems.Add(id, new Potion(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH), 0, id));
            #endregion


            #region Essence
            /*
             * Essence ids: 901-915
             * 
             * ids progress from melee-ranged-magic, repeat
             * 
             * 901-903: lesser
             * 904-906: normal
             * 907-909: greater
             * 910-912: potent
             * 913-915: illustrious
             * 
             */
            id = 901;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.LesserEssence), id));
            id = 902;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.LesserEssence), id));
            id = 903;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.LesserEssence), id));
            id = 904;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.EssenceyEssence), id));
            id = 905;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.EssenceyEssence), id));
            id = 906;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.EssenceyEssence), id));
            id = 907;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.GreaterEssence), id));
            id = 908;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.GreaterEssence), id));
            id = 909;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.GreaterEssence), id));
            id = 910;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.PotentEssence), id));
            id = 911;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.PotentEssence), id));
            id = 912;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.PotentEssence), id));
            id = 913;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.IllustriousEssence), id));
            id = 914;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.IllustriousEssence), id));
            id = 915;
            AllItems.Add(id, new Essence(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Essence.IllustriousEssence), id));
            #endregion


            #region Equippables
            /* Gear IDs:
             * 
             * 9900-10000: test gear
             * 
             * 9001 - 9300: shared epic drops
             * 9301 - 3400: mimic loot
             * 
             * 3001 - 3100: dungeon melee gear
             * 3101-3200: dungeon ranged gear
             * 3201-3300: dungeon magic gear
             * 3301-3350: dungeon Boss gear
             * 
             */
            #region Test Gear
            id = 9900;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_GENERIC),
                "Boots of Testing",
                new Dictionary<StatType, float>() { { StatType.RunSpeed, 2f }, { StatType.Vitality, 3 } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9901;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_STAFF),
                "Staff of Testing",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetUnanimatedModel(weaponDir + "staff"),
                AttackType.Magic,
                true,
                id));
            id = 9902;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_STAFF),
                "Claymore of Testing",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .5f } },
                GetUnanimatedModel(weaponDir + "ice_sword"),
                AttackType.Melee,
                false,
                id));
            #endregion


            #region Shared Epic Drops
            //rhino set
            id = 9001;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_HEAD_GENERIC),
                "Rhino Helm",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_head_rino"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 9002;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_CHEST_GENERIC),
                "Rhino Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_chest_rino"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 9003;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_LEGS_GENERIC),
                "Rhino Legguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_legs_rino"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 9004;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_GENERIC),
                "Rhino Boots",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9005;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_SHOULDERS_GENERIC),
                "Rhino Pauldrons",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_shoulders_rino"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 9006;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_WRIST_GENERIC),
                "Rhino Wristguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_wrist_rino"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 9007;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_BLING_GENERIC),
                "Rhino Ring",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));

            //ranged set?
            id = 9008;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_HEAD_GENERIC),
                "Skeleton Helm",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_head_rino"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 9009;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_CHEST_GENERIC),
                "Skeleton Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_chest_rino"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 9010;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_LEGS_GENERIC),
                "Skeleton Legguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_legs_rino"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 9011;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_FEET_GENERIC),
                "Skeleton Boots",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9012;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_SHOULDERS_GENERIC),
                "Skeleton Pauldrons",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_shoulders_rino"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 9013;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_WRIST_GENERIC),
                "Skeleton Bracers",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                GetAnimatedModel(armorDir + "armor_wrist_rino"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 9014;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_BLING_GENERIC),
                "Skeleton Ring",
                new Dictionary<StatType, float>() { { StatType.Agility, .25f }, { StatType.Vitality, .25f }, { StatType.AttackSpeed, .5f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));


            //cardinal set
            id = 9015;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_HEAD_GENERIC),
                "Cardinal Helm",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_head_cardinal"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 9016;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_CHEST_GENERIC),
                "Cardinal Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_chest_cardinal"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 9017;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_LEGS_GENERIC),
                "Cardinal Legguards",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_legs_cardinal"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 9018;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_GENERIC),
                "Cardinal Boots",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_boots_cardinal"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 9019;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_SHOULDERS_GENERIC),
                "Cardinal Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_shoulders_cardinal"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 9020;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_WRIST_GENERIC),
                "Cardinal Wristguards",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_wrist_cardinal"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 9021;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_BLING_GENERIC),
                "Cardinal Ring",
                new Dictionary<StatType, float>() { { StatType.Intellect, .25f }, { StatType.Vitality, .75f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));
            #endregion

            #region Mimic Loot
            id = 9301;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.KEY_SWORD),
                "Key Shiv",
                new Dictionary<StatType, float>() { { StatType.Strength, .75f }, { StatType.AttackSpeed, .1f }, { StatType.CritChance, .15f } },
                GetUnanimatedModel(weaponDir + "key_blade"),
                AttackType.Melee,
                false,
                id));
            #endregion

            #region Dungeon Level
            #region Melee Gear
            id = 3001;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.AXE_01),
                "Brutish Axe",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetUnanimatedModel(weaponDir + "axe"),
                AttackType.Melee,
                true,
                id));
            id = 3002;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_SWORD),
                "Warrior Sword",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetUnanimatedModel(weaponDir + "sword01"),
                AttackType.Melee,
                false,
                id));
            id = 3003;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_HEAD_GENERIC),
                "Warrior Helm",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_head_rino"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 3004;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_CHEST_GENERIC),
                "Warrior Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_chest_rino"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 3005;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_LEGS_GENERIC),
                "Warrior Legguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_legs_rino"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 3006;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_FEET_GENERIC),
                "Warrior Boots",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_boots_rino"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 3007;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_SHOULDERS_GENERIC),
                "Warrior Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_shoulders_rino"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 3008;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_WRIST_GENERIC),
                "Warrior Wristguards",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                GetAnimatedModel(armorDir + "armor_wrist_rino"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 3009;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MELEE_BLING_GENERIC),
                "Warrior Ring",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .75f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));
            #endregion

            #region Ranged Gear
            id = 3102;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_BOW),
                "Ranger Crossbow",
                new Dictionary<StatType, float>() { { StatType.Agility, .75f }, { StatType.CritChance, .25f } },
                GetUnanimatedModel(weaponDir + "crossbow"),
                AttackType.Ranged,
                false,
                id));
            id = 3103;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_HEAD_GENERIC),
                "Ranger Helm",
                new Dictionary<StatType, float>() { { StatType.Agility, .75f }, { StatType.Vitality, .15f }, {StatType.Armor, .1f} },
                GetAnimatedModel(armorDir + "Bowser\\armor_head_bowser"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 3104;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_CHEST_GENERIC),
                "Ranger Chestpiece",
                new Dictionary<StatType, float>() { { StatType.Agility, .5f }, { StatType.Vitality, .25f }, {StatType.Armor, .25f} },
                GetAnimatedModel(armorDir + "Bowser\\armor_chest_bowser"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 3105;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_LEGS_GENERIC),
                "Ranger Legguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .5f }, { StatType.Vitality, .4f }, {StatType.Armor, .1f} },
                GetAnimatedModel(armorDir + "Bowser\\armor_legs_bowser"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 3106;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_FEET_GENERIC),
                "Ranger Boots",
                new Dictionary<StatType, float>() { { StatType.Agility, .6f }, { StatType.Vitality, .3f }, { StatType.RunSpeed, .1f } },
                GetAnimatedModel(armorDir + "Bowser\\armor_boots_bowser"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 3107;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_SHOULDERS_GENERIC),
                "Ranger Shoulderpads",
                new Dictionary<StatType, float>() { { StatType.Agility, 1f } },
                GetAnimatedModel(armorDir + "Bowser\\armor_shoulders_bowser"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 3108;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_WRIST_GENERIC),
                "Ranger Wristguards",
                new Dictionary<StatType, float>() { { StatType.Agility, .6f }, { StatType.Vitality, .2f }, {StatType.Armor, .2f} },
                GetAnimatedModel(armorDir + "Bowser\\armor_wrist_bowser"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 3109;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.RANGED_BLING_GENERIC),
                "Ranger Ring",
                new Dictionary<StatType, float>() { { StatType.Agility, .8f }, { StatType.CooldownReduction, .2f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));
            #endregion

            #region Magic Gear
            id = 3202;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_STAFF),
                "Wizard Staff",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetUnanimatedModel(weaponDir + "staff"),
                AttackType.Magic,
                true,
                id));
            id = 3203;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_HEAD_GENERIC),
                "Wizard Helm",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_head_cardinal"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 3204;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_CHEST_GENERIC),
                "Wizard Vest",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_chest_cardinal"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 3205;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_LEGS_GENERIC),
                "Wizard Kilt",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_legs_cardinal"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 3206;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_FEET_GENERIC),
                "Wizard Boots",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_boots_cardinal"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 3207;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_SHOULDERS_GENERIC),
                "Wizard Shoulders",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_shoulders_cardinal"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 3208;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_WRIST_GENERIC),
                "Wizard Bracers",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                GetAnimatedModel(armorDir + "Cardinal\\armor_wrist_cardinal"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 3209;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.MAGIC_BLING_GENERIC),
                "Wizard Ring",
                new Dictionary<StatType, float>() { { StatType.Intellect, .65f }, { StatType.Vitality, .25f }, { StatType.CooldownReduction, .1f } },
                null,
                GearSlot.Bling,
                GearSlot.None,
                id));
            #endregion

            #region Boss Gear
            id = 3301;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.DRAGON_HEAD),
                "Dragonhide Coif",
                new Dictionary<StatType, float>() { { StatType.Armor, .35f }, { StatType.Vitality, .45f }, { StatType.CooldownReduction, .2f } },
                GetAnimatedModel(armorDir + "Dragon\\armor_head_dragon"),
                GearSlot.Head,
                GearSlot.None,
                id));
            id = 3302;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.DRAGON_CHEST),
                "Dragonhide Hauberk",
                new Dictionary<StatType, float>() { { StatType.Armor, .7f }, { StatType.Vitality, .3f } },
                GetAnimatedModel(armorDir + "Dragon\\armor_chest_dragon"),
                GearSlot.Chest,
                GearSlot.None,
                id));
            id = 3303;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.DRAGON_BOOTS),
                "Dragonhide Boots",
                new Dictionary<StatType, float>() { { StatType.Armor, .4f }, { StatType.Vitality, .4f }, {StatType.RunSpeed, .2f } },
                GetAnimatedModel(armorDir + "Dragon\\armor_boots_dragon"),
                GearSlot.Feet,
                GearSlot.None,
                id));
            id = 3304;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.DRAGON_LEGS),
                "Dragonhide Pants",
                new Dictionary<StatType, float>() { { StatType.Armor, .25f }, { StatType.Vitality, .55f }, {StatType.CooldownReduction, .2f } },
                GetAnimatedModel(armorDir + "Dragon\\armor_legs_dragon"),
                GearSlot.Legs,
                GearSlot.None,
                id));
            id = 3305;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.DRAGON_WRIST),
                "Dragonhide Wrist",
                new Dictionary<StatType, float>() { { StatType.Armor, .4f }, { StatType.Vitality, .4f }, {StatType.AttackSpeed, .2f} },
                GetAnimatedModel(armorDir + "Dragon\\armor_wrist_dragon"),
                GearSlot.Wrist,
                GearSlot.None,
                id));
            id = 3306;
            AllItems.Add(id, new Equippable(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Armor.DRAGON_SHOULDERS),
                "Dragonhide Pauldrons",
                new Dictionary<StatType, float>() { { StatType.Strength, .25f }, { StatType.Vitality, .55f }, {StatType.AttackSpeed, .2f } },
                GetAnimatedModel(armorDir + "Dragon\\armor_shoulders_dragon"),
                GearSlot.Shoulders,
                GearSlot.None,
                id));
            id = 3307;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.AXE_01),
                "Dragon Limb",
                new Dictionary<StatType, float>() { { StatType.Strength, .75f }, { StatType.AttackSpeed, .1f }, {StatType.CritChance, .15f} },
                GetUnanimatedModel(weaponDir + "dragon_claw"),
                AttackType.Melee,
                false,
                id));
            id = 3308;
            AllItems.Add(id, new Weapon(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Weapons.GENERIC_BOW),
                "Dragon Bow",
                new Dictionary<StatType, float>() { { StatType.Agility, .75f }, { StatType.AttackSpeed, .1f }, { StatType.CritChance, .15f } },
                GetUnanimatedModel(weaponDir + "dragon_bow"),
                AttackType.Melee,
                true,
                id));
            #endregion
            #endregion

            #endregion
        }
    }
}
