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
        RenderManager renderManager;
        EntityManager entityManager;
        GeneralComponentManager genComponentManager;
        SpriteManager spriteManager;
        #endregion


        #region Content
        SpriteFont normalFont;
        BasicEffect effectModelDrawer;
        Texture2D texCursor;
        Effect effectOutline;
        EffectParameter epOutlineThickness;
        EffectParameter epOutlineThreshhold;
        Texture2D toonMap;
        RenderTarget2D renderTarget;
        #endregion

        public Texture2D ToonMap { get { return toonMap; } }
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


            screenScale = ((float)GraphicsDevice.Viewport.Height / 480.0f + (float)GraphicsDevice.Viewport.Width / 800.0f) / 2;

            rand = new Random();

            Entity playerCollidable = new Cylinder(Vector3.Zero, 3, 1, 1);

            camera = new CameraComponent(this);
            Components.Add(camera);
            Services.AddService(typeof(CameraComponent), camera);

            //adding managers
            renderManager = new RenderManager(this);
            Components.Add(renderManager);
            Services.AddService(typeof(RenderManager), renderManager);

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
            StaticMesh ground = new StaticMesh(new Vector3[] { new Vector3(-500, 0, -500), new Vector3(500, 0, -500), new Vector3(-500, 0, 500), new Vector3(500, 0, 500) }, new int[] { 0, 1, 2, 2, 1, 3 });
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

            effectOutline = Content.Load<Effect>("Shaders\\LineShader");
            toonMap = Content.Load<Texture2D>("Textures\\Toon");

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24Stencil8);


            /*
             * terrainGraphics.LoadTerrain("bang.jt");


            terrainPhysics = new Terrain(terrainGraphics.HeightData, new AffineTransform(
                    new Vector3(1, 1, -1),
                    Quaternion.Identity,
                    Vector3.Zero));

            physics.Add(terrainPhysics);
             */

            initDrawingParams();


            epOutlineThickness = effectOutline.Parameters["Thickness"];
            epOutlineThickness.SetValue(.5f);
            epOutlineThreshhold = effectOutline.Parameters["Threshold"];
            epOutlineThreshhold.SetValue(.5f);

            base.LoadContent();
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
            for (int i = 0; i < 25; ++i)
            {
                for (int j = 0; j < 25; ++j)
                {
                    //entityManager.CreateFred(new Vector3(130 + i * 40, 10, -100 - j * 40));
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
                    
                    GraphicsDevice.SetRenderTarget(renderTarget);
                    GraphicsDevice.Clear(Color.DarkSlateBlue);

                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    
                    //base.Draw(gameTime);
                    renderManager.Draw(gameTime);

                    GraphicsDevice.SetRenderTarget(null);

                    Texture2D SceneTexture = renderTarget;
                    // Render the scene with Edge Detection, using the render target from last frame.
                    GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);


                    //spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                    {
                        // Apply the post process shader
                        effectOutline.CurrentTechnique.Passes[0].Apply();
                        {

                            spriteBatch.Draw(SceneTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                        }
                    }
                    spriteBatch.End();

                    effectModelDrawer.LightingEnabled = false;
                    effectModelDrawer.VertexColorEnabled = true;
                    effectModelDrawer.World = Matrix.Identity;
                    effectModelDrawer.View = camera.View;
                    effectModelDrawer.Projection = camera.Projection;
                    modelDrawer.Draw(effectModelDrawer, physics);
                    

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
