using System;
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

        #region Loot

        private IList<Item> GetLootFor(FloorName floor, GameEntity deadEntity)
        {
            DropTable table = deadEntity.GetComponent(typeof(DropTable)) as DropTable;
            if (table == null)
            {
                throw new Exception("UH OH SOMEONE DOESN'T HAVE A DROP TABLE!: " + deadEntity.Name);
            }

            // TODO we need to pass in the entity that killed it so we can properly
            // apply some of the boosts. Ie Potion of Luck
            return table.GetDrops(floor, null);
        }
        
        private Item GetNormalGear(int itemBase)
        {
            return GetSword();
        }

        private Item GetEliteGear(int itemBase)
        {
            return GetSword();
        }

        private List<Item> GetBossGear(int level)
        {
            List<Item> retItems = new List<Item>();

            retItems.Add(GetSword());

            return retItems;
        }

        #region Helpers
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
                                retItems[j] = new Item(ItemType.Gold, GetGoldIcon(quantity), "gold", quantity);
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
        
        private Dictionary<StatType, float> GetStats(string itemName)
        {
            Dictionary<StatType, float> itemStats = new Dictionary<StatType, float>();
            switch (itemName)
            {
                case "sword":
                    itemStats.Add(StatType.Strength, 5);
                    break;
                default:
                    itemStats.Add(StatType.Strength, 5);
                    break;
            }
            return itemStats;
        }

        /// <summary>
        /// generates new sword with random stats
        /// </summary>
        public Equippable GenerateSword()
        {
            return GetSword();
        }

        string attachDir = "Models\\Attachables\\";
        /// <summary>
        /// loads a saved sword with predefined stats
        /// </summary>
        public Equippable GetSword()
        {
            return new Weapon(GetIcon("sword"), "sword01", GetStats("sword"), GetUnanimatedModel(attachDir + "sword01"), AttackType.Melee, false);
        }

        public Equippable GetBaseSword()
        {
            return new Weapon(GetIcon("sword"), "sword01", null, GetUnanimatedModel(attachDir + "sword01"), AttackType.Melee, false);
        }

        public Equippable GenerateBow()
        {
            return GetBow();
        }

        public Equippable GetBow()
        {
            return new Weapon(GetIcon("bow"), "bow", GetStats("bow"), GetUnanimatedModel(attachDir + "bow01"), AttackType.Ranged, true);
        }

        public Equippable GetHelm()
        {
            return new Equippable(GetIcon("helm"), "armor", GetStats("bow"), GetAnimatedModel("Models\\Armor\\armor_head_rino"), GearSlot.Head, GearSlot.None);
        }
        public Equippable GetShoulders()
        {
            return new Equippable(GetIcon("shoulders"), "armor", GetStats("bow"), GetAnimatedModel("Models\\Armor\\armor_shoulders_rino"), GearSlot.Shoulders, GearSlot.None);
        }
        public Equippable GetWrist()
        {
            return new Equippable(GetIcon("wrist"), "armor", GetStats("bow"), GetAnimatedModel("Models\\Armor\\armor_wrist_rino"), GearSlot.Wrist, GearSlot.None);
        }
        public Equippable GetLegs()
        {
            return new Equippable(GetIcon("legs"), "armor", GetStats("bow"), GetAnimatedModel("Models\\Armor\\armor_legs_rino"), GearSlot.Legs, GearSlot.None);
        }
        public Equippable GetBoots()
        {
            return new Equippable(GetIcon("boots"), "armor", GetStats("bow"), GetAnimatedModel("Models\\Armor\\armor_boots_rino"), GearSlot.Feet, GearSlot.None);
        }
        public Equippable GetChest()
        {
            return new Equippable(GetIcon("chest"), "armor", GetStats("bow"), GetAnimatedModel("Models\\Armor\\armor_chest_rino"), GearSlot.Chest, GearSlot.None);
        }


        #endregion

        #region Icons
        Dictionary<string, Texture2D> equippableIcons = new Dictionary<string, Texture2D>();
        private Texture2D GetIcon(string name)
        {
            string texName = "Textures\\UI\\";
            switch (name)
            {
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
                retTex = Game.Content.Load<Texture2D>(texName);
                equippableIcons.Add(texName, retTex);
            }
            return retTex;
        }

        private Texture2D GetGoldIcon(int quantity)
        {
            Texture2D goldIcon;
            if (quantity <= 10)
            {
                goldIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\gold1");
            }
            else if (quantity <= 30)
            {
                goldIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\gold2");
            }
            else
            {
                goldIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\gold3");
            }
            return goldIcon;
        }
        #endregion

        #endregion
    }
}
