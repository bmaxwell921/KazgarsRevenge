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
        RenderManager renderManager;
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
        #endregion

        public override void Initialize()
        {

            renderManager = Game.Services.GetService(typeof(RenderManager)) as RenderManager;
            entityManager = Game.Services.GetService(typeof(EntityManager)) as EntityManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            spriteManager = Game.Services.GetService(typeof(SpriteManager)) as SpriteManager;

            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            LoadModel(out modelArrow, "Models\\Player\\idle");
            skinningDataArrow = modelArrow.Tag as SkinningData;

            LoadModel(out modelPlayer, "Models\\Player\\idle");
            skinningDataPlayer = modelPlayer.Tag as SkinningData;
        }

        protected void LoadModel(out Model model, string filePath)
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

            Entity playerPhysicalData = new Cylinder(position, 3, 1, 1);
            (Game.Services.GetService(typeof(CameraComponent)) as CameraComponent).AssignEntity(playerPhysicalData);
            AnimationPlayer playerAnimations = new AnimationPlayer(skinningDataPlayer);

            PhysicsComponent playerPhysics = new PhysicsComponent(Game as MainGame, playerPhysicalData);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(Game as MainGame, playerPhysicalData, modelPlayer, playerAnimations);
            HealthComponent playerHealth = new HealthComponent(Game as MainGame, 100);
            PlayerInputComponent playerController = new PlayerInputComponent(Game as MainGame, playerPhysicalData, playerAnimations);

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
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));

            AnimationPlayer arrowAnimations = new AnimationPlayer(skinningDataArrow);


            PhysicsComponent arrowPhysics = new PhysicsComponent(Game as MainGame, arrowData);
            AnimatedModelComponent arrowGraphics = new AnimatedModelComponent(Game as MainGame, arrowData, modelArrow, arrowAnimations);
            ArrowController arrowAI = new ArrowController(Game as MainGame, arrowData, damage);

            newArrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            newArrow.AddComponent(typeof(AnimatedModelComponent), arrowGraphics);
            renderManager.AddComponent(arrowGraphics);

            newArrow.AddComponent(typeof(ArrowController), arrowAI);
            genComponentManager.AddComponent(arrowAI);
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
