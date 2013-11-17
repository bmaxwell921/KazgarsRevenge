using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public abstract class ModelCreator : GameComponent
    {
        protected KazgarsRevengeGame mainGame;
        public ModelCreator(KazgarsRevengeGame game)
            : base(game)
        {
            mainGame = game;
        }

        public override void Initialize()
        {
            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            effectCellShading = Game.Content.Load<Effect>("Shaders\\CellShader");
        }

        protected Effect toonAnimatedEffect;
        protected Effect effectCellShading;
        protected Dictionary<string, Model> animatedModels = new Dictionary<string, Model>();
        protected Dictionary<string, Model> unanimatedModels = new Dictionary<string, Model>();
        protected Dictionary<string, Model> skinnedModels = new Dictionary<string, Model>();

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

        public Model GetUnanimatedModel(string modelPath)
        {
            Model m;
            if (unanimatedModels.TryGetValue(modelPath, out m))
            {
                return m;
            }
            else
            {
                LoadUnanimatedModel(out m, modelPath);
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

        public void LoadUnanimatedModel(out Model model, string modelPath)
        {
            model = Game.Content.Load<Model>(modelPath);

            //if this model has already been loaded, don't process its textures again
            if (model.Meshes[0].Effects[0] is BasicEffect)
            {
                List<Texture2D> modelTextures = new List<Texture2D>();
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect eff in mesh.Effects)
                    {
                        modelTextures.Add(eff.Texture);
                    }
                }

                int i = 0;
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = effectCellShading.Clone();
                        part.Effect.Parameters["ColorMap"].SetValue(modelTextures[i++]);
                        if (i >= modelTextures.Count)
                        {
                            i = modelTextures.Count - 1;
                        }
                    }
                }
            }
        }
    }
}
