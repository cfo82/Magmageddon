using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    abstract class ItemizedMenuScreen : MenuScreen
    {
        readonly int width;

        protected int selected = 0;

        public ItemizedMenuScreen(Menu menu, Vector2 position, int width) :
            base(menu, position)
        {
            this.width = width;
        }

        public override void OnOpen()
        {
            selected = 0;
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

        protected override void NavigationUp()
        {
            SelectPrevious();
        }

        protected override void NavigationDown()
        {
            SelectNext();
        }

        public void SelectNext()
        {
            selected++;
            if (selected == MenuItems.Length)
                selected = MenuItems.Length - 1;
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
            KeyboardState keyboardState = Keyboard.GetState();

            if (at > menu.buttonPressedAt + Menu.ButtonRepeatTimeout)
            {
                if ((gamePadState.Buttons.A == ButtonState.Pressed
                    && menu.lastGPState.Buttons.A == ButtonState.Released)
                    || (keyboardState.IsKeyDown(Keys.Enter)
                    && menu.lastKBState.IsKeyUp(Keys.Enter)))
                {
                    SelectedItem.PerformAction();
                    menu.buttonPressedAt = at;
                }
            }

            base.Update(gameTime);
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
}