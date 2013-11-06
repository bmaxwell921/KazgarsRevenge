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
    abstract class EntityManager : GameComponent
    {
        protected ModelManager modelManager;
        protected SpriteManager spriteManager;
        protected GeneralComponentManager genComponentManager;
        protected MainGame mainGame;


        public EntityManager(MainGame game)
            : base(game)
        {
            mainGame = game as MainGame;
        }

        #region Models
        protected Effect toonAnimatedEffect;
        protected Effect effectCellShading;
        protected Dictionary<string, Model> animatedModels = new Dictionary<string, Model>();
        protected Dictionary<string, Model> unanimatedModels = new Dictionary<string, Model>();

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

            modelManager = Game.Services.GetService(typeof(ModelManager)) as ModelManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            spriteManager = Game.Services.GetService(typeof(SpriteManager)) as SpriteManager;

            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            effectCellShading = Game.Content.Load<Effect>("Shaders\\CellShader");

        }
    }
}
