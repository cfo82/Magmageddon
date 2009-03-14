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
    public class Slider: Control
    {        
        Texture2D[] texture = new Texture2D[3];
        Rectangle[] rect = new Rectangle[4];

        Color highlightColor = Color.LightBlue;
        public Color HighlightColor { set { highlightColor = value; } }
        bool hasHighlight = false;
        public bool HasHighlight { set { hasHighlight = value; } }

        Vector2 cursorPos;
        Rectangle cursorArea;
        Vector2 cursorOffset = new Vector2(-2f, -2f);

        int width;
        float max = 100;
        
        float value = 0;
        public float Value
        {
            get { return value; }
            set { this.value = value; }
        }

        bool isSliding = false;
        Vector2 slideOffset = Vector2.Zero;
        Rectangle sliderArea = Rectangle.Empty;

        public EventHandler OnValueChanged;

        public Slider(string name, Vector2 position, int width)
            : base(name, position)
        {
            this.width = width;
            this.ForeColor = Color.White;
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            //Slider
            texture[0] = Texture2D.FromFile(graphics, @"content\textures\controls\slider\slider_left.png");
            texture[1] = Texture2D.FromFile(graphics, @"content\textures\controls\slider\slider_center.png");
            rect[0] = new Rectangle((int)Position.X + texture[0].Width, (int)Position.Y, width - (texture[0].Width * 2), texture[0].Height);
            rect[1] = new Rectangle(0, 0, texture[0].Width, texture[0].Height); //source rect (right part)
            rect[2] = new Rectangle(rect[0].X + rect[0].Width, rect[0].Y, texture[0].Width, texture[0].Height);
            rect[3] = rect[0];

            //Cursor
            texture[2] = Texture2D.FromFile(graphics, @"content\textures\controls\slider\slider_cursor.png");
            cursorArea = new Rectangle(0, 0, texture[2].Width, texture[2].Height);

            sliderArea.Width = width;
            sliderArea.Height = texture[1].Height;

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            for (int i = 0; i < texture.Length; i++)
            {
                if (texture[i] != null)
                {
                    texture[i].Dispose();
                    texture[i] = null;
                }
            }

            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            if (FormCollection.TopMostForm != Owner)
                return;

            UpdateSliding();

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            cursorArea.X = (int)(Position.X + cursorPos.X);
            cursorArea.Y = (int)(Position.Y + cursorPos.Y);

            if (Owner != null)
            {
                cursorArea.X += (int)Owner.Position.X;
                cursorArea.Y += (int)Owner.Position.Y;
            }

            if (!isSliding)
                cursorPos.X = (int)(((float)value / (float)max) * (width - 8));
            else
            {
                cursorPos.X = MouseHelper.Cursor.Location.X - (Position.X + Owner.Position.X) - slideOffset.X;

                if (cursorPos.X < 0)
                    cursorPos.X = 0;
                else if (cursorPos.X > width - 8)
                    cursorPos.X = width - 8;
            }

            if (value != cursorPos.X / (width - 8) * max)
            {
                value = cursorPos.X / (width - 8) * max;
                if (OnValueChanged != null)
                    OnValueChanged(cursorPos.X / (width - 8) * max, null);
            }
        }

        private void UpdateSliding()
        {
            sliderArea.X = rect[0].X;
            sliderArea.Y = rect[0].Y;

            if (cursorArea.Contains(MouseHelper.Cursor.Location) && Owner.area.Contains(cursorArea) && MouseHelper.HasBeenPressed)
            {
                isSliding = true;
                slideOffset = new Vector2(MouseHelper.Cursor.Location.X - cursorArea.X, MouseHelper.Cursor.Location.Y - cursorArea.Y);
            }
            
            if (isSliding && MouseHelper.IsReleased)
                isSliding = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            rect[0].X = (int)(Position.X + texture[0].Width);
            rect[0].Y = (int)(Position.Y);
            rect[2].X = rect[0].X + rect[0].Width;
            rect[2].Y = rect[0].Y;
            rect[3].X = rect[0].X;
            rect[3].Y = rect[0].Y;

            //Draw Slider
            spriteBatch.Draw(texture[0], Position, BackColor);
            spriteBatch.Draw(texture[1], rect[0], BackColor);
            spriteBatch.Draw(texture[0], rect[2], rect[1], BackColor, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);

            //Draw Slider Overlay
            if (hasHighlight)
            {
                if (value > 0)
                    spriteBatch.Draw(texture[0], Position, highlightColor);

                if (value > 2)
                {
                    rect[3].Width = (int)((float)value / (float)max * (float)rect[0].Width) - 5;
                    if (value > 0)
                        spriteBatch.Draw(texture[1], rect[3], highlightColor);
                }
            }

            //Draw Cursor
            spriteBatch.Draw(texture[2], Position + cursorPos + cursorOffset, ForeColor);
        }
    }
}
