using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables;
using BEPUphysics.DataStructures;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public class EnemyManager : EntityManager
    {
        LootManager lm;
        IDictionary<Identification, GameEntity> enemies;

        public EnemyManager(KazgarsRevengeGame game)
            : base(game)
        {
            enemies = new Dictionary<Identification, GameEntity>();
            
        }
        public override void Initialize()
        {
            lm = Game.Services.GetService(typeof(LootManager)) as LootManager;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            List<Identification> toRemove = new List<Identification>();
            foreach (KeyValuePair<Identification, GameEntity> k in enemies)
            {
                if (k.Value.Dead)
                {
                    toRemove.Add(k.Key);
                }
            }
            for (int i = toRemove.Count - 1; i >= 0; --i)
            {
                enemies.Remove(toRemove[i]);
            }
        }





        public void CreateEnemy(EntityType type, Vector3 loc)
        {
            switch (type)
            {
                case EntityType.EliteEnemy:
                    CreateEliteEnemy(loc);
                    break;
                default:
                    CreateNormalEnemy(loc);
                    break;
            }
        }
        
        public void CreateEliteEnemy(Vector3 loc)
        {
            int r;
            int enemyLevel = Math.Max(1, players.GetHighestLevel() + (RandSingleton.U_Instance.Next(5) - 1));
            switch (levelManager.currentLevel.currentFloor)
            {
                case FloorName.Dungeon:
                    r = RandSingleton.U_Instance.Next(3);
                    if (r == 0)
                    {
                        ((EnemyManager)Game.Services.GetService(typeof(EnemyManager))).CreatePigman(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), loc, enemyLevel, true);
                    }
                    else if (r == 1)
                    {
                        ((EnemyManager)Game.Services.GetService(typeof(EnemyManager))).CreateCrossbowSkeleton(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), loc, enemyLevel, true);
                    }
                    else
                    {
                        ((EnemyManager)Game.Services.GetService(typeof(EnemyManager))).CreateMagicSkeleton(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), loc, enemyLevel, true);
                    }
                    break;
            }
        }

        public void CreateNormalEnemy(Vector3 loc)
        {
            int r;
            int enemyLevel = Math.Max(1, players.GetHighestLevel() + (RandSingleton.U_Instance.Next(5) - 2));
            switch (levelManager.currentLevel.currentFloor)
            {
                case FloorName.Dungeon:
                    r = RandSingleton.U_Instance.Next(3);
                    if (r == 0)
                    {
                        CreatePigman(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), loc, enemyLevel, false);
                    }
                    else if (r == 1)
                    {
                        CreateCrossbowSkeleton(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), loc, enemyLevel, false);
                    }
                    else
                    {
                        CreateMagicSkeleton(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), loc, enemyLevel, false);
                    }
                    break;
            }
        }

        const float meleeRange = 40;
        public void CreatePigman(Identification id, Vector3 position, int level, bool elite)
        {
            EntityType enemyType = EntityType.NormalEnemy;
            if (elite)
            {
                enemyType = EntityType.EliteEnemy;
            }

            GameEntity enemy = new GameEntity((elite? "Elite " : "") + "Pigman", FactionType.Enemies, enemyType);
            enemy.id = id;

            Dictionary<string, AttachableModel> attached = new Dictionary<string, AttachableModel>();
            attached.Add("sword", new AttachableModel(GetUnanimatedModel("Models\\Weapons\\axe"), "pig_hand_R"));
            enemy.AddSharedData(typeof(Dictionary<string, AttachableModel>), attached);

            Vector3 boxSize = new Vector3(20f, 37f, 20f);
            if(elite)
            {
                boxSize.X = 30;
                boxSize.Z = 30;
            }
            SetupEntityPhysicsAndShadow(enemy, position, boxSize, 100);
            float modelScale = 8;
            if (elite)
            {
                modelScale = 15;
            }
            SetupEntityGraphics(enemy, "Models\\Enemies\\Pigman\\pig_idle", modelScale);

            EnemyController enemyController = new EnemyController(mainGame, enemy, level);
            if (elite)
            {
                enemyController.MakeElite();
            }
            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            enemy.AddComponent(typeof(DropTable), lm.CreateNormalDropTableFor(enemy, AttackType.Melee, AttackType.None));

            enemies.Add(id, enemy);
        }
        public void CreateMagicSkeleton(Identification id, Vector3 position, int level, bool elite)
        {
            EntityType enemyType = EntityType.NormalEnemy;
            if (elite)
            {
                enemyType = EntityType.EliteEnemy;
            }
            GameEntity enemy = new GameEntity((elite ? "Elite " : "") + "Skeleton", FactionType.Enemies, enemyType);
            enemy.id = id;

            Vector3 boxSize = new Vector3(20f, 37f, 20f);
            if (elite)
            {
                boxSize.X = 30;
                boxSize.Z = 30;
            }
            SetupEntityPhysicsAndShadow(enemy, position, boxSize, 100);
            float modelScale = 10;
            if (elite)
            {
                modelScale = 15;
            }
            SetupEntityGraphics(enemy, "Models\\Enemies\\Skeleton\\s_idle", modelScale);

            MagicSkeletonController enemyController = new MagicSkeletonController(mainGame, enemy, level);
            if (elite)
            {
                enemyController.MakeElite();
            }
            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            enemy.AddComponent(typeof(DropTable), lm.CreateNormalDropTableFor(enemy, AttackType.Magic, AttackType.None));

            enemies.Add(id, enemy);
        }
        public void CreateCrossbowSkeleton(Identification id, Vector3 position, int level, bool elite)
        {
            EntityType enemyType = EntityType.NormalEnemy;
            if (elite)
            {
                enemyType = EntityType.EliteEnemy;
            }
            GameEntity enemy = new GameEntity((elite ? "Elite " : "") + "Skeleton", FactionType.Enemies, enemyType);
            enemy.id = id;

            Dictionary<string, AttachableModel> attached = new Dictionary<string, AttachableModel>();
            attached.Add("hand", new AttachableModel(GetUnanimatedModel("Models\\Weapons\\crossbow"), "s_hand_R"));
            enemy.AddSharedData(typeof(Dictionary<string, AttachableModel>), attached);

            Vector3 boxSize = new Vector3(20f, 37f, 20f);
            if (elite)
            {
                boxSize.X = 30;
                boxSize.Z = 30;
            }
            SetupEntityPhysicsAndShadow(enemy, position, boxSize, 100);
            float modelScale = 10;
            if (elite)
            {
                modelScale = 15;
            }
            SetupEntityGraphics(enemy, "Models\\Enemies\\Skeleton\\s_idle", modelScale);

            CrossbowSkeletonController enemyController = new CrossbowSkeletonController(mainGame, enemy, level);
            if (elite)
            {
                enemyController.MakeElite();
            }
            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            enemy.AddComponent(typeof(DropTable), lm.CreateNormalDropTableFor(enemy, AttackType.Ranged, AttackType.None));

            enemies.Add(id, enemy);
        }
        public void CreateArmorEnemy(Identification id, Vector3 position, int level, bool elite)
        {
            EntityType enemyType = EntityType.NormalEnemy;
            if (elite)
            {
                enemyType = EntityType.EliteEnemy;
            }
            GameEntity enemy = new GameEntity((elite ? "Elite " : "") + "Animated Armor", FactionType.Enemies, enemyType);
            enemy.id = id;

            Dictionary<string, Model> syncedModels = new Dictionary<string, Model>();
            syncedModels.Add("feet", GetAnimatedModel("Models\\Armor\\armor_boots_rino"));
            syncedModels.Add("wrists", GetAnimatedModel("Models\\Armor\\armor_wrist_rino"));
            syncedModels.Add("head", GetAnimatedModel("Models\\Armor\\armor_head_rino"));
            enemy.AddSharedData(typeof(Dictionary<string, Model>), syncedModels);

            Dictionary<string, AttachableModel> attached = new Dictionary<string, AttachableModel>();
            attached.Add("sword", new AttachableModel(GetUnanimatedModel("Models\\Weapons\\sword01"), "Hand_R", MathHelper.Pi, 0));
            enemy.AddSharedData(typeof(Dictionary<string, AttachableModel>), attached);

            Vector3 boxSize = new Vector3(20f, 37f, 20f);
            if (elite)
            {
                boxSize.X = 30;
                boxSize.Z = 30;
            }
            SetupEntityPhysicsAndShadow(enemy, position, boxSize, 100);
            float modelScale = 10;
            if (elite)
            {
                modelScale = 15;
            }

            //get kazgar's animations, but don't add his model
            Model enemyModel = GetAnimatedModel("Models\\Player\\k_idle1");
            AnimationPlayer enemyAnimations = new AnimationPlayer(enemyModel.Tag as SkinningData);
            enemy.AddSharedData(typeof(AnimationPlayer), enemyAnimations);
            AnimatedModelComponent enemyGraphics = new AnimatedModelComponent(mainGame, enemy, enemyModel, modelScale, Vector3.Down * 18);

            if ((mainGame as MainGame).ParticlesSetting == MainGame.SettingAmount.High)
            {
                enemyGraphics.AddEmitter(typeof(ArmorSoulSystem), "soul", 50, 4, Vector3.Up * 8);
            }

            enemyGraphics.TurnOffMainModel();
            enemy.AddComponent(typeof(AnimatedModelComponent), enemyGraphics);
            modelManager.AddComponent(enemyGraphics);

            ArmorEnemyController enemyController = new ArmorEnemyController(mainGame, enemy, level);
            if (elite)
            {
                enemyController.MakeElite();
            }
            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            enemy.AddComponent(typeof(DropTable), lm.CreateNormalDropTableFor(enemy, AttackType.Melee, AttackType.Magic));

            enemies.Add(id, enemy);
        }
        public void CreateSuccubus(Identification id, Vector3 position, int level, bool elite)
        {
            EntityType enemyType = EntityType.NormalEnemy;
            if (elite)
            {
                enemyType = EntityType.EliteEnemy;
            }
            GameEntity enemy = new GameEntity((elite ? "Elite " : "") + "Succubus", FactionType.Enemies, enemyType);
            enemy.id = id;

            Vector3 boxSize = new Vector3(20f, 37f, 20f);
            if (elite)
            {
                boxSize.X = 30;
                boxSize.Z = 30;
            }
            SetupEntityPhysicsAndShadow(enemy, position, boxSize, 100);
            float modelScale = 10;
            if (elite)
            {
                modelScale = 15;
            }
            SetupEntityGraphics(enemy, "Models\\Enemies\\Succubus\\su_fly", modelScale);

            SuccubusController enemyController = new SuccubusController(mainGame, enemy, level);
            if (elite)
            {
                enemyController.MakeElite();
            }
            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            enemy.AddComponent(typeof(DropTable), lm.CreateNormalDropTableFor(enemy, AttackType.Magic, AttackType.None));

            enemies.Add(id, enemy);
        }

        List<GameEntity> friendlyNPCS = new List<GameEntity>();
        public void CreateDummy(Vector3 position)
        {
            GameEntity entity = new GameEntity("Training Dummy", FactionType.Enemies, EntityType.NormalEnemy);

            SetupEntityPhysicsAndShadow(entity, position, new Vector3(20, 40, 20), -1);

            SetupEntityGraphics(entity, "Models\\Enemies\\Dummy\\dummy_idle", 10);

            DummyController controller = new DummyController(mainGame, entity, players.GetHighestLevel());
            entity.AddComponent(typeof(AliveComponent), controller);
            genComponentManager.AddComponent(controller);

            friendlyNPCS.Add(entity);
        }
        public void CreateVendorGuy(Vector3 position)
        {
            GameEntity entity = new GameEntity("Shopkeeper", FactionType.Players, EntityType.Shopkeeper);

            SetupEntityPhysicsAndShadow(entity, position, new Vector3(20, 35, 20), -1);

            SetupEntityGraphics(entity, "Models\\Enemies\\Dummy\\dummy_idle", 10);

            ShopkeeperController controller = new ShopkeeperController(mainGame, entity);
            entity.AddComponent(typeof(PlayerInteractiveController), controller);
            genComponentManager.AddComponent(controller);

            friendlyNPCS.Add(entity);
        }
        public void CreateEssenceGuy(Vector3 position)
        {
            GameEntity entity = new GameEntity("Essence Guy", FactionType.Players, EntityType.Shopkeeper);

            SetupEntityPhysicsAndShadow(entity, position, new Vector3(20, 35, 20), -1);

            SetupEntityGraphics(entity, "Models\\Enemies\\Dummy\\dummy_idle", 10);

            EssenceGuyController controller = new EssenceGuyController(mainGame, entity);
            entity.AddComponent(typeof(PlayerInteractiveController), controller);
            genComponentManager.AddComponent(controller);

            friendlyNPCS.Add(entity);
        }


        #region Bosses
        public void CreateBoss(Identification id, Vector3 position)
        {
            //TODO: switch based on level
            CreateDragon(id, position);
        }

        public void CreateDragon(Identification id, Vector3 position)
        {
            position.Y = 42;
            GameEntity dragon = new GameEntity("Chillinator", FactionType.Enemies, EntityType.NormalEnemy);
            dragon.id = id;

            SetupEntityPhysicsAndShadow(dragon, position, new Vector3(100, 40, 100), 250);

            Model enemyModel = GetAnimatedModel("Models\\Enemies\\Dragon\\d_idle");
            AnimationPlayer enemyAnimations = new AnimationPlayer(enemyModel.Tag as SkinningData);
            dragon.AddSharedData(typeof(AnimationPlayer), enemyAnimations);
            AnimatedModelComponent enemyGraphics = new AnimatedModelComponent(mainGame, dragon, enemyModel, 10, Vector3.Down * 9);
            //enemyGraphics.AddEmitter(typeof(FireDragonHeadSystem), "firehead", 150, 0, Vector3.Zero, "d_head_emittor_L");
            //enemyGraphics.AddEmitter(typeof(FrostDragonHeadSystem), "frosthead", 150, 0, Vector3.Zero, "d_head_emittor_R");
            dragon.AddComponent(typeof(AnimatedModelComponent), enemyGraphics);
            modelManager.AddComponent(enemyGraphics);

            DragonController dragonController = new DragonController(mainGame, dragon);

            dragon.AddComponent(typeof(AliveComponent), dragonController);
            genComponentManager.AddComponent(dragonController);

            dragon.AddComponent(typeof(DropTable), lm.CreateBossDropTable(dragon, FloorName.Dungeon));

            enemies.Add(id, dragon);
        }
        #endregion






        /// <summary>
        /// Creates an enemy with typical physics and animated graphics around the given entity.
        /// You still need to give it an AIController and add the entity to the entity list after calling this.
        /// </summary>
        private void SetupEntityGraphics(GameEntity entity, string model, float scale)
        {
            Model enemyModel = GetAnimatedModel(model);
            AnimationPlayer enemyAnimations = new AnimationPlayer(enemyModel.Tag as SkinningData);
            entity.AddSharedData(typeof(AnimationPlayer), enemyAnimations);
            AnimatedModelComponent enemyGraphics = new AnimatedModelComponent(mainGame, entity, enemyModel, scale, Vector3.Down * 18);

            entity.AddComponent(typeof(AnimatedModelComponent), enemyGraphics);
            modelManager.AddComponent(enemyGraphics);
        }

        private void SetupEntityPhysicsAndShadow(GameEntity entity, Vector3 position, Vector3 dimensions, float mass)
        {
            position.Y = LevelManager.MOB_SPAWN_Y;
            Entity enemyPhysicalData;
            if (mass == -1)
            {
                enemyPhysicalData = new Box(position, dimensions.X, dimensions.Y, dimensions.Z);
            }
            else
            {
                enemyPhysicalData = new Box(position, dimensions.X, dimensions.Y, dimensions.Z, mass);
            }
            enemyPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.EnemyCollisionGroup;
            enemyPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            enemyPhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver4 * 3, 0, 0));
            entity.AddSharedData(typeof(Entity), enemyPhysicalData);

            PhysicsComponent enemyPhysics = new PhysicsComponent(mainGame, entity);
            BlobShadowDecal enemyShadow = new BlobShadowDecal(mainGame, entity, (dimensions.X + dimensions.Z) / 3);

            entity.AddComponent(typeof(PhysicsComponent), enemyPhysics);
            genComponentManager.AddComponent(enemyPhysics);

            entity.AddComponent(typeof(BlobShadowDecal), enemyShadow);
            billboardManager.AddComponent(enemyShadow);
        }

        public void ClearEnemies()
        {
            foreach (KeyValuePair<Identification, GameEntity> k in enemies)
            {
                k.Value.KillEntity();
            }
            enemies.Clear();

            foreach (GameEntity e in friendlyNPCS)
            {
                e.KillEntity();
            }
            friendlyNPCS.Clear();
        }
        
        /// <summary>
        /// Gets an enemy from this manager with the associated id, or null if none exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameEntity getEntity(Identification id)
        {
            if (!enemies.ContainsKey(id))
            {
                return null;
            }
            return enemies[id];
        }
    }
}
