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
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class EntityManager : GameComponent
    {
        ModelManager renderManager;
        EntityManager entityManager;
        GeneralComponentManager genComponentManager;
        SpriteManager spriteManager;
        MainGame mainGame;

        private List<GameEntity> entities = new List<GameEntity>();

        public EntityManager(MainGame game)
            : base(game)
        {
            mainGame = game as MainGame;
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
        Effect effectCellShading;
        #region Models
        Dictionary<string, Model> animatedModels = new Dictionary<string, Model>();
        Dictionary<string, Model> unanimatedModels = new Dictionary<string, Model>();

        public Model GetAnimatedModel(string filePath)
        {
            Model m;
            if (animatedModels.TryGetValue(filePath, out m))
            {
                return m;
            }
            else
            {
                LoadAnimatedModel(out m, filePath);
                animatedModels.Add(filePath, m);
                return m;
            }
        }

        public Model GetUnanimatedModel(string modelPath, string texturePath)
        {
            Model m;
            if (unanimatedModels.TryGetValue(modelPath, out m))
            {
                return m;
            }
            else
            {
                LoadUnanimatedModel(out m, modelPath, texturePath);
                unanimatedModels.Add(modelPath, m);
                return m;
            }
        }

        public void LoadAnimatedModel(out Model model, string filePath)
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

        public void LoadUnanimatedModel(out Model model, string modelPath, string texturePath)
        {
            model = Game.Content.Load<Model>(modelPath);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effectCellShading.Clone();
                    part.Effect.Parameters["ColorMap"].SetValue(Game.Content.Load<Texture2D>(texturePath));
                }
            }
        }
        #endregion

        public override void Initialize()
        {

            renderManager = Game.Services.GetService(typeof(ModelManager)) as ModelManager;
            entityManager = Game.Services.GetService(typeof(EntityManager)) as EntityManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            spriteManager = Game.Services.GetService(typeof(SpriteManager)) as SpriteManager;

            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            effectCellShading = Game.Content.Load<Effect>("Shaders\\CellShader");

        }

        //each method will, in effect, create a new type of entity by adding whatever components you want
        #region Entities - characters
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

            Model playerModel = GetAnimatedModel("Models\\Player\\k_idle1");
            //shared animation data (need this to be in the player controller component as well as the graphics component, so that the controller can determine when to play animations)
            AnimationPlayer playerAnimations = new AnimationPlayer(playerModel.Tag as SkinningData);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            

            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, playerPhysicalData);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, playerPhysicalData, playerModel, playerAnimations, new Vector3(10f), Vector3.Down * 18, attachables);
            HealthComponent playerHealth = new HealthComponent(mainGame, 100);
            PlayerInputComponent playerController = new PlayerInputComponent(mainGame, player, playerPhysicalData, playerAnimations, attachables);

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

        public void CreateBrute(Vector3 position)
        {
            GameEntity brute = new GameEntity("Brute", "bad");

            Entity brutePhysicalData = new Box(position, 12f, 37f, 12f, 100);
            brutePhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            brutePhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            brutePhysicalData.CollisionInformation.Tag = brute;
            brutePhysicalData.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0));

            Model bruteModel = GetAnimatedModel("Models\\Enemies\\Pigman\\pig_idle");
            AnimationPlayer bruteAnimations = new AnimationPlayer(bruteModel.Tag as SkinningData);

            PhysicsComponent brutePhysics = new PhysicsComponent(mainGame, brutePhysicalData);
            AnimatedModelComponent bruteGraphics = new AnimatedModelComponent(mainGame, brutePhysicalData, bruteModel, bruteAnimations, new Vector3(10f), Vector3.Down * 18, new Dictionary<string, AttachableModel>());
            HealthComponent bruteHealth = new HealthComponent(mainGame, 100);

            BruteController bruteController = new BruteController(mainGame, brute, bruteHealth, brutePhysicalData, bruteAnimations);

            brute.AddComponent(typeof(PhysicsComponent), brutePhysics);
            genComponentManager.AddComponent(brutePhysics);

            brute.AddComponent(typeof(AnimatedModelComponent), bruteGraphics);
            renderManager.AddComponent(bruteGraphics);

            brute.AddComponent(typeof(HealthComponent), bruteHealth);
            genComponentManager.AddComponent(bruteHealth);

            brute.AddComponent(typeof(BruteController), bruteController);
            spriteManager.AddComponent(bruteController);
            
            AddEntity(brute);
        }
        #endregion

        #region Entities - attacks
        Matrix arrowGraphicRot = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0);
        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage, string factionToHit)
        {
            GameEntity newArrow = new GameEntity("arrow", "good");

            Entity arrowData = new Box(position, 10, 47, 10, .01f);
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrowData);
            UnanimatedModelComponent arrowGraphics = 
                new UnanimatedModelComponent(mainGame, GetUnanimatedModel("Models\\Attachables\\arrow", "Models\\Attachables\\sword"),
                    arrowData, new Vector3(10), Vector3.Zero, arrowGraphicRot);

            ProjectileController arrowAI = new ProjectileController(mainGame, newArrow, arrowData, damage, 3000, factionToHit);

            newArrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            newArrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            renderManager.AddComponent(arrowGraphics);

            newArrow.AddComponent(typeof(ProjectileController), arrowAI);
            genComponentManager.AddComponent(arrowAI);
        }

        public void CreateMelleAttack(Vector3 position, int damage, string factionToHit)
        {
            GameEntity newAttack = new GameEntity("arrow", "good");

            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, attackData);

            ProjectileController attackAI = new ProjectileController(mainGame, newAttack, attackData, damage, 300, factionToHit);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(ProjectileController), attackAI);
            genComponentManager.AddComponent(attackAI);
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
