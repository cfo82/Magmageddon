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
    class VScrollbar: Control
    {
        Button btUp, btDown;

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
        Rectangle cursorArea, cursorTop, cursorBottom, cursorMiddle, cursorMidDest;
        Vector2 cursorPos, cursorOffset;

        public EventHandler OnChangeValue = null;

        new public float Height
        {
            get { return base.Height; }
            set { base.Height = value; if (!IsDisposed) Redraw(); }
        }

        public VScrollbar(Vector2 position, float height)
        {
            this.Position = position;
            this.Height = height;            
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            background = Texture2D.FromFile(graphics, @"content\textures\controls\scrollbar\vscrollbar_back.png");

            Size = new Vector2(12, Height);

            cursorTex = Texture2D.FromFile(graphics, @"content\textures\controls\scrollbar\vscrollbar_cursor.png");
            cursorTop = new Rectangle(0, 0, cursorTex.Width, 3);
            cursorMiddle = new Rectangle(0, 3, cursorTex.Width, 1);
            cursorBottom = new Rectangle(0, cursorTex.Height - 3, cursorTex.Width, 3);
            cursorMidDest = new Rectangle(0, 0, cursorTex.Width, 1);

            Redraw();

            base.Initialize(content, graphics);
        }

        private void Redraw()
        {            
            btUp = new Button("btUp", Position, @"content\textures\controls\scrollbar\vscrollbar_button.png", 1f, Color.White);
            btDown = new Button("btDown", new Vector2(Position.X, Position.Y + Height - 12f), @"content\textures\controls\scrollbar\vscrollbar_button.png", 1f, Color.White);
            btDown.Effect = SpriteEffects.FlipVertically;

            btUp.Owner = this.Owner;
            btDown.Owner = this.Owner;

            btUp.OnPress = btUp_OnPress;
            btDown.OnPress = btDown_OnPress;

            btUp.Initialize(FormCollection.ContentManager, FormCollection.Graphics.GraphicsDevice);
            btDown.Initialize(FormCollection.ContentManager, FormCollection.Graphics.GraphicsDevice);
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
            btUp.Dispose();
            btDown.Dispose();

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
                btUp.Update(gameTime);
                btDown.Update(gameTime);
            }
        }

        private void UpdateScrolling()
        {
            cursorPos.Y = MouseHelper.Cursor.Position.Y - cursorOffset.Y - Owner.Position.Y;

            if (cursorPos.Y < Position.Y + 12)
                cursorPos.Y = Position.Y + 12;
            else if (cursorPos.Y > Position.Y + Height - cursorArea.Height - 9)
                cursorPos.Y = Position.Y + Height - cursorArea.Height - 9;

            float y = cursorPos.Y - backArea.Y;
            int value = 0;

            if (!inverted)
                value = (int)System.Math.Round(y / (backArea.Height - cursorArea.Height) * max);
            else
                value = max - (int)System.Math.Round(y / (backArea.Height - cursorArea.Height) * max);

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
            DrawCursor(spriteBatch);
            btUp.Draw(spriteBatch);
            btDown.Draw(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            backArea.X = (int)Position.X;
            backArea.Y = (int)Position.Y + 12;
            backArea.Width = 12;
            backArea.Height = (int)Height - 24;
            spriteBatch.Draw(background, backArea, Color.White);
        }

        private void DrawCursor(SpriteBatch spriteBatch)
        {
            cursorArea.Height = System.Math.Max(20, backArea.Height - System.Math.Max(20, max / 4));
            cursorArea.Width = 12;

            cursorPos.X = Position.X;
            if (!isScrolling)
            {
                if (!inverted)
                    cursorPos.Y = backArea.Y + (Height - 21 - cursorArea.Height) * ((float)value / (float)max);
                else
                    cursorPos.Y = backArea.Y + (Height - 21 - cursorArea.Height) * ((float)(max - value) / (float)max);
            }

            cursorArea.X = (int)(cursorPos.X + Owner.Position.X);
            cursorArea.Y = (int)(cursorPos.Y + Owner.Position.Y);

            spriteBatch.Draw(cursorTex, cursorPos, cursorTop, BackColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);            
            
            cursorMidDest.X = (int)cursorPos.X;
            cursorMidDest.Y = (int)cursorPos.Y + 3;
            cursorMidDest.Height = cursorArea.Height - 6;
            spriteBatch.Draw(cursorTex, cursorMidDest, cursorMiddle, BackColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            spriteBatch.Draw(cursorTex, cursorPos + new Vector2(0f, cursorMidDest.Height), cursorBottom, BackColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
