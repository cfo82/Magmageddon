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
    public class Progressbar : Control
    {
        Texture2D border;
        Texture2D progressBar;
        private Rectangle sourceRect = Rectangle.Empty;
        private Rectangle destRect = Rectangle.Empty;

        int numberOfBlocks = 20;
        int blockWidth = 10;

        public bool bContinuous = true;
        public Style style = Style.Continuous;
        public enum Style
        {
            Continuous,
            Blocks
        }

        int value = 0;
        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (this.value < 0)
                    this.value = 0;
                else if (this.value > 100)
                    this.value = 100;
            }
        }

        public Progressbar(string name, Vector2 position, int width, int height, bool bContinuous)
        {
            this.Name = name;
            this.Position = position;
            this.Width = width;
            this.Height = height;
            this.BackColor = Color.White;
            this.ForeColor = Color.SkyBlue;

            this.bContinuous = bContinuous;
            if (this.bContinuous)
                style = Style.Continuous;
            else
                style = Style.Blocks;

            numberOfBlocks = width / blockWidth;
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            border = new Texture2D(graphics, (int)Width, (int)Height, 1, TextureUsage.None, SurfaceFormat.Color);
            progressBar = new Texture2D(graphics, (int)Width - 4, border.Height - 4, 1, TextureUsage.None, SurfaceFormat.Color);

            Height = border.Height;

            area.Width = (int)Width;
            area.Height = (int)Height;

            sourceRect = new Rectangle(0, 0, progressBar.Width, progressBar.Height);
            destRect = new Rectangle((int)Position.X + 2, (int)Position.Y + 2, (int)(progressBar.Width * value / 100f), progressBar.Height);

            Color[] pixel = new Color[border.Width * border.Height];            

            for (int y = 0; y < border.Height; y++)
            {
                for (int x = 0; x < border.Width; x++)
                {
                    if (x == 0 || y == 0 || x == border.Width - 1 || y == border.Height - 1)
                    {
                        pixel[x + y * border.Width] = Color.Black;

                        if ((x == 0 && y == 0) || (x == border.Width - 1 && y == 0) ||
                        (x == 0 && y == border.Height - 1) || (x == border.Width - 1 && y == border.Height - 1))
                            pixel[x + y * border.Width] = Color.TransparentBlack;
                    }
                    else
                        pixel[x + y * border.Width] = new Color(new Vector4(BackColor.ToVector3(), 1f));
                }
            }

            border.SetData<Color>(pixel);

            pixel = new Color[progressBar.Width * progressBar.Height];

            for (int y = 0; y < progressBar.Height; y++)
            {
                for (int x = 0; x < progressBar.Width; x++)
                {
                    bool bInvisiblePixel = false;

                    if (style == Style.Blocks)
                    {
                        int xPos = x % (int)((float)progressBar.Width / (float)numberOfBlocks);
                        if (xPos == 0)
                            bInvisiblePixel = true;
                    }

                    if (!bInvisiblePixel)
                    {
                        float gradient = 1.0f - y * 0.035f;
                        Color pixelColor = new Color(new Vector4(ForeColor.ToVector3() * gradient, 1f));
                        pixel[x + y * progressBar.Width] = pixelColor;
                    }
                }
            }

            progressBar.SetData<Color>(pixel);

            base.Initialize(content, graphics);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (value < 0) value = 0;
            else if (value > 100) value = 100;

            int rectWidth = (int)(progressBar.Width * ((float)value / 100f));

            if (style == Style.Continuous)
            {
                destRect.Width = rectWidth;
                sourceRect.Width = destRect.Width;
            }
            else
            {
                int totalBlocks = (int)System.Math.Round((numberOfBlocks) * ((float)value / 100f));

                destRect.Width = blockWidth * totalBlocks;
                sourceRect.Width = destRect.Width;
            }

            destRect.X = (int)Position.X + 2;
            destRect.Y = (int)Position.Y + 2;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {                        
            spriteBatch.Draw(border, Position, BackColor);
            spriteBatch.Draw(progressBar, destRect, sourceRect, ForeColor);
        }
    }
}
