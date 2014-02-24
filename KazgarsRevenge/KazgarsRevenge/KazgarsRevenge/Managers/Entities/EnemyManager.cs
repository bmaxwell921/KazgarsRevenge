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

            Entity brutePhysicalData = new Box(position, 20f, 37f, 20f, 100);
            brutePhysicalData.CollisionInformation.CollisionRules.Group = mainGame.EnemyCollisionGroup;
            brutePhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            brutePhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            brute.AddSharedData(typeof(Entity), brutePhysicalData);

            Model bruteModel = GetAnimatedModel("Models\\Enemies\\Pigman\\pig_idle");
            AnimationPlayer bruteAnimations = new AnimationPlayer(bruteModel.Tag as SkinningData);
            brute.AddSharedData(typeof(AnimationPlayer), bruteAnimations);

            Dictionary<string, AttachableModel> attached = new Dictionary<string, AttachableModel>();
            attached.Add("sword", new AttachableModel(GetUnanimatedModel("Models\\Attachables\\axe"), "pig_hand_R"));
            brute.AddSharedData(typeof(Dictionary<string, AttachableModel>), attached);

            PhysicsComponent brutePhysics = new PhysicsComponent(mainGame, brute);
            AnimatedModelComponent bruteGraphics = new AnimatedModelComponent(mainGame, brute, bruteModel, 10, Vector3.Down * 18);
            BlobShadowDecal bruteShadow = new BlobShadowDecal(mainGame, brute, 15);

            EnemyControllerSettings bruteSettings = new EnemyControllerSettings();
            bruteSettings.aniPrefix = "pig_";
            bruteSettings.attackAniName = "attack";
            bruteSettings.attackDamage = 5;
            bruteSettings.level = level;
            bruteSettings.attackRange = 30;
            bruteSettings.noticePlayerRange = 150;
            bruteSettings.stopChasingRange = 400;

            EnemyController bruteController = new EnemyController(mainGame, brute, bruteSettings);
            HealthBarBillboard hp = new HealthBarBillboard(mainGame, brute, 5, 40, bruteController as AliveComponent);

            brute.AddComponent(typeof(PhysicsComponent), brutePhysics);
            genComponentManager.AddComponent(brutePhysics);

            brute.AddComponent(typeof(AnimatedModelComponent), bruteGraphics);
            modelManager.AddComponent(bruteGraphics);

            brute.AddComponent(typeof(BlobShadowDecal), bruteShadow);
            billboardManager.AddComponent(bruteShadow);

            brute.AddComponent(typeof(AliveComponent), bruteController);
            genComponentManager.AddComponent(bruteController);
            
            brute.AddComponent(typeof(HealthBarBillboard), hp);
            billboardManager.AddComponent(hp);

            enemies.Add(id, brute);
        }

        public void CreateMagicSkeleton(Identification id, Vector3 position, int level)
        {
            GameEntity enemy = new GameEntity("Brute", FactionType.Enemies, EntityType.NormalEnemy);
            enemy.id = id;

            Entity enemyPhysicalData = new Box(position, 20f, 37f, 20f, 100);
            enemyPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.EnemyCollisionGroup;
            enemyPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            enemyPhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            enemy.AddSharedData(typeof(Entity), enemyPhysicalData);

            Model enemyModel = GetAnimatedModel("Models\\Enemies\\Skeleton\\s_idle");
            AnimationPlayer enemyAnimations = new AnimationPlayer(enemyModel.Tag as SkinningData);
            enemy.AddSharedData(typeof(AnimationPlayer), enemyAnimations);

            PhysicsComponent enemyPhysics = new PhysicsComponent(mainGame, enemy);
            AnimatedModelComponent enemyGraphics = new AnimatedModelComponent(mainGame, enemy, enemyModel, 10, Vector3.Down * 18);
            BlobShadowDecal enemyShadow = new BlobShadowDecal(mainGame, enemy, 15);

            EnemyControllerSettings enemySettings = new EnemyControllerSettings();
            enemySettings.aniPrefix = "s_";
            enemySettings.attackAniName = "magic";
            enemySettings.attackDamage = 5;
            enemySettings.level = level;
            enemySettings.attackRange = 1000;
            enemySettings.noticePlayerRange = 300;
            enemySettings.stopChasingRange = 450;

            SkeletonController enemyController = new SkeletonController(mainGame, enemy, enemySettings);
            HealthBarBillboard hp = new HealthBarBillboard(mainGame, enemy, 5, 40, enemyController as AliveComponent);

            enemy.AddComponent(typeof(PhysicsComponent), enemyPhysics);
            genComponentManager.AddComponent(enemyPhysics);

            enemy.AddComponent(typeof(AnimatedModelComponent), enemyGraphics);
            modelManager.AddComponent(enemyGraphics);

            enemy.AddComponent(typeof(BlobShadowDecal), enemyShadow);
            billboardManager.AddComponent(enemyShadow);

            enemy.AddComponent(typeof(AliveComponent), enemyController);
            genComponentManager.AddComponent(enemyController);

            enemy.AddComponent(typeof(HealthBarBillboard), hp);
            billboardManager.AddComponent(hp);

            enemies.Add(id, enemy);
        }

        public void CreateDragon(Identification id, Vector3 position)
        {
            GameEntity dragon = new GameEntity("Brute", FactionType.Enemies, EntityType.NormalEnemy);
            dragon.id = id;

            Entity dragonPhysicalData = new Box(position, 100f, 37f, 100f, 100);
            dragonPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.EnemyCollisionGroup;
            dragonPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            dragonPhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            dragon.AddSharedData(typeof(Entity), dragonPhysicalData);

            Model dragonModel = GetAnimatedModel("Models\\Enemies\\Pigman\\pig_idle");
            AnimationPlayer dragonAnimations = new AnimationPlayer(dragonModel.Tag as SkinningData);
            dragon.AddSharedData(typeof(AnimationPlayer), dragonAnimations);

            PhysicsComponent dragonPhysics = new PhysicsComponent(mainGame, dragon);
            AnimatedModelComponent dragonGraphics = new AnimatedModelComponent(mainGame, dragon, dragonModel, 50, Vector3.Down * 18);
            BlobShadowDecal dragonShadow = new BlobShadowDecal(mainGame, dragon, 15);

            EnemyControllerSettings dragonSettings = new EnemyControllerSettings();
            dragonSettings.aniPrefix = "pig_";
            dragonSettings.attackAniName = "attack";
            dragonSettings.attackDamage = 5;
            dragonSettings.level = 15;
            dragonSettings.attackRange = 100;
            dragonSettings.noticePlayerRange = 200;
            dragonSettings.stopChasingRange = 650;

            DragonController dragonController = new DragonController(mainGame, dragon, dragonSettings);
            HealthBarBillboard hp = new HealthBarBillboard(mainGame, dragon, 5, 40, dragonController as AliveComponent);

            dragon.AddComponent(typeof(PhysicsComponent), dragonPhysics);
            genComponentManager.AddComponent(dragonPhysics);

            dragon.AddComponent(typeof(AnimatedModelComponent), dragonGraphics);
            modelManager.AddComponent(dragonGraphics);

            dragon.AddComponent(typeof(BlobShadowDecal), dragonShadow);
            billboardManager.AddComponent(dragonShadow);

            dragon.AddComponent(typeof(AliveComponent), dragonController);
            genComponentManager.AddComponent(dragonController);

            dragon.AddComponent(typeof(HealthBarBillboard), hp);
            billboardManager.AddComponent(hp);

            enemies.Add(id, dragon);
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
