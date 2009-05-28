using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectMagma.MathHelpers;

namespace ProjectMagma
{
    public class Menu
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
            StaticStringStrength = new SineFloat(0.7f, 1.0f, 2.5f);
            StaticStringStrength.Start(0.001);
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
            LevelMenu = new LevelMenu(this);
            SettingsMenu = new SettingsMenu(this);
            HelpMenu = new HelpMenu(this);
            CreditsMenuPage1 = new CreditsMenuPage1(this);
            CreditsMenuPage2 = new CreditsMenuPage2(this);
            PlayerMenu = new PlayerMenu(this, LevelMenu);
            releaseNotesMenu = new ReleaseNotesMenu(this);
            StaticStringFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/menu_releasenotes");
            StaticStringFontSmall = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/menu_releasenotes_small");

        }

        internal void Update(GameTime gameTime)
        {
            Game.Instance.Profiler.BeginSection("menu_update");

            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            StaticStringStrength.Update(gameTime.TotalRealTime.TotalMilliseconds);
            if (active)
            {
                MenuScreen currentActiveScreen = activeScreen;

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
                            CloseActiveMenuScreen(true);
                            buttonPressedAt = at;
                        }
                        else
                            if ((gamePadState.Buttons.Y == ButtonState.Pressed
                                && lastGPState.Buttons.Y == ButtonState.Released))
                            {
                                Hidden = true;
                            }
                            else
                                if ((gamePadState.Buttons.Y == ButtonState.Released
                                    && lastGPState.Buttons.Y == ButtonState.Pressed))
                                {
                                    Hidden = false;
                                }
                }

                if (currentActiveScreen != activeScreen)
                {
                    activeScreen.Update(gameTime);
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
            if (Active && !Hidden)
            {
                int screenWidth = Game.Instance.GraphicsDevice.Viewport.Width;
                float screenscale = (float)screenWidth / 1280f;
                Matrix spriteScale = Matrix.CreateScale(screenscale, screenscale, 1);

                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                    SaveStateMode.None, spriteScale);

                spriteBatch.Draw(background, new Vector2(0, 0), new Color(125,125,125,160));
                //spriteBatch.Draw(

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

                // HACK: retrospectively, this DrawPrevious thing was a stupid idea. whoever wants to
                // refactor this at some point, should get rid of this property and such ugly conditionals.
                if ( (screens.Last.Value is ItemizedMenuScreen && (screens.Last.Value as ItemizedMenuScreen).DrawPrevious)
                  || (screens.Last.Value is PlayerMenu))
                {
                    DrawStaticStrings();
                }

                spriteBatch.End();
            }
        }

        private void DrawStaticStrings()
        {
            DrawTools.DrawCenteredShadowString(spriteBatch, StaticStringFont, "- A - SELECT                                                  - B - BACK",
                new Vector2(640, 620), StaticStringColor, 0.55f);
            //DrawTools.DrawCenteredShadowString(spriteBatch, StaticStringFont, "PROJECT MAGMA - MAY 19 RELEASE",
            //    new Vector2(640, 115), StaticStringColor, 0.7f);
        }

        public SineFloat StaticStringStrength { get; set; }
        public Color StaticStringColor { get { return new Color(Vector3.One * StaticStringStrength.Value); } }

        public void OpenMenuScreen(MenuScreen screen, bool playsound)
        {
            if (playsound)
                { Game.Instance.AudioPlayer.Play(Menu.OkSound, 1f); }
            screen.OnOpen();
            screen.DrawOffset.TargetValue = 0f;
            screen.DrawOffset.Value = 0f;
            screens.AddLast(screen);
            activeScreen = screen;
        }

        public void CloseActiveMenuScreen(bool playsound)
        {
            if (playsound)
                { Game.Instance.AudioPlayer.Play(Menu.BackSound, 0.8f); }
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
            Hidden = false;
            OpenMenuScreen(mainMenu, true);
        }

        public void OpenReleaseNotes()
        {
            active = true;
            OpenMenuScreen(releaseNotesMenu, false);
        }

        public void Close()
        {
            // only allow closing of menu when players are in play
            if (Game.Instance.Simulation.PlayerManager.Count > 0)
            {
                Game.Instance.AudioPlayer.Play(Menu.BackSound);

                active = false;
                screens.Clear();

                // don't resume simulation yet, but only after a-button is released
                waitForButtonRelease = true;
            }
        }

        public LevelMenu LevelMenu;
        public SettingsMenu SettingsMenu;
        public HelpMenu HelpMenu;
        public CreditsMenuPage1 CreditsMenuPage1;
        public CreditsMenuPage2 CreditsMenuPage2;
        public PlayerMenu PlayerMenu;

        private bool active = false;

        public bool Active
        {
            get { return active; }
        }

        private bool Hidden { get; set; }

        public SpriteFont StaticStringFont { get; set; }
        public SpriteFont StaticStringFontSmall { get; set; }

        private MenuScreen activeScreen = null;
        private readonly LinkedList<MenuScreen> screens = new LinkedList<MenuScreen>();

        private MainMenu mainMenu;
        private ReleaseNotesMenu releaseNotesMenu;

        private SpriteBatch spriteBatch;

        private Texture2D background;

        public static readonly string OkSound = "Sounds/menu/ok_Car_hit";
        public static readonly string BackSound = "Sounds/menu/back_Vibrate_hit";
        public static readonly string ChangeSound = "Sounds/menu/change_SONAR_S";
    }
}