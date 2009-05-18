using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectMagma.MathHelpers;
using System;

namespace ProjectMagma
{
    abstract class ItemizedMenuScreen : MenuScreen
    {
        int width;
        protected int selected = 0;
        SineFloat selectedSize, unselectedSize;

        public ItemizedMenuScreen(Menu menu) :
            base(menu, new Vector2(640, 360))
        {
            //this.width = width;
            selectedSize = new SineFloat(0.7f, 0.9f, 10.0f);
            unselectedSize = new SineFloat(0.48f, 0.52f, 3.0f);
            selectedSize.Start(Game.Instance.GlobalClock.ContinuousMilliseconds);
            unselectedSize.Start(Game.Instance.GlobalClock.ContinuousMilliseconds);
        }

        public void RecomputeWidth()
        {
            float width = 0;
            foreach(MenuItem item in MenuItems)
            {
                width = (int) Math.Max(width, font.MeasureString(item.Text).X);
            }
            width *= 0.8f;

            this.width = (int) width;
        }

        public override void OnOpen()
        {
            if(ResetSelectionOnOpen)
                selected = 0;
            SelectedItem.Activate();
        }

        protected bool ResetSelectionOnOpen
        {
            get { return true; }
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
            selectedSize.Start(Game.Instance.GlobalClock.ContinuousMilliseconds);
        }

        public void SelectPrevious()
        {
            SelectedItem.Deactivate();

            selected--;
            if (selected < 0)
                selected = MenuItems.Length - 1;

            SelectedItem.Activate();
            selectedSize.Start(Game.Instance.GlobalClock.ContinuousMilliseconds);
        }

        public abstract MenuItem[] MenuItems
        {
            get;
        }

        public override void Update(GameTime gameTime)
        {
            selectedSize.Update(gameTime.TotalGameTime.TotalMilliseconds);
            unselectedSize.Update(gameTime.TotalGameTime.TotalMilliseconds);

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
            // this needs to be updated even if paused
            DrawOffset.Update(gameTime.ElapsedRealTime.TotalMilliseconds);

            // this is to compensate for the size overhead of the currently selected item
            Vector2 globalOffset = new Vector2(0, (selectedSize.Value-1)/2 + 0.1f);
            float itemHeight = 50f;

            // draw the individual items
            Vector2 pos = new Vector2(640 + DrawOffset.Value, 360 + MenuItems.Length*itemHeight/2);
            for (int i = MenuItems.Length - 1; i >= 0; i--)
            {
                MenuItem item = MenuItems[i];
                
                // compute how much we need to shift the item to compensate for the selected item
                Vector2 offset = Vector2.Zero;
                if(i>selected) offset += globalOffset*itemHeight;
                if (i<selected) offset -= globalOffset*itemHeight;

                // draw the text and its attachments
                DrawEffectString(spriteBatch, item.Text, pos + offset, i == selected);
                DrawWithItem(gameTime, spriteBatch, item, pos + offset - Vector2.UnitY*itemHeight/2, 0.5f);

                // go on
                pos.Y -= itemHeight;
            }
        }
        
        public bool Active { get; set; }
        private Vector2 lastBox;

        private static readonly int contourOffset = 2;
        private static readonly Color shadowColor = new Color(70, 70, 70);
        private void DrawEffectString(SpriteBatch spriteBatch, string str, Vector2 pos, bool isSelected)
        {
            float scale = isSelected ? selectedSize.Value : unselectedSize.Value;

            // string should be centered
            lastBox = font.MeasureString(str) * scale;
            pos -= lastBox * 0.5f;

            // draw string countours
            DrawString(spriteBatch, str, pos + new Vector2(+contourOffset, +contourOffset), shadowColor, scale);
            DrawString(spriteBatch, str, pos + new Vector2(-contourOffset, +contourOffset), shadowColor, scale);
            DrawString(spriteBatch, str, pos + new Vector2(+contourOffset, -contourOffset), shadowColor, scale);
            DrawString(spriteBatch, str, pos + new Vector2(-contourOffset, -contourOffset), shadowColor, scale);
            DrawString(spriteBatch, str, pos, isSelected && Active ? Color.White : Color.LightGray, scale);

        }

        private void DrawString(SpriteBatch spriteBatch, string str, Vector2 pos, Color color, float scale)
        {
            spriteBatch.DrawString(font, str, pos, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1.0f);
        }

        public virtual void DrawWithItem(GameTime gameTime, SpriteBatch spriteBatch, MenuItem item, Vector2 pos, float scale)
        {
        }

    }
}