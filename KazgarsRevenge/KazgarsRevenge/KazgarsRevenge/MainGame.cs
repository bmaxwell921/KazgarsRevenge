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

using BEPUphysicsDrawer;
using BEPUphysicsDrawer.Lines;

namespace KazgarsRevenge
{
    enum GameState
    {
        Playing,
        Paused,
        Loading,
        LoadingFinished,
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        #region components
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Space physics;
        BoundingBoxDrawer modelDrawer;
        CameraComponent camera;
        ModelManager renderManager;
        EntityManager entityManager;
        GeneralComponentManager genComponentManager;
        SpriteManager spriteManager;
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
        GameState gameState = GameState.LoadingFinished;
        Random rand;


        public MainGame()
        {
            Window.Title = "Kazgar's Revenge";

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";


            physics = new Space();

            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < 10 * Environment.ProcessorCount; ++i)
                {
                    physics.ThreadManager.AddThread();
                }
            }
            physics.ForceUpdater.Gravity = new Vector3(0, -80f, 0);
            Services.AddService(typeof(Space), physics);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            entityManager = new EntityManager(this);
            Components.Add(entityManager);
            Services.AddService(typeof(EntityManager), entityManager);

            //adding large rectangle for units to stand on
            StaticMesh ground = new StaticMesh(new Vector3[] { new Vector3(-2000, 0, -2000), new Vector3(2000, 0, -2000), new Vector3(-2000, 0, 2000), new Vector3(2000, 0, 2000) }, new int[] { 0, 1, 2, 2, 1, 3 });
            physics.Add(ground);

            //debug drawing
            modelDrawer = new BoundingBoxDrawer(this);


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
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

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            physics.Update();

            switch (gameState)
            {
                case GameState.Loading:

                    break;
                case GameState.LoadingFinished:
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
            entityManager.CreateMainPlayer(new Vector3(200, 10, -200));
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    entityManager.CreateBrute(new Vector3(130 + i * 100, 10, -100 - j * 100));
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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
                    renderManager.Draw(gameTime, "NormalDepth");

                    //draw scene render target
                    GraphicsDevice.SetRenderTarget(renderTarget);
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    renderManager.Draw(gameTime, "Toon");
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

                    //spriteBatch.DrawString(normalFont, "zoom: " + camera.zoom, new Vector2(50, 50), Color.Yellow);
                    //spriteBatch.DrawString(normalFont, players.GetDebugString(), new Vector2(200, 200), Color.Yellow);
                    spriteBatch.Draw(texCursor, rectMouse, Color.White);
                    spriteBatch.End();
                    break;
                case GameState.Loading:
                    spriteBatch.Begin();
                    spriteBatch.DrawString(normalFont, "Loading", vecLoadingText, Color.Yellow);
                    spriteBatch.End();
                    break;
                case GameState.LoadingFinished:
                    spriteBatch.Begin();
                    spriteBatch.DrawString(normalFont, "Press Space To Start", vecLoadingText, Color.Yellow);
                    spriteBatch.End();
                    break;
            }
        }
    }
}
