using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionRuleManagement;

using BEPUphysicsDrawer;
using BEPUphysicsDrawer.Lines;
using KazgarsRevenge.Libraries;

namespace KazgarsRevenge
{
    enum GameState
    {
        Playing,
        Paused,
        StartMenu,
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Game
    {
        #region components
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Space physics;
        BoundingBoxDrawer modelDrawer;
        CameraComponent camera;
        ModelManager renderManager;
        GeneralComponentManager genComponentManager;
        SpriteManager spriteManager;
        LevelManager levels;
        PlayerManager players;
        EnemyManager enemies;
        AttackManager attacks;
        NetworkMessageManager networkMessages;
        SoundEffectLibrary soundEffectLibrary;
        #endregion

        #region Content
        SpriteFont normalFont;
        BasicEffect effectModelDrawer;
        Texture2D texCursor;
        Effect effectOutline;
        Effect effectCellShading;
        RenderTarget2D renderTarget;
        RenderTarget2D normalDepthRenderTarget;
        Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
        #endregion

        public RenderTarget2D RenderTarget { get { return renderTarget; } }

        float screenScale = 1;
        GameState gameState = GameState.StartMenu;
        Random rand;
        


        public MainGame()
        {
            Window.Title = "Kazgar's Revenge";

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            InitPhysicsStuff();
        }

        public static CollisionGroup GoodProjectileCollisionGroup;
        public static CollisionGroup PlayerCollisionGroup;
        public static CollisionGroup BadProjectileCollisionGroup;
        public static CollisionGroup EnemyCollisionGroup;
        protected void InitPhysicsStuff()
        {
            //physics! yay!
            physics = new Space();

            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < 10 * Environment.ProcessorCount; ++i)
                {
                    //threads! woo!
                    physics.ThreadManager.AddThread();
                }
            }
            physics.ForceUpdater.Gravity = new Vector3(0, -80f, 0);
            Services.AddService(typeof(Space), physics);


            //collision groups
            GoodProjectileCollisionGroup = new CollisionGroup();
            PlayerCollisionGroup = new CollisionGroup();
            BadProjectileCollisionGroup = new CollisionGroup();
            EnemyCollisionGroup = new CollisionGroup();

            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(PlayerCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, PlayerCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(GoodProjectileCollisionGroup, GoodProjectileCollisionGroup), CollisionRule.NoBroadPhase);
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(BadProjectileCollisionGroup, BadProjectileCollisionGroup), CollisionRule.NoBroadPhase);
        }

        protected override void Initialize()
        {
            /*graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();*/

            
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
            
            
            screenScale = ((float)GraphicsDevice.Viewport.Height / 480.0f + (float)GraphicsDevice.Viewport.Width / 800.0f) / 2;

            rand = new Random();

            Entity playerCollidable = new Cylinder(Vector3.Zero, 3, 1, 1);

            camera = new CameraComponent(this);
            Components.Add(camera);
            Services.AddService(typeof(CameraComponent), camera);

            //adding managers
            renderManager = new ModelManager(this);
            Components.Add(renderManager);
            Services.AddService(typeof(ModelManager), renderManager);

            genComponentManager = new GeneralComponentManager(this);
            Components.Add(genComponentManager);
            Services.AddService(typeof(GeneralComponentManager), genComponentManager);

            spriteManager = new SpriteManager(this);
            Components.Add(spriteManager);
            Services.AddService(typeof(SpriteManager), spriteManager);

            players = new PlayerManager(this);
            Components.Add(players);
            Services.AddService(typeof(PlayerManager), players);

            enemies = new EnemyManager(this);
            Components.Add(enemies);
            Services.AddService(typeof(EnemyManager), enemies);

            levels = new LevelManager(this);
            Components.Add(levels);
            Services.AddService(typeof(LevelManager), levels);

            attacks = new AttackManager(this);
            Components.Add(attacks);
            Services.AddService(typeof(AttackManager), attacks);

            networkMessages = new NetworkMessageManager(this);
            Components.Add(networkMessages);
            Services.AddService(typeof(NetworkMessageManager), networkMessages);

            soundEffectLibrary = new SoundEffectLibrary(this);
            Services.AddService(typeof(SoundEffectLibrary), soundEffectLibrary);

            //debug drawing
            modelDrawer = new BoundingBoxDrawer(this);


            base.Initialize();
        }

        protected override void LoadContent()
        {
            //adding large rectangle for units to stand on
            //StaticMesh ground = new StaticMesh(new Vector3[] { new Vector3(-2000, 0, -2000), new Vector3(2000, 0, -2000), new Vector3(-2000, 0, 2000), new Vector3(2000, 0, 2000) }, new int[] { 0, 1, 2, 2, 1, 3 });
            //physics.Add(ground);


            normalFont = Content.Load<SpriteFont>("Georgia");
            texCursor = Content.Load<Texture2D>("Textures\\whiteCursor");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            effectModelDrawer = new BasicEffect(GraphicsDevice);

            effectOutline = Content.Load<Effect>("Shaders\\EdgeDetection");
            effectCellShading = Content.Load<Effect>("shaders\\CellShader");

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, 
                                                pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                pp.BackBufferFormat, pp.DepthStencilFormat);

            normalDepthRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                                                         pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                         pp.BackBufferFormat, pp.DepthStencilFormat);

            initDrawingParams();

            base.LoadContent();
        }

        string attachDir = "Models\\Attachables\\";
        public AttachableModel GetAttachable(string modelName, string textureName, string otherAttachPoint)
        {
            AttachableModel m;
            if (attachables.TryGetValue(modelName + otherAttachPoint, out m))
            {
                return m;
            }
            else
            {
                Model mod = Content.Load<Model>(attachDir + modelName);
                Texture2D modTex = Content.Load<Texture2D>(attachDir + textureName);

                foreach (ModelMesh mesh in mod.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = effectCellShading.Clone();
                        part.Effect.Parameters["ColorMap"].SetValue(modTex);
                    }
                }
                m = new AttachableModel(mod, otherAttachPoint);
                attachables.Add(modelName + otherAttachPoint, m);
                return m;
            }
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            physics.Update();

            switch (gameState)
            {
                case GameState.StartMenu:
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        gameState = GameState.Playing;
                        DemoLevel();
                    }
                    break;
                case GameState.Paused:

                    break;
                case GameState.Playing:
                    base.Update(gameTime);
                    break;
            }

        }


        public void DemoLevel()
        {
            levels.DemoLevel();
            players.CreateMainPlayer(new Vector3(200, 0, -200));
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    enemies.CreateBrute(new Vector3(130 + i * 100, 0, -100 - j * 100));
                }
            }
        }

        Vector2 vecLoadingText;
        Rectangle rectMouse;
        private void initDrawingParams()
        {
            vecLoadingText = new Vector2(50, 50);
            rectMouse = new Rectangle(0, 0, 25, 25);
        }

        protected override void Draw(GameTime gameTime)
        {
            MouseState curMouse = Mouse.GetState();
            rectMouse.X = curMouse.X;
            rectMouse.Y = curMouse.Y;

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;
            switch (gameState)
            {
                case GameState.Playing:
                    //draw depth render target
                    GraphicsDevice.SetRenderTarget(normalDepthRenderTarget);
                    GraphicsDevice.Clear(Color.Black);
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    renderManager.Draw(gameTime, true);

                    //draw scene render target
                    GraphicsDevice.SetRenderTarget(renderTarget);
                    GraphicsDevice.Clear(Color.Black);
                    renderManager.Draw(gameTime, false);
                    GraphicsDevice.SetRenderTarget(null);

                    
                    //pass in depth render target to the edge detection shader
                    Texture2D normalDepthTexture = normalDepthRenderTarget;
                    effectOutline.Parameters["ScreenResolution"].SetValue(new Vector2(renderTarget.Width, renderTarget.Height));
                    effectOutline.Parameters["NormalDepthTexture"].SetValue(normalDepthTexture);
                    effectOutline.CurrentTechnique = effectOutline.Techniques["EdgeDetect"];
                    //draw scene
                    spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effectOutline);
                    spriteBatch.Draw(renderTarget, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                    spriteBatch.End();

                    /*
                    effectModelDrawer.LightingEnabled = false;
                    effectModelDrawer.VertexColorEnabled = true;
                    effectModelDrawer.World = Matrix.Identity;
                    effectModelDrawer.View = camera.View;
                    effectModelDrawer.Projection = camera.Projection;
                    modelDrawer.Draw(effectModelDrawer, physics);
                    */

                    spriteBatch.Begin();
                    spriteManager.Draw(spriteBatch);

                    //debug strings
                    //spriteBatch.DrawString(normalFont, "zoom: " + camera.zoom, new Vector2(50, 50), Color.Yellow);
                    //spriteBatch.DrawString(normalFont, players.GetDebugString(), new Vector2(200, 200), Color.Yellow);
                    spriteBatch.Draw(texCursor, rectMouse, Color.White);
                    spriteBatch.End();
                    break;
                case GameState.StartMenu:
                    spriteBatch.Begin();
                    spriteBatch.DrawString(normalFont, "Press Space To Start", vecLoadingText, Color.Yellow);
                    spriteBatch.End();
                    break;
            }
        }
    }
}
