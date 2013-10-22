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
    class EntityManager : GameComponent
    {
        ModelManager renderManager;
        EntityManager entityManager;
        GeneralComponentManager genComponentManager;
        SpriteManager spriteManager;

        private List<GameEntity> entities = new List<GameEntity>();

        public EntityManager(MainGame game)
            : base(game)
        {

        }

        public override void Update(GameTime gameTime)
        {
            for (int i = entities.Count - 1; i >= 0; --i)
            {
                if (entities[i].Dead)
                {
                    entities.RemoveAt(i);
                }
            }
        }

        Effect toonAnimatedEffect;
        #region Models and SkinningData
        Model modelArrow;
        SkinningData skinningDataArrow;
        Model modelPlayer;
        SkinningData skinningDataPlayer;
        Model modelTestEnemy;
        SkinningData skinningDataTestEnemy;
        #endregion

        public override void Initialize()
        {

            renderManager = Game.Services.GetService(typeof(ModelManager)) as ModelManager;
            entityManager = Game.Services.GetService(typeof(EntityManager)) as EntityManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            spriteManager = Game.Services.GetService(typeof(SpriteManager)) as SpriteManager;

            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            //LoadModel(out modelArrow, "Models\\Enemies\\walk");
            //skinningDataArrow = modelArrow.Tag as SkinningData;

            LoadModel(out modelPlayer, "Models\\Player\\idle1");
            skinningDataPlayer = modelPlayer.Tag as SkinningData;

            //LoadModel(out modelTestEnemy, "Models\\Enemies\\walk");
            //skinningDataTestEnemy = modelTestEnemy.Tag as SkinningData;
        }

        public void LoadModel(out Model model, string filePath)
        {
            model = Game.Content.Load<Model>(filePath);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    SkinnedEffect skinnedEffect = part.Effect as SkinnedEffect;
                    if (skinnedEffect != null)
                    {
                        // Create new custom skinned effect from our base effect
                        CustomSkinnedEffect custom = new CustomSkinnedEffect(toonAnimatedEffect);
                        custom.CopyFromSkinnedEffect(skinnedEffect);
                        part.Effect = custom;
                    }
                }
            }
        }

        //each method will, in effect, create a new type of entity by adding whatever components you want?
        #region Entity Adding
        public void AddEntity(GameEntity toAdd)
        {
            entities.Add(toAdd);
        }

        public void CreateMainPlayer(Vector3 position)
        {
            GameEntity player = new GameEntity("player", "good");

            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            //locking rotation on axis (so that bumping into something won't make the player tip over)
            playerPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            //more accurate collision detection for the player
            playerPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            //need the collisioninformation's tag to be set, to get the entity in a collision handler
            playerPhysicalData.CollisionInformation.Tag = player;

            //giving a reference to the player's physical data to the camera, so it can follow the player aorund
            (Game.Services.GetService(typeof(CameraComponent)) as CameraComponent).AssignEntity(playerPhysicalData);

            //shared animation data (need this to be in the player controller component as well as the graphics component, so that the controller can determine when to play animations)
            AnimationPlayer playerAnimations = new AnimationPlayer(skinningDataPlayer);
            
            List<AttachableModel> attachables = new List<AttachableModel>();
            attachables.Add((Game as MainGame).GetAttachable("sword1", "RootNode"));

            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(Game as MainGame, playerPhysicalData);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(Game as MainGame, playerPhysicalData, modelPlayer, playerAnimations, new Vector3(2f), Vector3.Zero, attachables);
            HealthComponent playerHealth = new HealthComponent(Game as MainGame, 100);
            PlayerInputComponent playerController = new PlayerInputComponent(Game as MainGame, playerPhysicalData, playerAnimations);

            //adding the controllers to their respective managers 
            //(need to decide what kinds of components need their own managers; currently 
            //it's just a 2d renderer, a 3d renderer, and a general manager)
            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            renderManager.AddComponent(playerGraphics);

            player.AddComponent(typeof(HealthComponent), playerHealth);
            genComponentManager.AddComponent(playerHealth);
            
            player.AddComponent(typeof(PlayerInputComponent), playerController);
            //needs redesign
            spriteManager.AddComponent(playerController);

            AddEntity(player);
        }

        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage)
        {
            GameEntity newArrow = new GameEntity("arrow", "players");

            Entity arrowData = new Box(position, 1, 2, 5, 1);
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));

            AnimationPlayer arrowAnimations = new AnimationPlayer(skinningDataArrow);


            PhysicsComponent arrowPhysics = new PhysicsComponent(Game as MainGame, arrowData);
            AnimatedModelComponent arrowGraphics = new AnimatedModelComponent(Game as MainGame, arrowData, modelArrow, arrowAnimations, new Vector3(.1f), Vector3.Zero, new List<AttachableModel>());
            ArrowController arrowAI = new ArrowController(Game as MainGame, arrowData, damage);

            newArrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            newArrow.AddComponent(typeof(AnimatedModelComponent), arrowGraphics);
            renderManager.AddComponent(arrowGraphics);

            newArrow.AddComponent(typeof(ArrowController), arrowAI);
            genComponentManager.AddComponent(arrowAI);
        }

        public void CreateFred(Vector3 position)
        {
            GameEntity fred = new GameEntity("Fred", "bad");

            Entity fredPhysicalData = new Box(position, 4f, 10f, 4f, 1);
            fredPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            fredPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            fredPhysicalData.CollisionInformation.Tag = fred;

            AnimationPlayer fredAnimations = new AnimationPlayer(skinningDataTestEnemy);

            PhysicsComponent fredPhysics = new PhysicsComponent(Game as MainGame, fredPhysicalData);
            AnimatedModelComponent fredGraphics = new AnimatedModelComponent(Game as MainGame, fredPhysicalData, modelTestEnemy, fredAnimations, new Vector3(.5f), Vector3.Zero, new List<AttachableModel>());
            HealthComponent fredHealth = new HealthComponent(Game as MainGame, 100);

            fred.AddComponent(typeof(PhysicsComponent), fredPhysics);
            genComponentManager.AddComponent(fredPhysics);

            fred.AddComponent(typeof(AnimatedModelComponent), fredGraphics);
            renderManager.AddComponent(fredGraphics);

            fred.AddComponent(typeof(HealthComponent), fredHealth);
            genComponentManager.AddComponent(fredHealth);

            AddEntity(fred);
        }
        #endregion

        #region Helpers
        private Matrix CreateRotationFromForward(Vector3 forward)
        {
            Matrix rotation = Matrix.Identity;
            rotation.Forward = Vector3.Normalize(forward);
            rotation.Right = Vector3.Normalize(Vector3.Cross(rotation.Forward, Vector3.Up));
            rotation.Up = Vector3.Up;
            return rotation;
        }
        #endregion
    }
}
