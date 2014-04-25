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
using System.Threading;
using EventInput;

namespace KazgarsRevenge
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : KazgarsRevengeGame
    {
        #region Settings
        public enum SettingAmount
        {
            Low,
            Medium,
            High,
        }
        private struct GameSettings
        {
            public SettingAmount particleSettings;
        }
        private GameSettings settings;
        public SettingAmount ParticlesSetting { get { return settings.particleSettings; } }
        #endregion

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
        //NetworkMessageManager nmm;
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
            GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate;

            settings.particleSettings = SettingAmount.Low;

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
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
                graphics.IsFullScreen = false;
                graphics.ApplyChanges();
                screenScale = ((float)GraphicsDevice.Viewport.Height / graphics.PreferredBackBufferHeight + (float)GraphicsDevice.Viewport.Width / graphics.PreferredBackBufferWidth) / 2;
            }

            Texture2DUtil.Instance.SetContent(Content);

            // Adds a keyboard to listen for key events
            Services.AddService(typeof(KeyboardDispatcher), new KeyboardDispatcher(this.Window));
            Services.AddService(typeof(KeyboardLayout), new KeyboardLayout());

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

            soundEffectLibrary = new SoundEffectLibrary(this);
            Services.AddService(typeof(SoundEffectLibrary), soundEffectLibrary);
            Components.Add(soundEffectLibrary);

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

            SetInGameManagersEnabled(false);
            base.Initialize();
        }

        /// <summary>
        /// Same as super method. If new XNA components are added make sure 
        /// you figure out if they should always update, or only update when
        /// the game is playing
        /// </summary>
        /// <param name="enabled"></param>
        public override void SetInGameManagersEnabled(bool enabled)
        {
            camera.Enabled = enabled;
            renderManager.Enabled = enabled;
            spriteManager.Enabled = enabled;
            billboards.Enabled = enabled;
            players.Enabled = enabled;
            enemies.Enabled = enabled;
            levels.Enabled = enabled;
            attacks.Enabled = enabled;
            particleManager.Enabled = enabled;
            levelModelManager.Enabled = enabled;
        }

        protected void SetUpLoggers()
        {
            lm = new LoggerManager();
            // Log to both the console and a file
            lm.AddLogger(new FileWriteLogger(FileWriteLogger.CLIENT_SUB_DIR));
            //lm.AddLogger(new ConsoleLogger());
            Services.AddService(typeof(LoggerManager), lm);

            ChunkUtil.Instance.SetLoggerManager(lm);
        }

        protected override void LoadContent()
        {
            Texture2DUtil.Instance.PreemptiveLoad();
            normalFont = Content.Load<SpriteFont>("Verdana");
            titleFont = Content.Load<SpriteFont>("Title");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            effectModelDrawer = new BasicEffect(GraphicsDevice);

            effectOutline = Content.Load<Effect>("Shaders\\EdgeDetection");

            effectCellShading = Content.Load<Effect>("Shaders\\CellShader");

            SetUpRenderTargets();

            GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(resetOutputTarget);

            initDrawingParams();

            base.LoadContent();
        }

        void resetOutputTarget(object sender, EventArgs e)
        {
            if (!renderTarget.IsDisposed)
            {
                renderTarget.Dispose();
            }
            if (!normalDepthRenderTarget.IsDisposed)
            {
                normalDepthRenderTarget.Dispose();
            }
            SetUpRenderTargets();
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

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;
        }


        string alertMessage = "";
        double alertTimeLeft = 0;
        float alertSourceX = 0;
        List<FloatingText> floatingText = new List<FloatingText>();
        Vector2 alertStart = Vector2.Zero;
        public void AddAlert(string text)
        {
            alertMessage = text;
            alertTimeLeft = 2500;
            alertSourceX = normalFont.MeasureString(alertMessage).X * average / 2;
        }

        public void AddFloatingText(FloatingText floater)
        {
            floatingText.Add(floater);
        }

        const bool debug = true;
        int loadInt = 0;
        int frameCounter = 0;
        int frameRate = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        protected override void Update(GameTime gameTime)
        {
            if (gameState == GameState.Playing)
            {
                if (debug)
                {
                    elapsedTime += gameTime.ElapsedGameTime;
                    if (elapsedTime > TimeSpan.FromSeconds(1))
                    {
                        elapsedTime -= TimeSpan.FromSeconds(1);
                        frameRate = frameCounter;
                        frameCounter = 0;
                    }
                }

                physics.Update();
                double millis = gameTime.ElapsedGameTime.TotalMilliseconds;
                if (alertTimeLeft > 0)
                {
                    alertTimeLeft -= millis;
                }
                for (int i = floatingText.Count - 1; i >= 0; --i)
                {
                    floatingText[i].Update(millis);
                    if (floatingText[i].Dead)
                    {
                        floatingText.RemoveAt(i);
                    }
                }
            }

            if (gameState == GameState.Loading)
            {
                ++loadInt;
                if (loadInt > 3)
                {
                    ActuallyLoadLevel(this.loadFloor);
                    TransitionToPlaying();
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
            SetInGameManagersEnabled(true);
            gameState = GameState.Playing;
        }

        private FloorName loadFloor;

        /// <summary>
        /// Starts the loading of a level with the given FloorName
        /// </summary>
        /// <param name="name"></param>
        public void LoadNewLevel(FloorName name)
        {
            gameState = GameState.Loading;
            loadFloor = name;
            createNewPlayer = true;
        }

        bool createNewPlayer = true;
        /// <summary>
        /// Assume that the player has already been created, so just reset its position
        /// </summary>
        public void LoadNextLevel(FloorName name)
        {
            if (levels.currentLevel.currentFloor == name)
            {
                return;
            }
            gameState = GameState.Loading;
            loadFloor = name;
            createNewPlayer = false;
        }

        public void ActuallyLoadLevel(FloorName name)
        {
            levels.CreateLevel(name);
            if (createNewPlayer)
            {
                players.CreateMainPlayerInLevel(DUMMY_ID);
            }
            else
            {
                players.StartNewLevel();
            }
            camera.StartLevel(name);
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
            if (gameState == GameState.Loading)
            {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();
                spriteBatch.DrawString(normalFont, "LOADING", new Vector2(50, 50), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                
                spriteBatch.End();
            }
            else if (gameState == GameState.Playing)
            {

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
                GraphicsDevice.BlendState = BlendState.NonPremultiplied;
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
                //spriteBatch.DrawString(normalFont, ""+camera.zoom, new Vector2(50, 100), Color.Red);
                //spriteBatch.DrawString(normalFont, players.GetDebugString(), new Vector2(200, 200), Color.Yellow);
                foreach (FloatingText f in floatingText)
                {
                    Vector3 pos = GraphicsDevice.Viewport.Project(f.position, camera.Projection, camera.View, Matrix.Identity);
                    Vector2 screenPos = new Vector2(pos.X, pos.Y);
                    spriteBatch.DrawString(normalFont, f.text, screenPos, f.color * f.alpha, 0, Vector2.Zero, f.scale, SpriteEffects.None, 0);
                }

                if (alertTimeLeft > 0)
                {
                    spriteBatch.DrawString(normalFont, alertMessage, new Vector2(maxX / 2, 172 * average), Color.Red * (float)(alertTimeLeft / 2500), 0, new Vector2(alertSourceX, 0), .5f * average, SpriteEffects.None, 0);
                }

                if (debug)
                {
                    ++frameCounter;
                    spriteBatch.DrawString(normalFont, frameRate + " FPS", new Vector2(maxX / 2, maxY / 2), Color.Red);
                }
                spriteBatch.End();

            }
            else
            {
                base.Draw(gameTime);
            }
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
