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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;        
        BoundingBoxDrawer modelDrawer;
        CameraComponent camera;
        ModelManager renderManager;
        SpriteManager spriteManager;
        BillBoardManager decalManager;
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
        Texture2D texCursor;
        Effect effectOutline;
        Effect effectCellShading;
        RenderTarget2D renderTarget;
        RenderTarget2D normalDepthRenderTarget;
        Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
        #endregion

        public RenderTarget2D RenderTarget { get { return renderTarget; } }
        private Thread loadingThread;
        float screenScale = 1;

        

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
            loadingThread = new Thread(DemoLevel);

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
                graphics.PreferredBackBufferHeight = 650;
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

            decalManager = new BillBoardManager(this);
            Components.Add(decalManager);
            Services.AddService(typeof(BillBoardManager), decalManager);

            players = new PlayerManager(this);
            Components.Add(players);
            Services.AddService(typeof(PlayerManager), players);

            lootManager = new LootManager(this);
            Components.Add(lootManager);
            Services.AddService(typeof(LootManager), lootManager);

            nmm = new NetworkMessageManager(this);
            Components.Add(nmm);
            Services.AddService(typeof(NetworkMessageManager), nmm);

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
            texCursor = Content.Load<Texture2D>("Textures\\whiteCursor");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            effectModelDrawer = new BasicEffect(GraphicsDevice);

            effectOutline = Content.Load<Effect>("Shaders\\EdgeDetection");
            effectCellShading = Content.Load<Effect>("shaders\\CellShader");

            SetUpRenderTargets();

            initDrawingParams();

            Texture2D empty=Content.Load<Texture2D>("Textures\\whitePixel");

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
            physics.Update();

            nmm.Update(gameTime);
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            switch (gameState)
            {
                case GameState.StartMenu:
                    if ((keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter)))
                    {
                        if (mainMenuState == mainMenu.PLAY)
                        {
                            gameState = GameState.Loading;
                            loadingThread.Start();
                        }
                        else if (mainMenuState == mainMenu.SETTINGS)
                        {
                            gameState = GameState.Settings;
                        }
                        else if (mainMenuState == mainMenu.NETWORKED)
                        {
                            lm.Log(Level.DEBUG, "Transitioning to ConnectionScreen");
                            gameState = GameState.ConnectionScreen;
                        }
                    }

                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        Exit();
                    }
                                        
                    if ((keyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down)))
                    {
                        if ((int) mainMenuState < numMenuStates - 1)
                        {
                            mainMenuState++;
                        }
                    }

                    if ((keyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up)))
                    {
                        if ((int) mainMenuState > 0)
                        {
                            mainMenuState--;
                        }
                    }
                    break;
                case GameState.Loading:
                    break;
                case GameState.Settings:
                    gameState = GameState.StartMenu;
                    break;
                case GameState.ConnectionScreen:
                    if (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter) && nmm.connections.Count != 0)
                    {
                        lm.Log(Level.DEBUG, "Connecting to Server 0");
                        // TODO change this to proper connection number based on input
                        nmm.ConnectTo(0);
                        gameState = GameState.Lobby;
                    }
                    break;
                case GameState.Lobby:
                    // Since isHost starts as false, we'll only be able to play
                    // once the server gets back to us with whether we're host
                    // this is desired since that message comes with our id as well
                    if (nmm.isHost && keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                    {
                        lm.Log(Level.DEBUG, "Starting game");
                        nmm.StartGame();
                        gameState = GameState.ReceivingMap;
                    }
                    break;
                case GameState.Paused:

                    break;
                case GameState.Playing:
                    for (int i = alertText.Count - 1; i >= 0; ++i)
                    {
                        alertText[i].alpha -= .01f;
                        alertText[i].position.Y -= 1;
                    }
                    base.Update(gameTime);
                    break;
            }

        }

        // TODO ISSUE #7
        private static readonly Identification DUMMY_ID = new Identification(0);
        public void DemoLevel()
        {
            levels.DemoLevel();
            players.CreateMainPlayer(new Vector3(200, 0, -200), DUMMY_ID);
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    enemies.CreateBrute(new Vector3(130 + i * 200, 5, -100 - j * 200));
                }
            }

            gameState = GameState.Playing;
        }

        Vector2 vecLoadingText;
        Rectangle rectMouse;
        Vector2 guiScale = new Vector2(1,1);
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
            rectMouse = new Rectangle(0, 0, 25, 25);
            guiScale = new Vector2(xRatio, yRatio);
            alertStart = new Vector2(maxX, maxY);
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

            //need to move
            Texture2D backgroundTexture;
            int screenWidth = graphics.PreferredBackBufferWidth;//GraphicsDevice.Viewport.Width;
            int screenHeight = graphics.PreferredBackBufferHeight;//GraphicsDevice.Viewport.Height;
            backgroundTexture = Content.Load<Texture2D>("Textures\\Menu\\menuBackground");
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            String titleString = "Kazgar's Revenge";
            String startString = "Start";
            String settingsString = "Settings";
            String loadingString = "Loading";
            String networkingString = "Multiplayer";

            Vector2 titleSize = titleFont.MeasureString(titleString);
            Vector2 startSize = normalFont.MeasureString(startString);
            Vector2 settingsSize = normalFont.MeasureString(settingsString);
            Vector2 loadingSize = normalFont.MeasureString(loadingString);
            Vector2 networkingSize = normalFont.MeasureString(networkingString);

            Vector2 titlePosition = new Vector2(screenWidth / 2, screenHeight * .35f);
            Vector2 startGamePosition = new Vector2(screenWidth / 2, screenHeight * .47f);
            Vector2 loadGamePosition = new Vector2(screenWidth / 2, screenHeight * .47f);
            Vector2 settingsPosition = new Vector2(screenWidth / 2, screenHeight * .55f);
            Vector2 networkingPosition = new Vector2(screenWidth / 2, screenHeight * .61f);

            Vector2 titleOrigin = new Vector2(titleSize.X / 2, titleSize.Y / 2);
            Vector2 startOrigin = new Vector2(startSize.X / 2, startSize.Y / 2);
            Vector2 settingsOrigin = new Vector2(settingsSize.X / 2, settingsSize.Y / 2);
            Vector2 loadingOrigin = new Vector2(loadingSize.X / 2, loadingSize.Y / 2);
            Vector2 networkingOrigin = new Vector2(networkingSize.X / 2, networkingSize.Y / 2);

            Color titleColor = Color.White;
            Color startColor = Color.White;
            Color settingsColor = Color.White;
            Color loadingColor = Color.White;
            Color networkingColor = Color.White;

            switch (gameState)
            {
                case GameState.Playing:
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

                    //draw decals
                    decalManager.Draw();


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




                    //debugging physics
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
                    foreach (FloatingText f in alertText)
                    {
                        spriteBatch.DrawString(normalFont, f.text, f.position, Color.Red, 0, Vector2.Zero, 0, SpriteEffects.None, 0);
                    }
                    spriteBatch.End();
                    break;
                case GameState.StartMenu:
                    if ((int) mainMenuState == 0)
                    {
                        startColor = Color.Red;
                    }
                    else if ((int) mainMenuState == 1)
                    {
                        settingsColor = Color.Red;
                    }
                    else if ((int)mainMenuState == 2)
                    {
                        networkingColor = Color.Red;
                    }
                    

                    spriteBatch.Begin();
                    spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
                    spriteBatch.DrawString(titleFont, titleString, titlePosition, titleColor, 0, titleOrigin, guiScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(normalFont, startString, startGamePosition, startColor, 0, startOrigin, guiScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(normalFont, settingsString, settingsPosition, settingsColor, 0, settingsOrigin, guiScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(normalFont, networkingString, networkingPosition, networkingColor, 0, networkingOrigin, guiScale, SpriteEffects.None, 0);
                    spriteBatch.End();
                    break;
                case GameState.Loading:
                    spriteBatch.Begin();
                    spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
                    spriteBatch.DrawString(titleFont, titleString, titlePosition, titleColor, 0, titleOrigin, guiScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(normalFont, loadingString, loadGamePosition, loadingColor, 0, loadingOrigin, guiScale, SpriteEffects.None, 0);
                    spriteBatch.End();
                    break;
                case GameState.ConnectionScreen:
                    DrawConnectScreen();
                    break;
                case GameState.Lobby:
                    spriteBatch.Begin();
                    string message = (nmm.isHost) ? "Press enter to begin!" : "Waiting for host to start";
                    spriteBatch.DrawString(normalFont, message, vecLoadingText, Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
                    spriteBatch.End();
                    break;
                case GameState.ReceivingMap:
                    spriteBatch.Begin();
                    spriteBatch.DrawString(normalFont, "Receiving Map", vecLoadingText, Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
                    spriteBatch.End();
                    break;
            }
        }

        public void DrawConnectScreen()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(normalFont, "Local Connections", vecLoadingText, Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
            float h = normalFont.MeasureString("Local Connections").Y;
            for (int i = 0; i < nmm.connections.Count; ++i)
            {
                ServerInfo si = nmm.connections[i];
                spriteBatch.DrawString(normalFont, si.ServerName, vecLoadingText + new Vector2(5, h * (i+1)), Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }

        // Overridden so we can let the server know we aren't playing anymore
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);

            nmm.CloseConnection();
        }
    }
}
