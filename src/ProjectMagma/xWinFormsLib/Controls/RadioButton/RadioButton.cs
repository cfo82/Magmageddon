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
    public class RadioButton: Control
    {
        Texture2D[] texture;
        Vector2 textOffset = Vector2.Zero;
        
        bool bMouseDown = false;
        bool bMouseOver = false;

        bool bChecked = false;
        public bool Value { get { return bChecked; } set { bChecked = value; } }

        bool bCanUncheck = true;
        public bool CanUncheck { get { return bCanUncheck; } set { bCanUncheck = value; } }
         
        public RadioButton(string name, Vector2 position, string text, bool bChecked)
            : base(name, position)
        {
            this.Text = text;
            this.bChecked = bChecked;
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            texture = new Texture2D[2];
            texture[0] = Texture2D.FromFile(graphics, @"content\textures\controls\radiobutton\checked.png");
            texture[1] = Texture2D.FromFile(graphics, @"content\textures\controls\radiobutton\unchecked.png");

            Width = (int)(texture[0].Width + Font.MeasureString(Text).X + 15);
            Height = (int)(System.Math.Max(texture[0].Height, Font.LineSpacing));

            area = new Rectangle(0, 0, (int)Size.X, (int)Size.Y);

            textOffset.X = texture[0].Width + 5f;
            textOffset.Y = System.Math.Abs(texture[0].Height - Font.LineSpacing) - 1;

            base.Initialize(content, graphics);
        }

        public void Toggle()
        {
            if (bChecked && bCanUncheck)
                bChecked = false;
            else if (!bChecked)
                bChecked = true;
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
                    Toggle();

                    if (!bMouseDown)
                    {
                        bMouseDown = true;
                        if (OnPress != null)
                            OnPress(this, null);
                    }
                }
                else if (bMouseDown && MouseHelper.HasBeenReleased)
                {
                    if (OnRelease != null)
                        OnRelease(this, null);
                }
            }
            else
            {
                if (bMouseOver)
                {
                    bMouseOver = false;
                    if (OnMouseOut != null)
                        OnMouseOut(this, null);
                }

                bMouseDown = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (bChecked)
                spriteBatch.Draw(texture[0], Position, BackColor);
            else
                spriteBatch.Draw(texture[1], Position, BackColor);

            spriteBatch.DrawString(Font, Text, Position + textOffset, ForeColor);

            base.Draw(spriteBatch);
        }
    }
}
