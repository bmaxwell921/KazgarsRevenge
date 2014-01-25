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
        public void CreateBrute(Vector3 position, int level)
        {
            GameEntity brute = new GameEntity("Brute", FactionType.Enemies, EntityType.NormalEnemy);

            Entity brutePhysicalData = new Box(position, 20f, 37f, 20f, 100);
            brutePhysicalData.CollisionInformation.CollisionRules.Group = game.EnemyCollisionGroup;
            brutePhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            brutePhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            brutePhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));
            brute.AddSharedData(typeof(Entity), brutePhysicalData);

            brute.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            PhysicsComponent brutePhysics = new PhysicsComponent(game, brute);

            EnemyControllerSettings bruteSettings = new EnemyControllerSettings();
            #region settings init
            bruteSettings.attackDamage = 5;
            bruteSettings.level = level;
            bruteSettings.attackRange = 25;
            bruteSettings.attackLength = 100;
            bruteSettings.noticePlayerRange = 200;
            bruteSettings.stopChasingRange = 600;
            bruteSettings.runSpeed = 80;
            bruteSettings.walkSpeed = 40;
            #endregion

            EnemyController bruteController = new EnemyController(game, brute, bruteSettings);

            brute.AddComponent(typeof(PhysicsComponent), brutePhysics);
            gcm.AddComponent(brutePhysics);

            brute.AddComponent(typeof(AliveComponent), bruteController);
            gcm.AddComponent(bruteController);

            enemies.Add(brute);
        }
        #endregion

        public void Reset()
        {
            enemies.Clear();
        }
    }
}
