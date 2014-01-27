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
    class LootManager : EntityManager
    {
        const float SOULS_PER_INCREASE = 1.0f;

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

        public void CreateLootSoul(Vector3 position, EntityType type)
        {
            CreateLootSoul(position, GetLootFor(type), 1);
        }

        public void CreateLootSoul(Vector3 position, List<Item> first, List<Item> second, int totalSouls)
        {
            CreateLootSoul(position, CombineLoot(first, second), totalSouls);
        }

        public void CreateLootSoul(Vector3 position, List<Item> containedLoot, int totalSouls)
        {
            position.Y = 10;
            GameEntity lootSoul = new GameEntity("loot", FactionType.Neutral, EntityType.Misc);
            float size = 10;// 3 + (float)Math.Floor((float)totalSouls / SOULS_PER_INCREASE);

            Entity lootPhysicalData = new Box(position, size, size, size, 1);
            lootPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.LootCollisionGroup;
            lootPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            lootPhysicalData.IsAffectedByGravity = false;
            lootSoul.AddSharedData(typeof(Entity), lootPhysicalData);

            Model lootModel = GetAnimatedModel("Models\\soul_idle");
            AnimationPlayer lootAnimations = new AnimationPlayer(lootModel.Tag as SkinningData);
            lootSoul.AddSharedData(typeof(AnimationPlayer), lootAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            lootSoul.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            PhysicsComponent lootPhysics = new PhysicsComponent(mainGame, lootSoul);
            AnimatedModelComponent lootGraphics = new AnimatedModelComponent(mainGame, lootSoul, lootModel, new Vector3(size), Vector3.Zero);
            LootSoulController lootController = new LootSoulController(mainGame, lootSoul, 10, containedLoot, totalSouls);
            BlobShadowDecal lootShadow = new BlobShadowDecal(mainGame, lootSoul, size);
            EmitterComponent soulEmitters = new EmitterComponent(mainGame, lootSoul);
            soulEmitters.AddEmitter(typeof(SoulTrailParticleSystem), 8, Math.Max(1, (int)size / 2), Vector3.Up * 5);

            lootSoul.AddComponent(typeof(PhysicsComponent), lootPhysics);
            genComponentManager.AddComponent(lootPhysics);

            lootSoul.AddComponent(typeof(UnanimatedModelComponent), lootGraphics);
            modelManager.AddComponent(lootGraphics);

            lootSoul.AddComponent(typeof(AIComponent), lootController);
            genComponentManager.AddComponent(lootController);

            lootSoul.AddComponent(typeof(BlobShadowDecal), lootShadow);
            decalManager.AddBlobShadow(lootShadow);

            lootSoul.AddComponent(typeof(EmitterComponent), soulEmitters);
            genComponentManager.AddComponent(soulEmitters);

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

        Random rand = new Random();
        private List<Item> GetLootFor(EntityType type)
        {
            List<Item> retItems = new List<Item>();
            int level = (int)levelManager.CurrentFloor;

            //get gold
            int goldQuantity = 5 * level * level;
            int extra = rand.Next(0, level * level + 1);
            if (diceRoll(1, 2) == 1)
            {
                goldQuantity += extra;
            }
            else
            {
                goldQuantity -= extra;
            }

            if (diceRoll(1, 20) <= 3)
            {
                //15% chance to triple gold drop
                goldQuantity *= 3;
            }

            retItems.Add(new Item(ItemType.Gold, GetGoldIcon(goldQuantity), "gold", goldQuantity));

            //decide on gear
            int baseItemLevel = (level - 1) * 10;
            switch (type)
            {
                case EntityType.NormalEnemy:
                    if (diceRoll(1, 6) == 1)
                    {
                        retItems.Add(GetNormalGear(baseItemLevel));
                    }
                    break;
                case EntityType.EliteEnemy:
                    if (diceRoll(1, 12) < 3)
                    {
                        retItems.Add(GetEliteGear(baseItemLevel));
                    }
                    break;
                case EntityType.Boss:
                    List<Item> bossGear = GetBossGear(baseItemLevel);
                    for (int i = 0; i < bossGear.Count; ++i)
                    {
                        retItems.Add(bossGear[i]);
                    }
                    break;
            }

            //decide on consumable (37.5% chance to drop some potion or other)
            if (diceRoll(1, 8) < 4)
            {
                //small chance to drop rarer potion
                if (diceRoll(1, 4) == 3)
                {
                    retItems.Add(GetRarePotion());
                }
                else
                {
                    retItems.Add(GetPotion());
                }
            }

            //decide on essence

            //decide on recipe


            return retItems;
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

        private Item GetPotion()
        {
            if (diceRoll(1, 4) == 1)
            {
                return new Item(ItemType.Potion, GetIcon("potion"), "Super Health Potion", diceRoll(1, 2));
            }
            return new Item(ItemType.Potion, GetIcon("potion"), "Health Potion", diceRoll(1, 2));
        }

        private Item GetRarePotion()
        {
            if (diceRoll(1, 12) == 2)
            {
                return new Item(ItemType.Potion, GetIcon("potion"), "Potion of Luck", 1);
            }
            else
            {
                return new Item(ItemType.Potion, GetIcon("potion"), "Potion of Instant Health", 1);
            }
        }

        #region Helpers
        /// <summary>
        /// returns (rolls)d(sides)
        /// diceRoll(3, 6) is the result of 3d6
        /// </summary>
        /// <param name="rolls">how many dice to roll</param>
        /// <param name="sides">how many sides the dice have</param>
        /// <returns>the combines result</returns>
        public int diceRoll(int rolls, int sides)
        {
            int ret = 0;
            for (int i = 0; i < rolls; ++i)
            {
                ret += rand.Next(1, sides + 1);
            }
            return ret;
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
            return new Weapon(GetIcon("sword"), "sword01", GetStats("sword"), GetUnanimatedModel(attachDir + "sword01"), AttackType.Melle, false);
        }

        public Equippable GenerateBow()
        {
            return GetBow();
        }

        public Equippable GetBow()
        {
            return new Weapon(GetIcon("bow"), "bow", GetStats("bow"), GetUnanimatedModel(attachDir + "bow01"), AttackType.Ranged, false);
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
                default:
                    texName += "Abilities\\HS";
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
                goldIcon = Game.Content.Load<Texture2D>("Textures\\gold1");
            }
            else if (quantity <= 30)
            {
                goldIcon = Game.Content.Load<Texture2D>("Textures\\gold2");
            }
            else
            {
                goldIcon = Game.Content.Load<Texture2D>("Textures\\gold3");
            }
            return goldIcon;
        }
        #endregion

        #endregion
    }
}
