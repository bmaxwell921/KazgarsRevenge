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
using System.Threading;

namespace KazgarsRevenge
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : KazgarsRevengeGame
    {
        #region components
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;        
        BoundingBoxDrawer modelDrawer;
        CameraComponent camera;
        ModelManager renderManager;
        SpriteManager spriteManager;
        BillBoardManager billboards;
        LootManager lootManager;
        LevelModelManager levelModelManager;

        PlayerManager players;
        NetworkMessageManager nmm;
        SoundEffectLibrary soundEffectLibrary;

        protected LevelManager levels;
        protected EnemyManager enemies;
        protected AttackManager attacks;

        private LoggerManager lm;

        ParticleManager particleManager;
        #endregion

        #region Content
        SpriteFont normalFont;
        SpriteFont titleFont;

        enum mainMenu { PLAY, SETTINGS, NETWORKED };
        mainMenu mainMenuState;
        int numMenuStates;
        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;

        BasicEffect effectModelDrawer;
        Effect effectOutline;
        Effect effectCellShading;
        RenderTarget2D renderTarget;
        RenderTarget2D normalDepthRenderTarget;
        Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
        public RenderTarget2D RenderTarget { get { return renderTarget; } }
        float screenScale = 1;
        #endregion


        public MainGame()
        {
            gameState = GameState.StartMenu;
            Window.Title = "Kazgar's Revenge";

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // LoggerManager created first since it doesn't rely on anything and everyone will want to use it
            SetUpLoggers();

            bool fullscreen = false;

            if (fullscreen)
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = true;
                graphics.ApplyChanges();
            }
            else
            {
                graphics.PreferredBackBufferWidth = 800;
                graphics.PreferredBackBufferHeight = 600;
                graphics.IsFullScreen = false;
                graphics.ApplyChanges();
                screenScale = ((float)GraphicsDevice.Viewport.Height / 480.0f + (float)GraphicsDevice.Viewport.Width / 800.0f) / 2;
            }

            mainMenuState = mainMenu.PLAY;
            numMenuStates = Enum.GetNames(typeof(mainMenu)).Length;

            Entity playerCollidable = new Cylinder(Vector3.Zero, 3, 1, 1);

            camera = new CameraComponent(this);
            Components.Add(camera);
            Services.AddService(typeof(CameraComponent), camera);

            //adding managers
            renderManager = new ModelManager(this);
            Components.Add(renderManager);
            Services.AddService(typeof(ModelManager), renderManager);

            spriteManager = new SpriteManager(this);
            Components.Add(spriteManager);
            Services.AddService(typeof(SpriteManager), spriteManager);

            billboards = new BillBoardManager(this);
            Components.Add(billboards);
            Services.AddService(typeof(BillBoardManager), billboards);

            players = new PlayerManager(this);
            Components.Add(players);
            Services.AddService(typeof(PlayerManager), players);

            lootManager = new LootManager(this);
            Components.Add(lootManager);
            Services.AddService(typeof(LootManager), lootManager);

            nmm = new NetworkMessageManager(this);
            Components.Add(nmm);
            Services.AddService(typeof(NetworkMessageManager), nmm);

            MessageSender ms = new MessageSender(nmm.Client, (LoggerManager)Services.GetService(typeof(LoggerManager)));
            Services.AddService(typeof(MessageSender), ms);

            soundEffectLibrary = new SoundEffectLibrary(this);
            Services.AddService(typeof(SoundEffectLibrary), soundEffectLibrary);

            enemies = new EnemyManager(this);
            Components.Add(enemies);
            Services.AddService(typeof(EnemyManager), enemies);

            levels = new LevelManager(this);
            Components.Add(levels);
            Services.AddService(typeof(LevelManager), levels);

            attacks = new AttackManager(this);
            Components.Add(attacks);
            Services.AddService(typeof(AttackManager), attacks);

            particleManager = new ParticleManager(this);
            Components.Add(particleManager);
            Services.AddService(typeof(ParticleManager), particleManager);

            levelModelManager = new LevelModelManager(this);
            Components.Add(levelModelManager);
            Services.AddService(typeof(LevelModelManager), levelModelManager);

            MenuManager mm = new MenuManager(this);
            Components.Add(mm);
            Services.AddService(typeof(MenuManager), mm);

            //debug drawing
            modelDrawer = new BoundingBoxDrawer(this);
            base.Initialize();
        }

        protected void SetUpLoggers()
        {
            lm = new LoggerManager();
            // Log to both the console and a file
            lm.AddLogger(new FileWriteLogger(FileWriteLogger.CLIENT_SUB_DIR));
            lm.AddLogger(new ConsoleLogger());
            Services.AddService(typeof(LoggerManager), lm);
        }

        protected override void LoadContent()
        {
            normalFont = Content.Load<SpriteFont>("Verdana");
            titleFont = Content.Load<SpriteFont>("Title");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            effectModelDrawer = new BasicEffect(GraphicsDevice);

            effectOutline = Content.Load<Effect>("Shaders\\EdgeDetection");
            effectCellShading = Content.Load<Effect>("Shaders\\CellShader");

            SetUpRenderTargets();

            initDrawingParams();

            Texture2D empty = Content.Load<Texture2D>("Textures\\whitePixel");


            //deferred stuff
            effectClearBuffer = Content.Load<Effect>("Shaders\\Deferred\\ClearGBufferShader");
            effectClearBuffer.Parameters["ClearColor"].SetValue(Color.Black.ToVector3());


            lightBlendState = new BlendState();
            lightBlendState.AlphaBlendFunction = BlendFunction.Add;
            lightBlendState.AlphaSourceBlend = Blend.One;
            lightBlendState.AlphaDestinationBlend = Blend.One;
            lightBlendState.ColorBlendFunction = BlendFunction.Add;
            lightBlendState.ColorSourceBlend = Blend.One;
            lightBlendState.ColorDestinationBlend = Blend.One;


            effectPointLight = Content.Load<Effect>("Shaders\\Deferred\\PointLight");
            effectDirectionalLight = Content.Load<Effect>("Shaders\\Deferred\\DirectionalLight");

            for (int i = 0; i < 1; ++i)
            {
                for (int j = 0; j < 1; ++j)
                {
                    PointLightEffect newLight = new PointLightEffect(effectPointLight);
                    newLight.LightPosition = new Vector3(-25 + i * 10, 50, -25 + j * 10);
                    //lights.Add(newLight);
                }
            }

            DirectionalLightEffect dirLight = new DirectionalLightEffect(effectDirectionalLight);
            //lights.Add(dirLight);


            effectFinalCombine = new FinalCombineEffect(Content.Load<Effect>("Shaders\\Deferred\\FinalCombineShader"));

            base.LoadContent();
        }

        private void SetUpRenderTargets()
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice,
                                                pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                pp.BackBufferFormat, pp.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);

            normalDepthRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                                                         pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                         pp.BackBufferFormat, pp.DepthStencilFormat);
        }

        List<FloatingText> alertText = new List<FloatingText>();
        Vector2 alertStart = Vector2.Zero;
        public void AddAlert(string text)
        {
            alertText.Add(new FloatingText(alertStart, text));
        }
        protected override void Update(GameTime gameTime)
        {

            nmm.Update(gameTime);
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            if (gameState == GameState.Playing)
            {
                physics.Update();
                for (int i = alertText.Count - 1; i >= 0; ++i)
                {
                    alertText[i].alpha -= .01f;
                    alertText[i].position.Y -= 1;
                }
            }
            base.Update(gameTime);
        }

        // TODO ISSUE #7
        private static readonly Identification DUMMY_ID = new Identification(0, Identification.NO_CLIENT);

        /// <summary>
        /// Tells the game to transition to the playing state
        /// </summary>
        /// <param name="name"></param>
        public void TransitionToPlaying()
        {
            gameState = GameState.Playing;
        }

        /// <summary>
        /// Starts the loading of a level with the given FloorName
        /// </summary>
        /// <param name="name"></param>
        public void LoadLevel(FloorName name)
        {
            //levels.CreateLevel(name);
            levels.DemoLevel();

            // TODO put him somewhere useful
            players.CreateMainPlayer(new Vector3(120, 20, 120), DUMMY_ID);

            // TODO get rid of this for better spawning mechanics
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    enemies.CreateBrute(IdentificationFactory.getId(EntityType.NormalEnemy, Identification.NO_CLIENT), new Vector3(130 + i * 300, 20, -100 - j * 300), 1);
                }
            }
        }

        Vector2 vecLoadingText;

        public Vector2 guiScale = new Vector2(1,1);
        int maxX;
        int maxY;
        float average;
        private void initDrawingParams()
        {
            maxX = GraphicsDevice.Viewport.Width;
            maxY = GraphicsDevice.Viewport.Height;
            float xRatio = maxX / 1920f;
            float yRatio = maxY / 1080f;
            average = (xRatio + yRatio) / 2;

            vecLoadingText = new Vector2(50, 50);
            guiScale = new Vector2(xRatio, yRatio);
            alertStart = new Vector2(maxX, maxY);
        }

        protected override void Draw(GameTime gameTime)
        {
            MouseState curMouse = Mouse.GetState();

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;

            if (gameState == GameState.Playing)
            {
                bool deferred = true;
                if (deferred)
                {
                    DrawDeferred(gameTime);
                }
                else
                {
                    if (renderTarget.IsContentLost || normalDepthRenderTarget.IsContentLost)
                    {
                        SetUpRenderTargets();
                    }

                    //draw depth render target
                    GraphicsDevice.SetRenderTarget(normalDepthRenderTarget);
                    GraphicsDevice.Clear(Color.Black);
                    GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    renderManager.Draw(gameTime, true);

                    //draw scene render target
                    GraphicsDevice.SetRenderTarget(renderTarget);
                    GraphicsDevice.Clear(Color.Black);
                    levelModelManager.Draw(gameTime, false);
                    renderManager.Draw(gameTime, false);

                    //draw particles
                    particleManager.Draw(gameTime);

                    GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                    //draw billboarded stuff
                    billboards.Draw();


                    //reset graphics device
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




                    //physics debugging
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
                    foreach (FloatingText f in alertText)
                    {
                        spriteBatch.DrawString(normalFont, f.text, f.position, Color.Red, 0, Vector2.Zero, 0, SpriteEffects.None, 0);
                    }
                    spriteBatch.End();
                }
            }
            else
            {
                base.Draw(gameTime);
            }
        }

        #region deferred variables
        Effect effectClearBuffer;
        VertexPositionTexture[] screenVerts = new VertexPositionTexture[]
            {
                new VertexPositionTexture(
                    new Vector3(1,-1,0),
                    new Vector2(1,1)),
                new VertexPositionTexture(
                    new Vector3(-1,-1,0),
                    new Vector2(0,1)),
                new VertexPositionTexture(
                    new Vector3(-1,1,0),
                    new Vector2(0,0)),
                new VertexPositionTexture(
                    new Vector3(1,1,0),
                    new Vector2(1,0))
            };
        short[] screenIndices = new short[] { 0, 1, 2, 2, 3, 0 };

        BlendState lightBlendState;

        Effect effectPointLight;
        Effect effectDirectionalLight;
        List<LightEffect> lights = new List<LightEffect>();

        FinalCombineEffect effectFinalCombine;
        #endregion
        private void DrawDeferred(GameTime gameTime)
        {
            //clear
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(null);

            //rendery stuff
            GBufferTarget gbuffer = camera.RenderTargets;
            DeferredBegin(gbuffer);
            DeferredDraw(gameTime);
            DeferredEnd(gbuffer);

            //draw the final image
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(camera.OutputTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            
            //rendertarget debugging
            /*
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.BlendState = BlendState.Opaque;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(camera.RenderTargets.Normals, Vector2.Zero, Color.White);
            spriteBatch.End();
            */



            //physics debugging
            /*
            effectModelDrawer.LightingEnabled = false;
            effectModelDrawer.VertexColorEnabled = true;
            effectModelDrawer.World = Matrix.Identity;
            effectModelDrawer.View = camera.View;
            effectModelDrawer.Projection = camera.Projection;
            modelDrawer.Draw(effectModelDrawer, physics);
            */

            //draw particles
            particleManager.Draw(gameTime);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            //draw billboarded stuff
            billboards.Draw();


            spriteBatch.Begin();
            spriteManager.Draw(spriteBatch);
            foreach (FloatingText f in alertText)
            {
                spriteBatch.DrawString(normalFont, f.text, f.position, Color.Red, 0, Vector2.Zero, 0, SpriteEffects.None, 0);
            }
            spriteBatch.End();

        }

        private void DeferredBegin(GBufferTarget gbuffer)
        {
            /*
             * Begin 
             */
            RenderTarget2D[] renderTargets = gbuffer.GetRenderTargets();
            GraphicsDevice.SetRenderTargets(renderTargets[0], renderTargets[1], renderTargets[2], renderTargets[3]);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            //clear all buffers of data
            effectClearBuffer.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, screenVerts, 0, 4, screenIndices, 0, 2);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void DeferredDraw(GameTime gameTime)
        {
            /*
             * Draw
             */
            renderManager.Draw(gameTime, false);
            levelModelManager.Draw(gameTime, false);

            //Resolve GBuffer
            GraphicsDevice.SetRenderTarget(null);

            //begin light pass
            GraphicsDevice.SetRenderTarget(camera.RenderTargets.Lighting);
            GraphicsDevice.BlendState = lightBlendState;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.Clear(Color.Transparent);

            //Draw lights
            foreach (LightEffect light in lights)
            {
                light.ColorMap = camera.RenderTargets.Albedo;
                light.NormalMap = camera.RenderTargets.Normals;
                light.HighlightsMap = camera.RenderTargets.Highlights;
                light.DepthMap = camera.RenderTargets.Depth;
                light.CameraPosition = camera.Position;
                light.HalfPixel = camera.RenderTargets.HalfPixel;
                light.InverseViewProjection = camera.InverseViewProj;

                foreach (EffectPass pass in light.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleList, screenVerts, 0, 4, screenIndices, 0, 2);
            }
        }

        private void DeferredEnd(GBufferTarget gbuffer)
        {
            /*
             * End
             */
            GraphicsDevice.SetRenderTarget(camera.OutputTarget);

            GraphicsDevice.BlendState = BlendState.Opaque;

            effectFinalCombine.ColorMap = gbuffer.Albedo;
            effectFinalCombine.HighlightMap = gbuffer.Highlights;
            effectFinalCombine.LightMap = gbuffer.Lighting;
            effectFinalCombine.HalfPixel = gbuffer.HalfPixel;

            effectFinalCombine.Apply();


            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, screenVerts, 0, 4, screenIndices, 0, 2);
        }

        // Overridden so we can let the server know we aren't playing anymore
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            if (players.myId != null)
            {
                //((MessageSender)Services.GetService(typeof(MessageSender))).CloseConnection(players.myId.id);
            }
        }
    }
}
