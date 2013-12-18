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
        List<GameEntity> enemies = new List<GameEntity>();

        public EnemyManager(KazgarsRevengeGame game)
            : base(game)
        {

        }

        public override void Update(GameTime gameTime)
        {
            
        }

        #region entities
        public void CreateBrute(Vector3 position)
        {
            GameEntity brute = new GameEntity("Brute", FactionType.Enemies);

            Entity brutePhysicalData = new Box(position, 20f, 37f, 20f, 100);
            brutePhysicalData.CollisionInformation.CollisionRules.Group = mainGame.EnemyCollisionGroup;
            brutePhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            brutePhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            brute.AddSharedData(typeof(Entity), brutePhysicalData);

            Model bruteModel = GetAnimatedModel("Models\\Enemies\\Pigman\\pig_idle");
            AnimationPlayer bruteAnimations = new AnimationPlayer(bruteModel.Tag as SkinningData);
            brute.AddSharedData(typeof(AnimationPlayer), bruteAnimations);

            HealthData bruteHealth = new HealthData(100);
            brute.AddSharedData(typeof(HealthData), bruteHealth);

            brute.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            PhysicsComponent brutePhysics = new PhysicsComponent(mainGame, brute);
            AnimatedModelComponent bruteGraphics = new AnimatedModelComponent(mainGame, brute, bruteModel, new Vector3(10f), Vector3.Down * 18);
            HealthHandlerComponent bruteHealthHandler = new HealthHandlerComponent(mainGame, brute);


            EnemyControllerSettings bruteSettings = new EnemyControllerSettings();
            #region settings init
            bruteSettings.attackAniName = "pig_attack";
            bruteSettings.idleAniName = "pig_idle";
            bruteSettings.runAniName = "pig_walk";
            bruteSettings.walkAniName = "pig_walk";
            bruteSettings.hitAniName = "pig_hit";
            bruteSettings.deathAniName = "pig_death";
            bruteSettings.attackDamage = 5;
            bruteSettings.attackLength = bruteAnimations.skinningDataValue.AnimationClips["pig_attack"].Duration.TotalMilliseconds;
            bruteSettings.attackRange = 25;
            bruteSettings.noticePlayerRange = 200;
            bruteSettings.stopChasingRange = 600;
            bruteSettings.runSpeed = 80;
            bruteSettings.walkSpeed = 40;
            #endregion

            EnemyController bruteController = new EnemyController(mainGame, brute, bruteSettings);

            brute.AddComponent(typeof(PhysicsComponent), brutePhysics);
            genComponentManager.AddComponent(brutePhysics);

            brute.AddComponent(typeof(AnimatedModelComponent), bruteGraphics);
            modelManager.AddComponent(bruteGraphics);

            brute.AddComponent(typeof(HealthHandlerComponent), bruteHealthHandler);
            genComponentManager.AddComponent(bruteHealthHandler);

            brute.AddComponent(typeof(AIController), bruteController);
            genComponentManager.AddComponent(bruteController);

            enemies.Add(brute);
        }
        #endregion
    }
}
