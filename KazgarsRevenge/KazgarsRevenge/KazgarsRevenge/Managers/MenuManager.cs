using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to manage all the menuing
    /// </summary>
    public class MenuManager : DrawableGameComponent
    {
        /*
         * To add in MenuManger:
         *  1) Add MenuManager initialization to MainGame.Initialize();
         *  2) Change Update code
         *      a) Delete all of the update code except the initial stuff and the GameState.Playing stuff
         *      b) Pull base.Update(gameTime) outside of the GameState.Playing case
         *  3) Change Draw code
         *      a) Delete all the Draw code except for the initial stuff and the GameState.Playing stuff
         *      b) In the not GameState.Playing do base.Draw(gameTime)
         *      
         * TODO next:
         *  - Update moving between menus (unless Andy gets me my models....)
         */ 
        #region Fonts
        private SpriteFont titleFont;
        private SpriteFont normalFont;
        #endregion

        #region Strings
        private static readonly string GAME_TITLE = "KAZGAR'S REVENGE";
        private static readonly string PLAY = "PLAY";
        private static readonly string SETTINGS = "SETTINGS";
        private static readonly string ACCOUNTS = "ACCOUNTS";
        private static readonly string LEVELS = "LEVELS";
        private static readonly string LOADING = "LOADING";
        #endregion

        private int screenWidth;

        private int screenHeight;

        // The current Menu we're on
        private IMenu currentMenu;

        // All of the previous menus we've been thru
        private Queue<IMenu> menuQ;

        // I don't like that these are here
        private KeyboardState keyboardState;
        private KeyboardState prevKeyboardState;

        // I know it's bad to have two sprite batches, but they're used completely separately so it should be ok
        private SpriteBatch sb;

        public MenuManager(KazgarsRevengeGame game)
            : base(game as Game)
        {
            menuQ = new Queue<IMenu>();

            MainGame mg = Game as MainGame;
            this.screenWidth = mg.graphics.PreferredBackBufferWidth;
            this.screenHeight = mg.graphics.PreferredBackBufferHeight;

            normalFont = Game.Content.Load<SpriteFont>("Verdana");
            titleFont = Game.Content.Load<SpriteFont>("Title");
        }

        #region Setup
        private void SetUpMenus()
        {
            // HACK
            int maxX = GraphicsDevice.Viewport.Width;
            int maxY = GraphicsDevice.Viewport.Height;
            float xRatio = maxX / 1920f;
            float yRatio = maxY / 1080f;
            float average = (xRatio + yRatio) / 2;
            Vector2 guiScale = new Vector2(xRatio, yRatio);


            // TODO other menus come up here
            LoadingMenu loading = new LoadingMenu(sb, LOADING, titleFont, guiScale, new Vector2(screenWidth / 2, screenHeight * .35f));

            SelectionMenu title = new SelectionMenu(sb, GAME_TITLE, titleFont, guiScale, new Vector2(screenWidth / 2, screenHeight * .35f), 
                new Rectangle(0,0,screenWidth,screenHeight), Game.Content.Load<Texture2D>(@"Textures\Menu\menuBackground"));  
            // TODO Change 2nd arg of MenuTransitionAction to an actual menu
            title.AddSelection(new TextSelection(sb, PLAY, normalFont, guiScale, new KeyEvent(Keys.Enter), new MenuTransitionAction(this, null), 
                new Vector2(screenWidth / 2f, screenHeight * 0.47f)));
            title.AddSelection(new TextSelection(sb, SETTINGS, normalFont, guiScale, new KeyEvent(Keys.Enter), new MenuTransitionAction(this, null), 
                new Vector2(screenWidth / 2f, screenHeight * 0.55f)));
            title.SelectFirst();
            this.currentMenu = title;
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
            currentMenu.Unload();
            menuQ.Enqueue(currentMenu);

            // Bring in new
            next.Load();
            currentMenu = next;
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

            this.TransitionTo(menuQ.Dequeue());
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            sb = new SpriteBatch(Game.GraphicsDevice);
            SetUpMenus();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            sb.Begin();
            currentMenu.Draw(gameTime);
            sb.End();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            prevKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            
            // Get all the events to send
            IList<IEvent> events = new List<IEvent>();
            GetKeyEvents(events);
            GetMouseEvent(events);

            // Send em
            foreach (IEvent e in events)
            {
                currentMenu.HandleEvent(e);
            }

        }

        // Adds any keyEvents
        private void GetKeyEvents(IList<IEvent> events)
        {
            foreach (Keys pressed in keyboardState.GetPressedKeys())
            {
                if (prevKeyboardState.IsKeyUp(pressed)) 
                {
                    events.Add(new KeyEvent(pressed));
                }
            }
        }

        // Adds any mouseEvent
        private void GetMouseEvent(IList<IEvent> events)
        {
            MouseState state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Pressed)
            {
                events.Add(new MouseEvent(new Vector2(state.X, state.Y)));
            }
        }

        #endregion
    }
}
