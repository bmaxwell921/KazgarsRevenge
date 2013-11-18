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
        LootManager gearGenerator;

        PlayerManager players;
        NetworkMessageManager nmm;
        SoundEffectLibrary soundEffectLibrary;

        protected LevelManager levels;
        protected EnemyManager enemies;
        protected AttackManager attacks;
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


        public MainGame()
        {
            gameState = GameState.StartMenu;
            Window.Title = "Kazgar's Revenge";

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            /*graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();*/

            
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            
            
            screenScale = ((float)GraphicsDevice.Viewport.Height / 480.0f + (float)GraphicsDevice.Viewport.Width / 800.0f) / 2;

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

            players = new PlayerManager(this);
            Components.Add(players);
            Services.AddService(typeof(PlayerManager), players);

            gearGenerator = new LootManager(this);
            Components.Add(gearGenerator);
            Services.AddService(typeof(LootManager), gearGenerator);

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

            //debug drawing
            modelDrawer = new BoundingBoxDrawer(this);


            base.Initialize();
        }

        protected override void LoadContent()
        {
            normalFont = Content.Load<SpriteFont>("Verdana");
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

            Texture2D empty=Content.Load<Texture2D>("Textures\\whitePixel");

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
                m = new AttachableModel(attacks.GetUnanimatedModel(attachDir + modelName), otherAttachPoint);
                attachables.Add(modelName + otherAttachPoint, m);
                return m;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            physics.Update();

            switch (gameState)
            {
                case GameState.StartMenu:
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        //gameState = GameState.Lobby;

                        gameState = GameState.Playing;
                        DemoLevel();
                    }
                    break;
                case GameState.Lobby:
                    nmm.Update(gameTime);
                    break;
                case GameState.Paused:

                    break;
                case GameState.Playing:
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
            //players.CreateDude();
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
        float guiScale = 1;
        private void initDrawingParams()
        {
            vecLoadingText = new Vector2(50, 50);
            rectMouse = new Rectangle(0, 0, 25, 25);
            float xRatio = GraphicsDevice.Viewport.Width / 1920f;
            float yRatio = GraphicsDevice.Viewport.Height / 1080f;
            guiScale = (xRatio + yRatio) / 2;
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
                    spriteBatch.DrawString(normalFont, "Press Space To Start", vecLoadingText, Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
                    spriteBatch.End();
                    break;
                case GameState.Lobby:
                    DrawLobbyScreen();
                    break;
            }
        }

        public void DrawLobbyScreen()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(normalFont, "Local Connections", vecLoadingText, Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
            float h = normalFont.MeasureString("Local Connections").Y;
            for (int i = 0; i < nmm.connections.Count; ++i)
            {
                ServerInfo si = nmm.connections[i];
                spriteBatch.DrawString(normalFont, "\t" + si.ServerName, vecLoadingText + new Vector2(0, h * i), Color.Yellow, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }
    }
}
