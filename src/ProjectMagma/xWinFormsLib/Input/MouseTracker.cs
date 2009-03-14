/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace xWinFormsLib
{
    class MouseTracker
    {
        SpriteBatch spriteBatch;

        Texture2D texture;
        Rectangle rect;
        Vector2 center;
        
        float scale = 1.25f;
        Color originalColor = new Color(0, 0, 0, 110);
        Color color = new Color(0, 0, 0, 110);

        MouseState ms, ps;
        Vector2 mousePosition;

        public MouseTracker()
        {
            spriteBatch = new SpriteBatch(FormCollection.Graphics.GraphicsDevice);
            texture = Texture2D.FromFile(FormCollection.Graphics.GraphicsDevice, 
                        @"content\textures\cursors\circle.png");
            rect = new Rectangle(0, 0, texture.Width, texture.Height);
            center = new Vector2(rect.Width / 2f, rect.Height / 2f);
        }

        public void Dispose()
        {
            texture.Dispose();
            spriteBatch.Dispose();
        }

        public void Update()
        {
            ps = ms;
            ms = Mouse.GetState();
            mousePosition.X = ms.X;
            mousePosition.Y = ms.Y;

            CheckButtons();

            if (scale > 0.6f)
                scale *= 0.9f;
            else if (scale != 0.6f)
            {
                scale = 0.6f;
                Restore();
            }
        }

        private void CheckButtons()
        {
            if (ms.LeftButton == ButtonState.Pressed && ps.LeftButton == ButtonState.Released)
                Press();
            else if (ms.LeftButton == ButtonState.Released && ps.LeftButton == ButtonState.Pressed)
                Release();

            if (ms.RightButton == ButtonState.Pressed && ps.RightButton == ButtonState.Released)
                Press();
            else if (ms.RightButton == ButtonState.Released && ps.RightButton == ButtonState.Pressed)
                Release();

            if (ms.MiddleButton == ButtonState.Pressed && ps.MiddleButton == ButtonState.Released)
                Press();
            else if (ms.MiddleButton == ButtonState.Released && ps.MiddleButton == ButtonState.Pressed)
                Release();

            if (ms.XButton1 == ButtonState.Pressed && ps.XButton1 == ButtonState.Released)
                Press();
            else if (ms.XButton1 == ButtonState.Released && ps.XButton1 == ButtonState.Pressed)
                Release();

            if (ms.XButton2 == ButtonState.Pressed && ps.XButton2 == ButtonState.Released)
                Press();
            else if (ms.XButton2 == ButtonState.Released && ps.XButton2 == ButtonState.Pressed)
                Release();
        }

        private void Press()
        {
            scale = 1.25f;
            color = Color.Red;
        }

        private void Release()
        {
            scale = 1.25f;
            color = Color.Blue;
        }

        private void Restore()
        {
            if (color != originalColor)
                color = originalColor;
        }

        public void Draw()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(texture, mousePosition, rect, color, 0f, center, scale, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
    }
}
