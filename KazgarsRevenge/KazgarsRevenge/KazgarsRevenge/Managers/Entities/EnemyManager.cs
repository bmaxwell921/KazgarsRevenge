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
    class EnemyManager : EntityManager
    {
        List<GameEntity> enemies = new List<GameEntity>();

        public EnemyManager(MainGame game)
            : base(game)
        {

        }

        public void CreateBrute(Vector3 position)
        {
            GameEntity brute = new GameEntity("Brute", "bad");

            Entity brutePhysicalData = new Box(position, 20f, 37f, 20f, 100);
            brutePhysicalData.CollisionInformation.CollisionRules.Group = MainGame.EnemyCollisionGroup;
            brutePhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            brutePhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            brutePhysicalData.CollisionInformation.Tag = brute;
            brutePhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));

            Model bruteModel = GetAnimatedModel("Models\\Enemies\\Pigman\\pig_idle");
            AnimationPlayer bruteAnimations = new AnimationPlayer(bruteModel.Tag as SkinningData);
            HealthData bruteHealth = new HealthData(100);

            PhysicsComponent brutePhysics = new PhysicsComponent(mainGame, brutePhysicalData);
            AnimatedModelComponent bruteGraphics = new AnimatedModelComponent(mainGame, brutePhysicalData, bruteModel, bruteAnimations, new Vector3(10f), Vector3.Down * 18, new Dictionary<string, AttachableModel>());
            HealthHandlerComponent bruteHealthHandler = new HealthHandlerComponent(mainGame, bruteHealth, brute);

            BruteController bruteController = new BruteController(mainGame, brute, bruteHealth, brutePhysicalData, bruteAnimations);

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
    }
}
