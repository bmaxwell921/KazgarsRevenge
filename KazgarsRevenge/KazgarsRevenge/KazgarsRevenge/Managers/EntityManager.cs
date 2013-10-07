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
        #endregion

        public override void Initialize()
        {
            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            LoadModel(out modelArrow, "idle");
            skinningDataArrow = modelArrow.Tag as SkinningData;
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

        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage)
        {
            GameEntity newArrow = new GameEntity("Arrow", "Players");

            Entity arrowPhysicalData = new Box(position, 1, 2, 5, 1);

            arrowPhysicalData.LinearVelocity = initialTrajectory;

            Matrix rotation = Matrix.Identity;
            rotation.Forward = Vector3.Normalize(initialTrajectory);
            rotation.Right = Vector3.Normalize(Vector3.Cross(rotation.Forward, Vector3.Up));
            rotation.Up = Vector3.Up;

            arrowPhysicalData.Orientation = Quaternion.CreateFromRotationMatrix(rotation);

            PhysicsComponent arrowPhysics = new PhysicsComponent(Game as MainGame, arrowPhysicalData);
            AnimatedModelComponent arrowGraphics = new AnimatedModelComponent(Game as MainGame, arrowPhysicalData, modelArrow, skinningDataArrow);
            ArrowController arrowAI = new ArrowController(Game as MainGame, arrowPhysicalData, damage);
        }

        #endregion

    }
}
