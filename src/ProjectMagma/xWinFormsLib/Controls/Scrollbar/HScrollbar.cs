/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace xWinFormsLib
{
    class HScrollbar : Control
    {
        Button btLeft, btRight;

        Texture2D background;
        Rectangle backArea;

        int value = 0;
        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (this.value < 0)
                    this.value = 0;
                else if (this.value > max)
                    this.value = max;
            }
        }
        int max = 0;
        public int Max
        {
            get { return max; }
            set
            {
                max = value;
                if (max < 0)
                    max = 0;
                if (this.value > max)
                    this.value = max;
            }
        }

        int step = 1;
        public int Step { get { return step; } set { step = value; } }

        bool isScrolling = false;
        bool inverted = false;
        public bool Inverted { get { return inverted; } set { inverted = value; } }

        Texture2D cursorTex;
        Rectangle cursorArea, cursorLeft, cursorRight, cursorMiddle, cursorMidDest;
        Vector2 cursorPos, cursorOffset;

        public EventHandler OnChangeValue = null;

        new public float Width
        {
            get { return base.Width; }
            set { base.Width = value; if (!IsDisposed) Redraw(); }
        }

        public HScrollbar(Vector2 position, float width)
        {
            this.Position = position;
            this.Width = width;
            this.Height = 12;
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            btLeft = new Button("btLeft", Position, @"content\textures\controls\scrollbar\hscrollbar_button.png", 1f, Color.White);
            btLeft.Owner = this.Owner;
            btLeft.OnPress = btUp_OnPress;
            btLeft.Initialize(FormCollection.ContentManager, FormCollection.Graphics.GraphicsDevice);

            background = Texture2D.FromFile(graphics, @"content\textures\controls\scrollbar\hscrollbar_back.png");

            cursorTex = Texture2D.FromFile(graphics, @"content\textures\controls\scrollbar\hscrollbar_cursor.png");
            cursorLeft = new Rectangle(0, 0, 3, cursorTex.Height);
            cursorMiddle = new Rectangle(3, 0, 1, cursorTex.Height);
            cursorRight = new Rectangle(cursorTex.Width - 3, 0, 3, cursorTex.Height);
            cursorMidDest = new Rectangle(0, 0, 1, cursorTex.Height);

            Redraw();

            base.Initialize(content, graphics);
        }

        private void Redraw()
        {
            backArea.X = (int)Position.X + 12;
            backArea.Y = (int)Position.Y;
            backArea.Width = (int)Width - 24;
            backArea.Height = 12;

            btRight = new Button("btRight", new Vector2(Position.X + Width - 12f, Position.Y), @"content\textures\controls\scrollbar\hscrollbar_button.png", 1f, Color.White);
            btRight.Effect = SpriteEffects.FlipHorizontally;
            btRight.Owner = this.Owner;
            btRight.OnPress = btDown_OnPress;
            btRight.Initialize(FormCollection.ContentManager, FormCollection.Graphics.GraphicsDevice);
        }

        private void btUp_OnPress(object obj, EventArgs e)
        {
            if (!inverted)
            {
                if (Value > 0)
                {
                    Value -= step;
                    if (OnChangeValue != null)
                        OnChangeValue(Value, null);
                }
            }
            else
            {
                if (Value < max)
                {
                    Value += step;
                    if (OnChangeValue != null)
                        OnChangeValue(Value, null);
                }
            }
        }
        private void btDown_OnPress(object obj, EventArgs e)
        {
            if (!inverted)
            {
                if (Value < max)
                {
                    Value += step;
                    if (OnChangeValue != null)
                        OnChangeValue(Value, null);
                }
            }
            else
            {
                if (Value > 0)
                {
                    Value -= step;
                    if (OnChangeValue != null)
                        OnChangeValue(Value, null);
                }
            }
        }

        public override void Dispose()
        {
            cursorTex.Dispose();
            background.Dispose();
            btLeft.Dispose();
            btRight.Dispose();

            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Owner.area.Contains(cursorArea) && cursorArea.Contains(MouseHelper.Cursor.Location))
            {
                if (MouseHelper.HasBeenPressed)
                {
                    isScrolling = true;
                    cursorOffset = MouseHelper.Cursor.Position - new Vector2(cursorArea.X, cursorArea.Y);
                }
            }

            if (isScrolling)
                if (MouseHelper.IsPressed)
                    UpdateScrolling();
                else if (MouseHelper.IsReleased)
                    isScrolling = false;

            if (max > 0)
            {
                btLeft.Update(gameTime);
                btRight.Update(gameTime);
            }
        }

        private void UpdateScrolling()
        {
            cursorPos.X = MouseHelper.Cursor.Position.X - cursorOffset.X - Owner.Position.X;

            if (cursorPos.X < Position.X + 12)
                cursorPos.X = Position.X + 12;
            else if (cursorPos.X > Position.X + Width - cursorArea.Width - 9)
                cursorPos.X = Position.X + Width - cursorArea.Width - 9;

            float x = cursorPos.X - backArea.X;

            int value = 0;

            if (!inverted)
                value = (int)System.Math.Round(x / (backArea.Width - cursorArea.Width) * max);
            else
                value = max - (int)System.Math.Round(x / (backArea.Width - cursorArea.Width) * max);

            if (value < 0)
                value = 0;
            else if (value > max)
                value = max;

            if (this.value != value)
            {
                this.value = value;
                if (OnChangeValue != null)
                    OnChangeValue(value, null);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            if (max > 0)
                DrawCursor(spriteBatch);
            btLeft.Draw(spriteBatch);
            btRight.Draw(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {            
            spriteBatch.Draw(background, backArea, Color.White);
        }

        private void DrawCursor(SpriteBatch spriteBatch)
        {
            cursorArea.Width = System.Math.Max(20, backArea.Width - System.Math.Max(20, max / 4));
            cursorArea.Height = 12;

            cursorPos.Y = Position.Y;
            if (!isScrolling)
            {
                if (!inverted)
                    cursorPos.X = backArea.X + (Width - 21 - cursorArea.Width) * ((float)value / (float)max);
                else
                    cursorPos.X = backArea.X + (Width - 21 - cursorArea.Width) * ((float)(max - value) / (float)max);
            }

            cursorArea.X = (int)(cursorPos.X + Owner.Position.X);
            cursorArea.Y = (int)(cursorPos.Y + Owner.Position.Y);

            spriteBatch.Draw(cursorTex, cursorPos, cursorLeft, BackColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            cursorMidDest.X = (int)cursorPos.X + 3;
            cursorMidDest.Y = (int)cursorPos.Y;
            cursorMidDest.Width = cursorArea.Width - 6;
            spriteBatch.Draw(cursorTex, cursorMidDest, cursorMiddle, BackColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            spriteBatch.Draw(cursorTex, cursorPos + new Vector2(cursorMidDest.Width, 0f), cursorRight, BackColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
