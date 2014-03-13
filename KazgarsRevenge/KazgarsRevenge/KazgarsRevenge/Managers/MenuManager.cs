using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to manage all the menuing
    /// </summary>
    public class MenuManager : DrawableGameComponent, IKeyboardSubscriber
    {

        #region Fonts
        public SpriteFont titleFont;
        public SpriteFont normalFont;
        #endregion

        #region Strings
        public static readonly string GAME_TITLE = "KAZGAR'S REVENGE";
        public static readonly string PLAY = "PLAY";
        public static readonly string SETTINGS = "SETTINGS";
        public static readonly string ACCOUNTS = "ACCOUNTS";
        public static readonly string LEVELS = "LEVELS";
        public static readonly string LOADING = "LOADING";
        public static readonly string NEW_ACCOUNT = "New Account";
        #endregion

        public KeyboardDispatcher kd;
        // Screen info
        public Vector2 guiScale;
        public int screenWidth;
        public int screenHeight;

        // The current Menu we're on
        private IMenu currentMenu;

        // All of the previous menus we've been thru
        private Queue<IMenuInfoBox> menuQ;

        // Dictionary so Menus can set up links as needed
        public IDictionary<string, IMenu> menus;

        // I know it's bad to have two sprite batches, but they're used completely separately so it should be ok
        public SpriteBatch sb;

        #region Synchronous Loading
        private LoadingState loadingState;
        // Used to lock the loadingState
        private object lockObj = new Object();
        #endregion

        public MenuManager(KazgarsRevengeGame game)
            : base(game as Game)
        {
            menuQ = new Queue<IMenuInfoBox>();
            menus = new Dictionary<string, IMenu>();
            MainGame mg = Game as MainGame;
            loadingState = LoadingState.NOT_STARTED;

            this.screenWidth = mg.graphics.PreferredBackBufferWidth;
            this.screenHeight = mg.graphics.PreferredBackBufferHeight;

            normalFont = Game.Content.Load<SpriteFont>("Verdana");
            titleFont = Game.Content.Load<SpriteFont>("Title");

            this.kd = Game.Services.GetService(typeof(KeyboardDispatcher)) as KeyboardDispatcher;
            kd.Register(this);
        }

        #region Setup
        private void SetUpMenus()
        {   
            // TODO other menus come up here
            Vector2 titleLoc = new Vector2(screenWidth / 2, screenHeight * .35f);
            Texture2D background = Texture2DUtil.Instance.GetTexture(TextureStrings.Menu.BACKGRND);
            Rectangle backgroundBox = new Rectangle(0, 0, screenWidth, screenHeight);

            NewAccountMenu newAcct = new NewAccountMenu(this, NEW_ACCOUNT, titleLoc, background, backgroundBox, new Vector2(screenWidth / 2f, screenHeight * 0.47f));

            LoadingMenu loading = new LoadingMenu(this, LOADING, titleLoc, background, backgroundBox);

            LevelSelectMenu levelsMenu = new LevelSelectMenu(this, LEVELS, titleLoc, background, backgroundBox, new Vector2(screenWidth / 2f, screenHeight * 0.47f));

            AccountMenu accountMenu = new AccountMenu(this, ACCOUNTS, titleLoc, background, backgroundBox, new Vector2(screenWidth / 2f, screenHeight * 0.47f));

            LinkedMenu title = new LinkedMenu(this, GAME_TITLE, new Vector2(screenWidth / 2, screenHeight * .35f), background, backgroundBox);
            title.AddSelection(new SelectionV2(this, PLAY, new Vector2(screenWidth / 2f, screenHeight * 0.47f)), accountMenu);
            title.AddSelection(new SelectionV2(this, SETTINGS, new Vector2(screenWidth / 2f, screenHeight * 0.55f)), null);
            this.currentMenu = title;
            kd.Register(currentMenu);

            //LoadingMenu loading = new LoadingMenu(sb, LOADING, titleFont, guiScale, new Vector2(screenWidth / 2, screenHeight * .35f));

            menus[GAME_TITLE] = title;
            menus[SETTINGS] = null;
            menus[ACCOUNTS] = accountMenu;
            menus[LEVELS] = levelsMenu;
            menus[LOADING] = loading;
            menus[NEW_ACCOUNT] = newAcct;
        }
        #endregion

        #region Action methods
        /// <summary>
        /// Method to transition to a new menu.
        /// </summary>
        /// <param name="next"></param>
        public void TransitionTo(IMenu next)
        {
            if (next == null)
            {
                return;
            }
            // Get rid of old
            object info = currentMenu.Unload();
            menuQ.Enqueue(new IMenuInfoBox(currentMenu, info));

            // Bring in new
            next.Load(info);
            // Unregister the old current
            kd.Unregister(currentMenu);
            currentMenu = next;

            // Register the new current
            kd.Register(currentMenu);
        }

        /// <summary>
        /// Method to take us back to the last 
        /// menu
        /// </summary>
        public void GoBack()
        {
            if (menuQ.Count <= 0)
            {
                return;
            }

            // Should just be able to toss the return away...
            currentMenu.Unload();

            IMenuInfoBox next = menuQ.Dequeue();

            // Load the old info that was passed in
            next.menu.Load(next.info);
            currentMenu = next.menu;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            sb = new SpriteBatch(Game.GraphicsDevice);

            // Set up guiScale
            int maxX = GraphicsDevice.Viewport.Width;
            int maxY = GraphicsDevice.Viewport.Height;
            float xRatio = maxX / 1920f;
            float yRatio = maxY / 1080f;
            float average = (xRatio + yRatio) / 2;
            guiScale = new Vector2(xRatio, yRatio);

            SetUpMenus();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            sb.Begin();
            currentMenu.Draw(gameTime);
            sb.End();
        }

        private static readonly KeyEvent enterKey = new KeyEvent(Keys.Enter);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            lock (lockObj)
            {
                if (loadingState == LoadingState.NOT_STARTED)
                {
                    // TODO
                    //NormalUpdate(gameTime);
                    return;
                }
            }
            LoadingUpdate(gameTime);
        }

        // Checks to see if we're done loading, if so move on
        private void LoadingUpdate(GameTime gameTime)
        {
            // Don't let people mess with me!
            lock (lockObj)
            {
                if (loadingState == LoadingState.COMPLETE)
                {
                    (Game as MainGame).TransitionToPlaying();
                }
            }
        }

        /// <summary>
        /// Method called by LoadingMenu to being the loading process
        /// </summary>
        /// <param name="name"></param>
        public void LoadLevel(FloorName name)
        {
            // Fire up a new thread for loading
            //new Thread(this.ActualLoad).Start(name);
            this.ActualLoad(name);
        }

        // Does the loading, then sets the loadingStae as Complete
        private void ActualLoad(object name)
        {
            (Game as MainGame).LoadLevel((FloorName)name);
            lock (lockObj)
            {
                loadingState = LoadingState.COMPLETE;
            }
        }

        /// <summary>
        /// Called after Level loading is completed to begin the game
        /// </summary>
        public void AfterLoading()
        {
            //// TODO how does this manager act when we go to playing?
            //this.menuQ.Clear();
            //currentMenu = menus[GAME_TITLE];
            (Game as MainGame).TransitionToPlaying();
        }

        #endregion


        #region Keyboard Subscriber
        // MenuManager only needs to listen for enter key
        public bool ReceiveTextInput(char inputChar)
        {
            return false;
        }

        public bool ReceiveTextInput(string text)
        {
            return false;
        }

        public bool ReceiveCommandInput(char command)
        {
            return false;
        }

        /// <summary>
        /// MenuManager is only listening for the Enter key. If it's pressed
        /// we transition to the selected menu
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ReceiveSpecialInput(Keys key)
        {
            if (key.Equals(Keys.Enter))
            {
                LinkedMenu lm = currentMenu as LinkedMenu;
                if (lm == null)
                {
                    return false;
                }
                TransitionTo(lm.GetSelection());
                return true;
            }
            return false;
        }

        public bool Selected {
            get
            {
                return this.Enabled;
            }
            set
            {
                // Do nothing
            }
        } 
        #endregion
        /// <summary>
        /// Class to box up Menus and info passed to their load method
        /// </summary>
        private class IMenuInfoBox
        {
            // The associated menu
            public IMenu menu;

            // Info to pass to the load method
            public object info;

            public IMenuInfoBox(IMenu menu, object info)
            {
                this.menu = menu;
                this.info = info;
            }
        }

        /// <summary>
        /// Method to let the player manager know who's account is currently being used
        /// </summary>
        /// <param name="account"></param>
        public void SetPlayerAccount(Account account)
        {
            (Game.Services.GetService(typeof(PlayerManager)) as PlayerManager).currentAccount = account;
        }
    }
}
