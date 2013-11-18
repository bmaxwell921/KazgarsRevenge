using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class LootManager : EntityManager
    {
        List<GameEntity> lootSouls = new List<GameEntity>();

        public LootManager(KazgarsRevengeGame game)
            : base(game)
        {

        }

        public void CreateLootSoul(Vector3 position, List<Item> containedLoot)
        {
            GameEntity lootSoul = new GameEntity("loot", FactionType.Neutral);

            Entity lootPhysicalData = new Box(position, 1, 1, 1);
            lootPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.LootCollisionGroup;

            //locking rotation on axis (so that bumping into something won't make the player tip over)
            lootPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            //more accurate collision detection for the player
            lootPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            lootSoul.AddSharedData(typeof(Entity), lootPhysicalData);


            Model lootModel = GetAnimatedModel("Models\\Enemies\\Pigman\\pig_idle");
            AnimationPlayer lootAnimations = new AnimationPlayer(lootModel.Tag as SkinningData);
            lootSoul.AddSharedData(typeof(AnimationPlayer), lootAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            lootSoul.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            PhysicsComponent lootPhysics = new PhysicsComponent(mainGame, lootSoul);
            AnimatedModelComponent lootGraphics = new AnimatedModelComponent(mainGame, lootSoul, lootModel, new Vector3(10f), Vector3.Down * 18);
            LootSoulController lootController = new LootSoulController(mainGame, lootSoul, 10, containedLoot);

            lootSoul.AddComponent(typeof(PhysicsComponent), lootPhysics);
            genComponentManager.AddComponent(lootPhysics);

            lootSoul.AddComponent(typeof(AnimatedModelComponent), lootGraphics);
            modelManager.AddComponent(lootGraphics);

            lootSoul.AddComponent(typeof(LootSoulController), lootController);
            genComponentManager.AddComponent(lootController);
        }

        #region Loot Creation
        Dictionary<string, Texture2D> equippableIcons = new Dictionary<string, Texture2D>();
        string attachDir = "Models\\Attachables\\";
        private Texture2D GetIcon(string gearName)
        {
            string texName = "Textures\\";
            switch (gearName)
            {
                case "sword":
                    texName += "whitePixel";
                    break;
                default:
                    texName += "whitePixel";
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
            return GetSword(GetStats("sword"));
        }

        /// <summary>
        /// loads a saved sword with predefined stats
        /// </summary>
        public Equippable GetSword(Dictionary<StatType, float> swordStats)
        {
            return new Weapon(GetIcon("sword"), "sword01", swordStats, GetUnanimatedModel(attachDir + "sword01"), AttackType.Melle, false);
        }

        public Equippable GenerateBow()
        {
            return GetBow(GetStats("bow"));
        }

        public Equippable GetBow(Dictionary<StatType, float> bowStats)
        {
            return new Weapon(GetIcon("bow"), "bow", bowStats, GetUnanimatedModel(attachDir + "bow01"), AttackType.Ranged, false);
        }
        #endregion
    }
}
