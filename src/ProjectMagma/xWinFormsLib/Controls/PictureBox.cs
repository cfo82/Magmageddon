/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace xWinFormsLib
{
    public class PictureBox : Control
    {
        Texture2D texture;
        string textureName;
        Rectangle srcRect;
        Rectangle destRect;

        Texture2D pixel;
        int border;
        Rectangle borderRect;

        bool bMouseOver = false;

        public PictureBox(string name, Vector2 position, string texture, int border)
            : base(name, position)
        {
            this.textureName = texture;
            this.border = border;
        }
        public PictureBox(string name, Vector2 position, string texture, int width, int height, int border)
            : base(name, position)
        {
            this.textureName = texture;
            this.Width = width;
            this.Height = height;
            this.border = border;
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            texture = Texture2D.FromFile(graphics, textureName);
            srcRect = new Rectangle(0, 0, texture.Width, texture.Height);

            if (Size == Vector2.Zero)
                Size = new Vector2(texture.Width, texture.Height);

            destRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);

            area.Width = destRect.Width;
            area.Height = destRect.Height;

            if (border > 0)
            {
                pixel = new Texture2D(graphics, 1, 1);
                pixel.SetData<Color>(new Color[] { Color.White });
                borderRect = new Rectangle(destRect.X - border, destRect.Y - border, destRect.Width + border * 2, destRect.Height + border * 2);
            }

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            texture.Dispose();

            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateEvents();
        }

        private void UpdateEvents()
        {
            if (area.Contains(MouseHelper.Cursor.Location) && Owner.area.Contains(area))
            {
                if (!bMouseOver)
                {
                    bMouseOver = true;
                    if (OnMouseOver != null)
                        OnMouseOver(this, null);
                }

                if (MouseHelper.HasBeenPressed)
                {
                    if (OnPress != null)
                        OnPress(this, null);
                }
                else if (MouseHelper.HasBeenReleased)
                {
                    if (OnRelease != null)
                        OnRelease(this, null);
                }
            }
            else if (bMouseOver)
            {
                bMouseOver = false;
                if (OnMouseOut != null)
                    OnMouseOut(this, null);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (border > 0)
                spriteBatch.Draw(pixel, borderRect, Color.Black);

            spriteBatch.Draw(texture, destRect, srcRect, BackColor);
        }
    }
}
