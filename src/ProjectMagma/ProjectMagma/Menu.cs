using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma
{
    class Menu
    {
        public const float ButtonRepeatTimeout = 200;
        public const float StickRepeatTimeout = 250;
        public const float StickDirectionSelectionMin = 0.6f;

        private static Menu instance = new Menu();
        
        public double buttonPressedAt = 0;
        public double elementSelectedAt = 0;

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

            background = Game.Instance.Content.Load<Texture2D>("Sprites/Menu/background");

            mainMenu = new MainMenu(this);
        }

        internal void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if(active)
            {
                if (gamePadState.Buttons.Back == ButtonState.Pressed)
                {
                    Close();
                }
                else
                {
                    activeScreen.Update(gameTime);
                }
            }
            else
                if (gamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    Open();
                    buttonPressedAt = at;
                }
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

                foreach (MenuScreen screen in screens)
                {
                    screen.Draw(gameTime, spriteBatch);
                }

                spriteBatch.End();
            }
        }

        public void OpenMenuScreen(MenuScreen screen)
        {
            screens.AddLast(screen);
            activeScreen = screen;
        }

        public void CloseActiveMenuScreen()
        {
            if(activeScreen == mainMenu)
            {
                Close();
            }
            else
            {
                screens.RemoveLast();
                activeScreen = screens.Last.Value;
            }
        }

        public void Open()
        {
            active = true;
            OpenMenuScreen(mainMenu);
        }

        public void Close()
        {
            active = false;
            screens.Clear();
        }

        private bool active = false;

        public bool Active
        {
            get { return active; }
        }


        private MenuScreen activeScreen = null;
        private readonly LinkedList<MenuScreen> screens = new LinkedList<MenuScreen>(); 

        private MainMenu mainMenu;

        private SpriteBatch spriteBatch;
        
        private Texture2D background;
    }

    class MenuItem
    {
        private readonly String name;
        private readonly Texture2D sprite;
        private readonly Texture2D selectedSprite;
        private event ItemSelectionHandler itemSelected;

        public MenuItem(String name, String sprite, ItemSelectionHandler itemSelected)
        {
            this.name = name;
            this.sprite = Game.Instance.Content.Load<Texture2D>("Sprites/Menu/" + sprite);
            this.selectedSprite = Game.Instance.Content.Load<Texture2D>("Sprites/Menu/" + sprite + "_selected");
            this.itemSelected = itemSelected;
        }

        public String Name
        {
            get { return name; }
        }

        public Texture2D Sprite
        {
            get { return sprite; }
        }

        public Texture2D SelectedSprite
        {
            get { return selectedSprite; }
        }

        public void PerformAction()
        {
            itemSelected(this);
        }
    }

    abstract class MenuScreen
    {
        protected readonly Menu menu;

        readonly Vector2 position;

        public MenuScreen(Menu menu, Vector2 position)
        {
            this.menu = menu;
            this.position = position;
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }

    abstract class ItemizedMenuScreen: MenuScreen
    {
        readonly int width;

        protected int selected = 0;

        public ItemizedMenuScreen(Menu menu, Vector2 position, int width):
            base(menu, position)
        {
            this.width = width;
        }
  
        public int Width
        {
            get { return width; }
        }

        public int Selected
        {
            get { return selected; }
        }

        public MenuItem SelectedItem
        {
            get { return MenuItems[selected]; }
        }

        public void SelectNext()
        {
            selected++;
            if(selected == MenuItems.Length)
                selected = MenuItems.Length-1;
        }

        public void SelectPrevious()
        {
            selected--;
            if (selected < 0)
                selected = 0;
        }

        public abstract MenuItem[] MenuItems
        {
            get;
        }

        public override void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (at > menu.buttonPressedAt + Menu.ButtonRepeatTimeout)
            {
                if (gamePadState.Buttons.Start == ButtonState.Pressed
                    || gamePadState.Buttons.A == ButtonState.Pressed)
                {
                    SelectedItem.PerformAction();
                    menu.buttonPressedAt = at;
                }
                else
                if (gamePadState.Buttons.X == ButtonState.Pressed)
                {
                    menu.CloseActiveMenuScreen();
                    menu.buttonPressedAt = at;
                }
            }

            if (at > menu.elementSelectedAt + Menu.StickRepeatTimeout)
            {
                if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) > Menu.StickDirectionSelectionMin)
                {
                    SelectPrevious();
                    menu.elementSelectedAt = at;
                }
                else
                    if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) < -Menu.StickDirectionSelectionMin)
                    {
                        SelectNext();
                        menu.elementSelectedAt = at;
                    }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 pos = Position;
            for (int i = MenuItems.Length - 1; i >= 0; i--)
            {
                MenuItem item = MenuItems[i];

                Texture2D sprite;
                if (i == Selected)
                    sprite = item.SelectedSprite;
                else
                    sprite = item.Sprite;

                float scale = (float)Width / (float)sprite.Width;
                pos.Y -= sprite.Height * scale;

                spriteBatch.Draw(sprite, pos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

                DrawWithItem(gameTime, spriteBatch, item, pos, scale);
            }
        }

        public virtual void DrawWithItem(GameTime gameTime, SpriteBatch spriteBatch, MenuItem item, Vector2 pos, float scale)
        {
        }

    }

    class MainMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;
        readonly MenuScreen levelMenu;
        readonly MenuScreen settingsMenu;

        public MainMenu(Menu menu): base(menu, new Vector2(30, 650), 200)
        {
            this.menuItems = new MenuItem[] { 
                new MenuItem("new_game", "new_game", new ItemSelectionHandler(NewGameHandler)),
                new MenuItem("settings", "settings", new ItemSelectionHandler(SettingsHandler)),
                new MenuItem("exit_game", "exit_game", new ItemSelectionHandler(ExitGameHandler))
            };

            levelMenu = new LevelMenu(menu);
            settingsMenu = new SettingsMenu(menu);
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }
       
        private void NewGameHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(levelMenu);
        }

        private void SettingsHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(settingsMenu);
        }

        private void ExitGameHandler(MenuItem sender)
        {
            Game.Instance.Exit();
        }
    }

    class LevelMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;

        public LevelMenu(Menu menu)
            : base(menu, new Vector2(280, 600), 200)
        {
            List<LevelInfo> levels = Game.Instance.Levels;
            this.menuItems = new MenuItem[levels.Count];
            for (int i = 0; i < menuItems.Length; i++)
            {
                menuItems[i] = new MenuItem(levels[i].Name, levels[i].FileName,
                    new ItemSelectionHandler(LevelSelected));
            }
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void LevelSelected(MenuItem sender)
        {

        }
    }

    class SettingsMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;
        private readonly Texture2D volumeIndicator;
        private readonly Texture2D volumeIndicatorSelected;

        public SettingsMenu(Menu menu)
            : base(menu, new Vector2(280, 600), 250)
        {
           this.menuItems = new MenuItem[] { 
                new MenuItem("sound_volume", "sound_volume", new ItemSelectionHandler(DoNothing)),
                new MenuItem("music_volume", "music_volume", new ItemSelectionHandler(DoNothing))
            };

           volumeIndicator = Game.Instance.Content.Load<Texture2D>("Sprites/Menu/volume_indicator");
           volumeIndicatorSelected = Game.Instance.Content.Load<Texture2D>("Sprites/Menu/volume_indicator_selected");
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void DoNothing(MenuItem sender)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000f;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitX) > Menu.StickDirectionSelectionMin)
            {
                if (SelectedItem.Name == "music_volume")
                    Game.Instance.MusicVolume += dt;
                else
                if (SelectedItem.Name == "sound_volume")
                    Game.Instance.EffectsVolume += dt;
            }
            else
                if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitX) < -Menu.StickDirectionSelectionMin)
                {
                    if (SelectedItem.Name == "music_volume")
                        Game.Instance.MusicVolume -= dt;
                    else
                        if (SelectedItem.Name == "sound_volume")
                            Game.Instance.EffectsVolume -= dt;
                }

            if (Game.Instance.MusicVolume > 1)
                Game.Instance.MusicVolume = 1;
            if(Game.Instance.MusicVolume < 0)
                Game.Instance.MusicVolume = 0;
            if (Game.Instance.EffectsVolume > 1)
                Game.Instance.EffectsVolume = 1;
            if (Game.Instance.EffectsVolume < 0)
                Game.Instance.EffectsVolume = 0;
        }

        public override void DrawWithItem(GameTime gameTime, SpriteBatch spriteBatch, MenuItem item, Vector2 pos, float scale)
        {
            if (item.Name == "music_volume")
            {
                pos.X += 550 * scale;
                Vector2 nuScale = new Vector2(scale * Game.Instance.MusicVolume, scale);
                if(SelectedItem == item)
                    spriteBatch.Draw(volumeIndicatorSelected, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
                else
                    spriteBatch.Draw(volumeIndicator, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
            }
            else
                if (item.Name == "sound_volume")
                {
                    pos.X += 550 * scale;
                    Vector2 nuScale = new Vector2(scale * Game.Instance.EffectsVolume, scale);
                    if (SelectedItem == item)
                        spriteBatch.Draw(volumeIndicatorSelected, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
                    else
                        spriteBatch.Draw(volumeIndicator, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
                }
        }
    }

    /*
    class PlayerMenu : MenuScreen
    {
        public PlayerMenu(Menu menu)
            : base(menu, new Vector2(430, 600))
        {
        }


        public abstract void Update(GameTime gameTime)
        {
        }

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }
    }
     */

    delegate void ItemSelectionHandler(MenuItem sender);
 
}
