using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    class Menu
    {
        public const float ButtonRepeatTimeout = 100;
        public const float StickRepeatTimeout = 250;
        public const float StickDirectionSelectionMin = 0.6f;

        private static Menu instance = new Menu();

        public double buttonPressedAt = 0;
        public GamePadState lastGPState = GamePad.GetState(PlayerIndex.One);
        public KeyboardState lastKBState = Keyboard.GetState();
        public double elementSelectedAt = 0;

        // are we waiting for b-button to be released
        private bool waitForButtonRelease = false;

        private Menu()
        {
        }

        internal static Menu Instance
        {
            get { return instance; }
        }

        internal void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);

            background = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/background");

            mainMenu = new MainMenu(this);
            releaseNotesMenu = new ReleaseNotesMenu(this);
        }

        internal void Update(GameTime gameTime)
        {
            Game.Instance.Profiler.BeginSection("menu_update");

            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            if (active)
            {
                activeScreen.Update(gameTime);

                if (at > buttonPressedAt + Menu.ButtonRepeatTimeout)
                {
                    if ((gamePadState.Buttons.Start == ButtonState.Pressed
                        && lastGPState.Buttons.Start == ButtonState.Released)
                        || (keyboardState.IsKeyDown(Keys.Escape)
                        && lastKBState.IsKeyUp(Keys.Escape)))
                    {
                        Close();
                        buttonPressedAt = at;
                    }
                    else
                        if ((gamePadState.Buttons.B == ButtonState.Pressed
                            && lastGPState.Buttons.B == ButtonState.Released)
                            || (keyboardState.IsKeyDown(Keys.Back)
                            && lastKBState.IsKeyUp(Keys.Back)))
                        {
                            CloseActiveMenuScreen();
                            buttonPressedAt = at;
                        }
                }
            }
            else
            {
                if (at > buttonPressedAt + Menu.ButtonRepeatTimeout
                    && ((gamePadState.Buttons.Start == ButtonState.Pressed
                        && lastGPState.Buttons.Start == ButtonState.Released)
                        || (keyboardState.IsKeyDown(Keys.Escape)
                        && lastKBState.IsKeyUp(Keys.Escape))))
                {
                    Game.Instance.Pause();
                    Open();
                    buttonPressedAt = at;
                }
                else
                {
                    // resume simulation as soon as button we used to close is released (so we don't accidentally fire)
                    if (Game.Instance.Paused
                        && waitForButtonRelease
                        && gamePadState.Buttons.B == ButtonState.Released
                        && gamePadState.Buttons.A == ButtonState.Released)
                    {
                        Game.Instance.Resume();
                        waitForButtonRelease = false;
                    }
                }
            }

            lastGPState = gamePadState;
            Game.Instance.Profiler.EndSection("menu_update");
        }

        internal void Draw(GameTime gameTime)
        {
            if (active)
            {
                int screenWidth = Game.Instance.GraphicsDevice.Viewport.Width;
                float screenscale = (float)screenWidth / 1280f;
                Matrix spriteScale = Matrix.CreateScale(screenscale, screenscale, 1);

                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                    SaveStateMode.None, spriteScale);

                spriteBatch.Draw(background, new Vector2(0, 0), Color.White);

                // first traversal: sum up total width of all screens
                int totalWidth = 0;

                foreach(MenuScreen screen in screens)
                {
                    if (screen is ItemizedMenuScreen)
                    {
                        totalWidth += (screen as ItemizedMenuScreen).Width;
                    }                    
                }

                // second traversal (backwards): compute individual x offsets and draw the screens
                int offset = 0;
                LinkedListNode<MenuScreen> node = screens.Last;
                do
                {
                    MenuScreen screen = node.Value;
                    screen.DrawOffset.TargetValue = offset + totalWidth / 2;
                    if (screen is ItemizedMenuScreen)
                    {
                        (screen as ItemizedMenuScreen).Active = (screen == screens.Last.Value);
                        screen.DrawOffset.TargetValue -= (screens.Last.Value as ItemizedMenuScreen).Width / 2;
                    }
                    //screen.DrawOffset.TargetValue = offset + totalWidth/2 - (screens.Last.Value as ItemizedMenuScreen).Width/2;
                    
                    screen.Draw(gameTime, spriteBatch);
                    if (screen is ItemizedMenuScreen)
                    {
                        offset -= (screen as ItemizedMenuScreen).Width;
                    }
                    node = node.Previous;

                    if (!screen.DrawPrevious)
                        break;
                }
                while (node != null);

                spriteBatch.End();
            }
        }

        public void OpenMenuScreen(MenuScreen screen)
        {
            screen.OnOpen();
            screen.DrawOffset.TargetValue = 0f;
            screen.DrawOffset.Value = 0f;
            screens.AddLast(screen);
            activeScreen = screen;
        }

        public void CloseActiveMenuScreen()
        {
            if (activeScreen == mainMenu)
            {
                Close();
            }
            else if (activeScreen == releaseNotesMenu)
            {
                screens.Last.Value.OnClose();
                screens.RemoveLast();
                Open();
            }
            else
            {
                screens.Last.Value.OnClose();
                screens.RemoveLast();
                activeScreen = screens.Last.Value;
            }
        }

        public void Open()
        {
            active = true;
            OpenMenuScreen(mainMenu);
        }

        public void OpenReleaseNotes()
        {
            active = true;
            OpenMenuScreen(releaseNotesMenu);
        }

        public void Close()
        {
            // only allow closing of menu when players are in play
            if (Game.Instance.Simulation.PlayerManager.Count > 0)
            {
                active = false;
                screens.Clear();

                // don't resume simulation yet, but only after a-button is released
                waitForButtonRelease = true;
            }
        }

        private bool active = false;

        public bool Active
        {
            get { return active; }
        }


        private MenuScreen activeScreen = null;
        private readonly LinkedList<MenuScreen> screens = new LinkedList<MenuScreen>();

        private MainMenu mainMenu;
        private ReleaseNotesMenu releaseNotesMenu;

        private SpriteBatch spriteBatch;

        private Texture2D background;
    }
}