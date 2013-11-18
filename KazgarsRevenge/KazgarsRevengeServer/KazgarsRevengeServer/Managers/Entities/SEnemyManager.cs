using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KazgarsRevenge;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;

namespace KazgarsRevengeServer
{
    public class SEnemyManager : SEntityManager
    {
        IList<GameEntity> enemies;

        SPlayerManager pm;

        public SEnemyManager(KazgarsRevengeGame game)
            : base(game)
        {
            enemies = new List<GameEntity>();
        }

        public override void Initialize()
        {
            base.Initialize();
            pm = Game.Services.GetService(typeof(SPlayerManager)) as SPlayerManager;
        }

        #region Entities
        public void CreateBrute(Vector3 position)
        {
            GameEntity brute = new GameEntity("Brute", FactionType.Enemies);

            Entity brutePhysicalData = new Box(position, 20f, 37f, 20f, 100);
            brutePhysicalData.CollisionInformation.CollisionRules.Group = game.EnemyCollisionGroup;
            brutePhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            brutePhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            brutePhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            brute.AddSharedData(typeof(Entity), brutePhysicalData);

            HealthData bruteHealth = new HealthData(100);
            brute.AddSharedData(typeof(HealthData), bruteHealth);

            brute.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            PhysicsComponent brutePhysics = new PhysicsComponent(game, brute);

            HealthHandlerComponent bruteHealthHandler = new HealthHandlerComponent(game, brute);

            BruteController bruteController = new BruteController(game, brute);

            brute.AddComponent(typeof(PhysicsComponent), brutePhysics);
            gcm.AddComponent(brutePhysics);

            brute.AddComponent(typeof(HealthHandlerComponent), bruteHealthHandler);
            gcm.AddComponent(bruteHealthHandler);

            brute.AddComponent(typeof(AIController), bruteController);
            gcm.AddComponent(bruteController);

            enemies.Add(brute);
        }
        #endregion
    }
}
