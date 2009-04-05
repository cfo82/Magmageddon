using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    class Menu
    {
        private const float ButtonRepeatTimeout = 500;

        private static Menu instance = new Menu();
        private double buttonPressedAt = 0;
        private double elementSelectedAt = 0;

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

            mainMenu = new MainMenu();
        }

        internal void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if(active)
            {
                if (gamePadState.Buttons.Back == ButtonState.Pressed)
                {
                    active = false;
                }
                else
                {
                    if ((gamePadState.Buttons.Start == ButtonState.Pressed
                        || gamePadState.Buttons.A == ButtonState.Pressed)
                        && at > buttonPressedAt + ButtonRepeatTimeout)
                    {
                        activeScreen.SelectedItem.PerformAction();
                        buttonPressedAt = at;
                        return;
                    }

                    if (at > elementSelectedAt + ButtonRepeatTimeout)
                    {
                        if (gamePadState.ThumbSticks.Left.Y > 0)
                        {
                            activeScreen.SelectNext();
                            elementSelectedAt = at;
                        }
                        else
                            if (gamePadState.ThumbSticks.Left.Y < 0)
                            {
                                activeScreen.SelectPrevious();
                                elementSelectedAt = at;
                            }
                    }
                }
            }
            else
                if (gamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    active = true;
                    buttonPressedAt = at;
                    activeScreen = mainMenu;
                    screens.Clear();
                    screens.Add(mainMenu);
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
                    Vector2 pos = screen.Position;
                    for (int i = screen.MenuItems.Length - 1; i >= 0; i--)
                    {
                        Texture2D sprite;
                        if (i == screen.Selected)
                            sprite = screen.MenuItems[i].SelectedSprite;
                        else
                            sprite = screen.MenuItems[i].Sprite;
                        float scale = (float)screen.Width / (float)sprite.Width;
                        pos.Y -= sprite.Height * scale;
                        spriteBatch.Draw(sprite, pos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                }

                spriteBatch.End();
            }
        }

        private bool active = false;

        public bool Active
        {
            get { return active; }
        }


        private MenuScreen activeScreen = null;
        private readonly List<MenuScreen> screens = new List<MenuScreen>(); 

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
        readonly Vector2 position;
        readonly int width;

        protected int selected = 0;

        public MenuScreen(Vector2 position, int width)
        {
            this.position = position;
            this.width = width;
        }

        public Vector2 Position
        {
            get { return position; }
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
                selected = 0;
        }

        public void SelectPrevious()
        {
            selected--;
            if (selected < 0)
                selected = MenuItems.Length-1;
        }

        public abstract MenuItem[] MenuItems
        {
            get;
        }
    }

    class MainMenu : MenuScreen
    {
        readonly MenuItem[] menuItems;

        public MainMenu(): base(new Vector2(30, 650), 200)
        {
            this.menuItems = new MenuItem[] { 
                new MenuItem("new_game", "new_game", new ItemSelectionHandler(NewGameHandler)),
                new MenuItem("exit_game", "exit_game", new ItemSelectionHandler(ExitGameHandler))
            };
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }
       
        private void NewGameHandler(MenuItem sender)
        {

        }

        private void ExitGameHandler(MenuItem sender)
        {
            Game.Instance.Exit();
        }
    }

    delegate void ItemSelectionHandler(MenuItem sender);
 
}
