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
            SelectedItem.Deactivate();

            selected++;
            if (selected == MenuItems.Length)
                selected = 0;

            SelectedItem.Activate();
        }

        public void SelectPrevious()
        {
            SelectedItem.Deactivate();

            selected--;
            if (selected < 0)
                selected = MenuItems.Length - 1;

            SelectedItem.Activate();
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

        private readonly Vector2 shadowOffset = new Vector2(2, 4);

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 pos = Position;
            for (int i = MenuItems.Length - 1; i >= 0; i--)
            {
                MenuItem item = MenuItems[i];

                spriteBatch.DrawString(font, item.Text, pos + shadowOffset, Color.Red);
                spriteBatch.DrawString(font, item.Text, pos + shadowOffset / 2, Color.Orange);
                spriteBatch.DrawString(font, item.Text, pos, Color.Yellow);
                if (i == selected)
                    spriteBatch.DrawString(font, item.Text, pos, Color.White);
                
                DrawWithItem(gameTime, spriteBatch, item, pos, 0.5f);

                Vector2 box = font.MeasureString(item.Name);
                pos.Y -= box.Y;
            }
        }

        public virtual void DrawWithItem(GameTime gameTime, SpriteBatch spriteBatch, MenuItem item, Vector2 pos, float scale)
        {
        }

    }
}