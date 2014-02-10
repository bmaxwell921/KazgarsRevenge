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
            #region settings init
            bruteSettings.attackAniName = "pig_attack";
            bruteSettings.idleAniName = "pig_idle";
            bruteSettings.runAniName = "pig_walk";
            bruteSettings.walkAniName = "pig_walk";
            bruteSettings.hitAniName = "pig_hit";
            bruteSettings.deathAniName = "pig_death";
            bruteSettings.attackDamage = 5;
            bruteSettings.attackLength = bruteAnimations.GetAniMillis("pig_attack");
            bruteSettings.level = level;
            bruteSettings.attackRange = 30;
            bruteSettings.noticePlayerRange = 200;
            bruteSettings.stopChasingRange = 650;
            #endregion

            EnemyController bruteController = new EnemyController(mainGame, brute, bruteSettings);

            brute.AddComponent(typeof(PhysicsComponent), brutePhysics);
            genComponentManager.AddComponent(brutePhysics);

            brute.AddComponent(typeof(AnimatedModelComponent), bruteGraphics);
            modelManager.AddComponent(bruteGraphics);

            brute.AddComponent(typeof(AliveComponent), bruteController);
            genComponentManager.AddComponent(bruteController);

            brute.AddComponent(typeof(BlobShadowDecal), bruteShadow);
            billboardManager.AddComponent(bruteShadow);

            enemies.Add(id, brute);
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
