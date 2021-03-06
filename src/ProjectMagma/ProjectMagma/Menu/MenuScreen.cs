using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectMagma.MathHelpers;

namespace ProjectMagma
{
    public abstract class MenuScreen
    {
        protected readonly SpriteFont font;

        protected readonly Menu menu;

        readonly Vector2 position;

        public MenuScreen(Menu menu, Vector2 position)
        {
            this.font = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/menu_item");
            this.menu = menu;
            this.position = position;

            DrawPrevious = true;
            DrawOffset = new EaseFloat(0.0f, 0.1f);
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public virtual void OnOpen()
        {
        }

        public virtual void OnClose()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            if (at > menu.elementSelectedAt + Menu.StickRepeatTimeout)
            {
                if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) > Menu.StickDirectionSelectionMin
                    || gamePadState.DPad.Up == ButtonState.Pressed
                    || keyboardState.IsKeyDown(Keys.Up))
                {
                    NavigationUp();
                    menu.elementSelectedAt = at;
                }
                else
                    if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) < -Menu.StickDirectionSelectionMin
                        || gamePadState.DPad.Down == ButtonState.Pressed
                        || keyboardState.IsKeyDown(Keys.Down))
                    {
                        NavigationDown();
                        menu.elementSelectedAt = at;
                    }
            }
        }

        protected virtual void NavigationUp()
        {
        }

        protected virtual void NavigationDown()
        {
        }

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);


        public EaseFloat DrawOffset { get; set; }
        public bool DrawPrevious { get; set; }
    }
}