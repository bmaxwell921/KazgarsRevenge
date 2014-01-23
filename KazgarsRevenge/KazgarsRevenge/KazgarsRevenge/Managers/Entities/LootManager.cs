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

        public void CreateLootSoul(Vector3 position, string entityName)
        {
            CreateLootSoul(position, GetLootFor(entityName), 1);
        }

        public void CreateLootSoul(Vector3 position, List<Item> first, List<Item> second, int totalSouls)
        {
            CreateLootSoul(position, CombineLoot(first, second), totalSouls);
        }

        public void CreateLootSoul(Vector3 position, List<Item> containedLoot, int totalSouls)
        {
            position.Y = 10;
            GameEntity lootSoul = new GameEntity("loot", FactionType.Neutral);
            float size = 3 + (float)Math.Floor((float)totalSouls / SOULS_PER_INCREASE);

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
            AnimatedModelComponent lootGraphics = new AnimatedModelComponent(mainGame, lootSoul, lootModel, new Vector3(3 + (float)Math.Floor((float)totalSouls / SOULS_PER_INCREASE)), Vector3.Zero);
            LootSoulController lootController = new LootSoulController(mainGame, lootSoul, 10, containedLoot, totalSouls);
            BlobShadowDecal lootShadow = new BlobShadowDecal(mainGame, lootSoul, size);
            EmitterComponent soulEmitters = new EmitterComponent(mainGame, lootSoul);
            soulEmitters.AddEmitter(typeof(SoulTrailParticleSystem), 8, (int)size / 2, Vector3.Up * 5);

            //soulEmitters.AddEmitter(typeof(ShadowParticleSystem), 20, 0, new Vector3(0, -size + .1f, 0));

            lootSoul.AddComponent(typeof(PhysicsComponent), lootPhysics);
            genComponentManager.AddComponent(lootPhysics);

            lootSoul.AddComponent(typeof(UnanimatedModelComponent), lootGraphics);
            modelManager.AddComponent(lootGraphics);

            lootSoul.AddComponent(typeof(AIController), lootController);
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
        private List<Item> GetLootFor(string entityName)
        {
            List<Item> retItems = new List<Item>();
            int level = (int)levelManager.CurrentFloor;

            //get gold
            int goldQuantity = 5 * level * level;
            int extra = rand.Next(0, level * level + 1);
            if (rand.Next(10) < 5)
            {
                goldQuantity += extra;
            }
            else
            {
                goldQuantity -= extra;
            }

            retItems.Add(new Item(GetGoldIcon(goldQuantity), "gold", goldQuantity));

            //decide on gear
            if (rand.Next(30) < 5)
            {
                retItems.Add(GetGear(level));
            }

            //decide on consumable

            //decide on essence

            //decide on recipe


            return retItems;
        }

        
        private Item GetGear(int level)
        {
            return GetSword();
        }

        private List<Item> GearBossGear(int level)
        {
            return null;
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
                    for (int j = 0; j <= prevMaxIndex; ++j)
                    {
                        if (retItems[j].Name == second[i].Name)
                        {
                            retItems[j].AddQuantity(second[i].Quantity);
                            //if it's gold, make sure the icon is appropriate for the amount of gold
                            if (retItems[j].Name == "gold")
                            {
                                int quantity = retItems[j].Quantity;
                                retItems[j] = new Item(GetGoldIcon(quantity), "gold", quantity);
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
        private Texture2D GetIcon(string gearName)
        {
            string texName = "Textures\\";
            switch (gearName)
            {
                case "sword":
                    texName += "UI\\Abilities\\HS";
                    break;
                default:
                    texName += "UI\\Abilities\\HS";
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
