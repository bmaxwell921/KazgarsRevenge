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
    public abstract class EntityManager : GameComponent
    {
        protected ModelManager modelManager;
        protected SpriteManager spriteManager;
        protected BillBoardManager billboardManager;
        protected GeneralComponentManager genComponentManager;
        protected PlayerManager players;
        protected LevelModelManager levelModelManager;
        protected LevelManager levelManager;
        protected EnemyManager enemyManager;


        protected CameraComponent camera;

        protected KazgarsRevengeGame mainGame;

        public EntityManager(KazgarsRevengeGame game)
            : base(game)
        {
            this.mainGame = game;
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
        }


        public override void Initialize()
        {
            base.Initialize();
            modelManager = Game.Services.GetService(typeof(ModelManager)) as ModelManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            spriteManager = Game.Services.GetService(typeof(SpriteManager)) as SpriteManager;
            billboardManager = Game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager;
            players = Game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            levelModelManager = Game.Services.GetService(typeof(LevelModelManager)) as LevelModelManager;
            levelManager = Game.Services.GetService(typeof(LevelManager)) as LevelManager;
            enemyManager = Game.Services.GetService(typeof(EnemyManager)) as EnemyManager;
            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            effectCellShading = Game.Content.Load<Effect>("Shaders\\CellShader");

        }


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
                //lock (animatedModels)
                //{
                    LoadAnimatedModel(out m, filePath);
                    animatedModels.Add(filePath, m);
                    return m;
                //}
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
                //lock (unanimatedModels)
                //{
                    LoadUnanimatedModel(out m, modelPath);
                    unanimatedModels.Add(modelPath, m);
                    return m;
                //}
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
                        CustomSkinnedEffect newEffect = new CustomSkinnedEffect(toonAnimatedEffect);
                        newEffect.CopyFromSkinnedEffect(skinnedEffect);
                        newEffect.LightPositions = camera.lightPositions;
                        part.Effect = newEffect;
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
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        BasicEffect oldEffect = part.Effect as BasicEffect;

                        Effect newEffect = effectCellShading.Clone();
                        newEffect.Parameters["ColorMap"].SetValue(oldEffect.Texture);
                        newEffect.Parameters["lightPositions"].SetValue(camera.lightPositions);
                        part.Effect = newEffect;
                    }
                }
            }
        }
    }
}
