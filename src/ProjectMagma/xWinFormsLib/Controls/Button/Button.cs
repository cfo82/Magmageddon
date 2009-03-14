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
using Microsoft.Xna.Framework.Input;

namespace xWinFormsLib
{
    public class Button : Control
    {
        Texture2D texture;
        string textureName = @"content\textures\controls\button\default.png";
        Rectangle[] srcRect = new Rectangle[3];
        Rectangle[] destRect = new Rectangle[3];

        Vector2 textPos;
        Vector2 textSize;

        Vector2 origin = Vector2.Zero;
        float scale = 1f;
        SpriteEffects effect = SpriteEffects.None;

        Color highlight, dimColor;      //Need to fix this.. thoughts: 4 new color properties at the control level -> ForeColor_MouseOver/ForeColor_MouseDown/BackColor_MouseOver/BackColor_MouseDown
        Color texthighlight, textdim;

        bool bMouseOver = false;
        bool bMouseDown = false;

        public SpriteEffects Effect { get { return effect; } set { effect = value; } }

        /// <summary>
        /// Button
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="position">Form Position</param>
        /// <param name="text">Button Text</param>
        /// <param name="backColor">Background Color</param>
        /// <param name="foreColor">Foreground Color</param>
        public Button(string name, Vector2 position, string text, Color backColor, Color foreColor)
            : base(name, position)
        {
            this.Text = text;
            this.BackColor = backColor;
            this.ForeColor = foreColor;
        }

        /// <summary>
        /// Button
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="position">Form Position</param>
        /// <param name="width">Button Width</param>
        /// <param name="text">Button Text</param>
        /// <param name="backColor">Background Color</param>
        /// <param name="foreColor">Foreground Color</param>
        public Button(string name, Vector2 position, float width, string text, Color backColor, Color foreColor)
            : base(name, position)
        {
            this.Text = text;
            this.Size = new Vector2(width, 0f);
            this.BackColor = backColor;
            this.ForeColor = foreColor;
        }

        /// <summary>
        /// Button
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="position">Form Position</param>
        /// <param name="size">Button Size</param>
        /// <param name="text">Button Text</param>
        /// <param name="backColor">Background Color</param>
        /// <param name="foreColor">Foreground Color</param>
        public Button(string name, Vector2 position, Vector2 size, string text, Color backColor, Color foreColor)
            : base(name, position)
        {
            this.Text = text;
            this.Size = size;
            this.BackColor = backColor;
            this.ForeColor = foreColor;
        }

        /// <summary>
        /// Button
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="position">Form Position</param>
        /// <param name="texture">Button Texture</param>
        /// <param name="scale">Texture Scale</param>
        /// <param name="backColor">Button Color</param>
        /// <param name="origin">Texture Draw Origin</param>
        public Button(string name, Vector2 position, string texture, float scale, Color backColor)
            : base(name, position)
        {
            this.Position = position;
            this.textureName = texture;
            this.scale = scale;
            this.BackColor = backColor;            
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            texture = Texture2D.FromFile(graphics, textureName);

            srcRect[0] = new Rectangle(0, 0, texture.Width - 1, texture.Height);
            srcRect[1] = new Rectangle(texture.Width - 1, 0, 1, texture.Height);
            srcRect[2] = new Rectangle(texture.Width - 1, 0, -texture.Width + 1, texture.Height);

            if (Text != string.Empty)
                textSize = Font.MeasureString(Text);

            if (textureName == @"content\textures\controls\button\default.png")
            {
                if (Size.X < Font.MeasureString(Text).X + 20f)
                    Size = new Vector2(Font.MeasureString(Text).X + 20f, Size.Y);
                if (Size.Y < texture.Height)
                    Size = new Vector2(Size.X, texture.Height);
                if (Size.Y < Font.MeasureString(Text).Y + 5f)
                    Size = new Vector2(Size.X, Font.MeasureString(Text).Y + 5f);
            }
            else
            {
                srcRect[0].Width += 1;
                Size = new Vector2(texture.Width * scale, texture.Height * scale);
            }

            area.Width = (int)Size.X;
            area.Height = (int)Size.Y;
            
            Vector4 colorVect = BackColor.ToVector4();
            highlight = BackColor;
            BackColor = new Color(new Vector4(colorVect.X * 0.95f, colorVect.Y * 0.95f, colorVect.Z * 0.95f, colorVect.W));
            dimColor = new Color(new Vector4(colorVect.X * 0.85f, colorVect.Y * 0.85f, colorVect.Z * 0.85f, colorVect.W));

            colorVect = ForeColor.ToVector4();
            texthighlight = ForeColor;
            ForeColor = new Color(new Vector4(colorVect.X * 0.95f, colorVect.Y * 0.5f, colorVect.Z * 0.95f, colorVect.W));
            textdim = new Color(new Vector4(colorVect.X * 0.9f, colorVect.Y * 0.9f, colorVect.Z * 0.9f, colorVect.W));

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            if (texture != null)
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
            if (Owner != null && Owner.area.Contains(area) && area.Contains(MouseHelper.Cursor.Location))
            {
                if (!bMouseOver)
                {
                    bMouseOver = true;
                    if (OnMouseOver != null)
                        OnMouseOver(this, null);
                }

                if (!bMouseDown && MouseHelper.HasBeenPressed)
                {
                    bMouseDown = true;
                    if (OnPress != null)
                        OnPress(this, null);
                }
                else if (bMouseDown && MouseHelper.HasBeenReleased)
                {
                    bMouseDown = false;
                    if (OnRelease != null)
                        OnRelease(this, null);
                }
            }
            else if (bMouseOver)
            {
                bMouseOver = false;
                bMouseDown = false;
                if (OnMouseOut != null)
                    OnMouseOut(this, null);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (textureName == @"content\textures\controls\button\default.png")
                DrawDefaultButton(spriteBatch, Vector2.Zero, null);
            else if (texture != null)
                DrawCustomButton(spriteBatch, Vector2.Zero, null);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            area.X = (int)(Position.X + offset.X);
            area.Y = (int)(Position.Y + offset.Y);

            if (textureName == @"content\textures\controls\button\default.png")
                DrawDefaultButton(spriteBatch, offset, null);
            else if(texture != null)
                DrawCustomButton(spriteBatch, offset, null);
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (textureName == @"content\textures\controls\button\default.png")
                DrawDefaultButton(spriteBatch, Vector2.Zero, color);
            else
                DrawCustomButton(spriteBatch, Vector2.Zero, color);
        }

        private void DrawDefaultButton(SpriteBatch spriteBatch, Vector2 offset, Nullable<Color> color)
        {
            destRect[0].X = (int)(Position.X + offset.X);
            destRect[0].Y = (int)(Position.Y + offset.Y);
            destRect[0].Width = texture.Width - 1;
            destRect[0].Height = (int)Size.Y;

            destRect[1].X = destRect[0].X + destRect[0].Width;
            destRect[1].Y = destRect[0].Y;
            destRect[1].Width = (int)Size.X - destRect[0].Width * 2;
            destRect[1].Height = destRect[0].Height;

            destRect[2].X = destRect[1].X + destRect[1].Width;
            destRect[2].Y = destRect[0].Y;
            destRect[2].Width = destRect[0].Width;
            destRect[2].Height = destRect[0].Height;

            if (Text != string.Empty)
            {
                textPos.X = (int)(destRect[0].X + (Size.X - textSize.X) / 2f);
                textPos.Y = (int)(destRect[0].Y + (destRect[0].Height - textSize.Y) / 2f);
            }

            if (!color.HasValue && bMouseOver)
            {
                if (bMouseDown)
                {
                    spriteBatch.Draw(texture, destRect[0], srcRect[0], dimColor);
                    spriteBatch.Draw(texture, destRect[1], srcRect[1], dimColor);
                    spriteBatch.Draw(texture, destRect[2], srcRect[2], dimColor);
                    if (Text != string.Empty)
                        spriteBatch.DrawString(Font, Text, textPos, textdim);
                }
                else
                {
                    spriteBatch.Draw(texture, destRect[0], srcRect[0], highlight);
                    spriteBatch.Draw(texture, destRect[1], srcRect[1], highlight);
                    spriteBatch.Draw(texture, destRect[2], srcRect[2], highlight);
                    if (Text != string.Empty)
                        spriteBatch.DrawString(Font, Text, textPos, texthighlight);
                }
            }
            else if (!color.HasValue)
            {
                spriteBatch.Draw(texture, destRect[0], srcRect[0], BackColor);
                spriteBatch.Draw(texture, destRect[1], srcRect[1], BackColor);
                spriteBatch.Draw(texture, destRect[2], srcRect[2], BackColor);
                if (Text != string.Empty)
                    spriteBatch.DrawString(Font, Text, textPos, ForeColor);
            }
            else
            {
                spriteBatch.Draw(texture, destRect[0], srcRect[0], color.Value);
                spriteBatch.Draw(texture, destRect[1], srcRect[1], color.Value);
                spriteBatch.Draw(texture, destRect[2], srcRect[2], color.Value);
                if (Text != string.Empty)
                    spriteBatch.DrawString(Font, Text, textPos, ForeColor);
            }
        }

        private void DrawCustomButton(SpriteBatch spriteBatch, Vector2 offset, Nullable<Color> color)
        {
            if (!color.HasValue)
            {
                if (bMouseOver)
                {
                    if (bMouseDown)
                        spriteBatch.Draw(texture, Position + offset, srcRect[0], dimColor, 0f, Vector2.Zero, scale, effect, 0f);
                    else
                        spriteBatch.Draw(texture, Position + offset, srcRect[0], highlight, 0f, Vector2.Zero, scale, effect, 0f);
                }
                else
                    spriteBatch.Draw(texture, Position + offset, srcRect[0], BackColor, 0f, Vector2.Zero, scale, effect, 0f);
            }
            else
                spriteBatch.Draw(texture, Position + offset, srcRect[0], color.Value, 0f, Vector2.Zero, scale, effect, 0f);
        }
    }
}
