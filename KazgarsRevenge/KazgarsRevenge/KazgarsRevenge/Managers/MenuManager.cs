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

        public MenuManager(KazgarsRevengeGame game)
            : base(game as Game)
        {
            menuQ = new Queue<IMenu>();       
        }

        #region Setup
        private void SetUpMenus()
        {
            MainGame mg = Game as MainGame;
            // TODO other menus come up here
            LoadingMenu loading = new LoadingMenu(mg.spriteBatch, LOADING, titleFont, mg.guiScale, new Vector2(screenWidth / 2, screenHeight * .35f));

            SelectionMenu title = new SelectionMenu(mg.spriteBatch, GAME_TITLE, titleFont, mg.guiScale, new Vector2(screenWidth / 2, screenHeight * .35f));  
            // TODO Change 2nd arg of MenuTransitionAction to an actual menu
            title.AddSelection(new TextSelection(mg.spriteBatch, PLAY, normalFont, mg.guiScale, new KeyEvent(Keys.Enter), new MenuTransitionAction(this, null), 
                new Vector2(screenWidth / 2f, screenHeight * 0.47f)));
            title.AddSelection(new TextSelection(mg.spriteBatch, SETTINGS, normalFont, mg.guiScale, new KeyEvent(Keys.Enter), new MenuTransitionAction(this, null), 
                new Vector2(screenWidth / 2f, screenHeight * 0.55f))); 

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
            MainGame mg = Game as MainGame;
            this.screenWidth = mg.graphics.PreferredBackBufferWidth;
            this.screenHeight = mg.graphics.PreferredBackBufferHeight;

            normalFont = Game.Content.Load<SpriteFont>("Verdana");
            titleFont = Game.Content.Load<SpriteFont>("Title");

            SetUpMenus();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            currentMenu.Draw(gameTime);
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
