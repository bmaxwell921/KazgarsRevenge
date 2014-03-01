﻿using System;
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
        //List<GameEntity> enemies = new List<GameEntity>();
        IDictionary<Identification, GameEntity> enemies;

        public EnemyManager(KazgarsRevengeGame game)
            : base(game)
        {
            enemies = new Dictionary<Identification, GameEntity>();
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        #region entities
        const float melleRange = 40;
        public void CreateBrute(Identification id, Vector3 position, int level)
        {
            GameEntity brute = new GameEntity("Brute", FactionType.Enemies, EntityType.NormalEnemy);
            brute.id = id;

            Dictionary<string, AttachableModel> attached = new Dictionary<string, AttachableModel>();
            attached.Add("sword", new AttachableModel(GetUnanimatedModel("Models\\Attachables\\axe"), "pig_hand_R"));
            brute.AddSharedData(typeof(Dictionary<string, AttachableModel>), attached);

            SetupEnemyEntity(brute, position, new Vector3(20f, 37f, 20f), 100, "Models\\Enemies\\Pigman\\pig_idle");


            EnemyController bruteController = new EnemyController(mainGame, brute, level);
            brute.AddComponent(typeof(AliveComponent), bruteController);
            genComponentManager.AddComponent(bruteController);

            AddHealthBarComponent(brute, 40);

            enemies.Add(id, brute);
        }

        public void CreateMagicSkeleton(Identification id, Vector3 position, int level)
        {
            GameEntity enemy = new GameEntity("Skeleton", FactionType.Enemies, EntityType.NormalEnemy);
            enemy.id = id;

            SetupEnemyEntity(enemy, position, new Vector3(20f, 37f, 20f), 100, "Models\\Enemies\\Skeleton\\s_idle");

            SkeletonController enemyController = new SkeletonController(mainGame, enemy, level);
            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            AddHealthBarComponent(enemy, 40);

            enemies.Add(id, enemy);
        }

        public void CreateArmorEnemy(Identification id, Vector3 position, int level)
        {
            GameEntity enemy = new GameEntity("Animated Armor", FactionType.Enemies, EntityType.NormalEnemy);
            enemy.id = id;

            Dictionary<string, Model> syncedModels = new Dictionary<string, Model>();
            syncedModels.Add("feet", GetAnimatedModel("Models\\Armor\\armor_boots_rino"));
            syncedModels.Add("wrists", GetAnimatedModel("Models\\Armor\\armor_wrist_rino"));
            syncedModels.Add("head", GetAnimatedModel("Models\\Armor\\armor_head_rino"));
            enemy.AddSharedData(typeof(Dictionary<string, Model>), syncedModels);

            Dictionary<string, AttachableModel> attached = new Dictionary<string, AttachableModel>();
            attached.Add("sword", new AttachableModel(GetUnanimatedModel("Models\\Attachables\\sword01"), "Hand_R", MathHelper.Pi, 0));
            enemy.AddSharedData(typeof(Dictionary<string, AttachableModel>), attached);

            SetupEntityNoGraphics(enemy, position, new Vector3(20, 37, 20), 100);

            //get kazgar's animations, but don't add his model
            Model enemyModel = GetAnimatedModel("Models\\Player\\k_idle1");
            AnimationPlayer enemyAnimations = new AnimationPlayer(enemyModel.Tag as SkinningData);
            enemy.AddSharedData(typeof(AnimationPlayer), enemyAnimations);
            AnimatedModelComponent enemyGraphics = new AnimatedModelComponent(mainGame, enemy, enemyModel, 10, Vector3.Down * 18);

            if ((mainGame as MainGame).ParticlesSetting == MainGame.SettingAmount.High)
            {
                enemyGraphics.AddEmitter(typeof(ArmorSoulSystem), "soul", 50, 4, Vector3.Up * 8);
            }

            enemyGraphics.TurnOffMainModel();
            enemy.AddComponent(typeof(AnimatedModelComponent), enemyGraphics);
            modelManager.AddComponent(enemyGraphics);


            ArmorEnemyController controller = new ArmorEnemyController(mainGame, enemy, level);
            enemy.AddComponent(typeof(AliveComponent), controller);
            genComponentManager.AddComponent(controller);

            AddHealthBarComponent(enemy, 40);

            enemies.Add(id, enemy);
        }

        public void CreateDragon(Identification id, Vector3 position)
        {
            GameEntity dragon = new GameEntity("Brute", FactionType.Enemies, EntityType.NormalEnemy);
            dragon.id = id;

            SetupEnemyEntity(dragon, position, new Vector3(100, 37, 100), 250, "Models\\Enemies\\Pigman\\pig_idle");

            DragonController dragonController = new DragonController(mainGame, dragon);

            dragon.AddComponent(typeof(AliveComponent), dragonController);
            genComponentManager.AddComponent(dragonController);

            enemies.Add(id, dragon);
        }

        /// <summary>
        /// Creates an enemy with typical physics and animated graphics around the given entity.
        /// You still need to give it an AIController and add the entity to the entity list after calling this.
        /// </summary>
        private void SetupEnemyEntity(GameEntity entity, Vector3 position, Vector3 dimensions, float mass, string model)
        {
            SetupEntityNoGraphics(entity, position, dimensions, mass);

            Model enemyModel = GetAnimatedModel(model);
            AnimationPlayer enemyAnimations = new AnimationPlayer(enemyModel.Tag as SkinningData);
            entity.AddSharedData(typeof(AnimationPlayer), enemyAnimations);
            AnimatedModelComponent enemyGraphics = new AnimatedModelComponent(mainGame, entity, enemyModel, 10, Vector3.Down * 18);

            entity.AddComponent(typeof(AnimatedModelComponent), enemyGraphics);
            modelManager.AddComponent(enemyGraphics);
        }

        private void SetupEntityNoGraphics(GameEntity entity, Vector3 position, Vector3 dimensions, float mass)
        {
            Entity enemyPhysicalData = new Box(position, dimensions.X, dimensions.Y, dimensions.Z, mass);
            enemyPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.EnemyCollisionGroup;
            enemyPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            enemyPhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            entity.AddSharedData(typeof(Entity), enemyPhysicalData);

            PhysicsComponent enemyPhysics = new PhysicsComponent(mainGame, entity);
            BlobShadowDecal enemyShadow = new BlobShadowDecal(mainGame, entity, dimensions.X);

            entity.AddComponent(typeof(PhysicsComponent), enemyPhysics);
            genComponentManager.AddComponent(enemyPhysics);

            entity.AddComponent(typeof(BlobShadowDecal), enemyShadow);
            billboardManager.AddComponent(enemyShadow);
        }

        private void AddHealthBarComponent(GameEntity entity, float barHeight)
        {
            HealthBarBillboard hp = new HealthBarBillboard(mainGame, entity, 5, barHeight);
            entity.AddComponent(typeof(HealthBarBillboard), hp);
            billboardManager.AddComponent(hp);
        }
        #endregion
        
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
