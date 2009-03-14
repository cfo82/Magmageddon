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
    public class ButtonRow: Control
    {
        Texture2D buttonTex, pixelTex;
        Rectangle[] sourceRect = new Rectangle[2];

        Vector2 size, tailPos;
        string[] titles;
        Vector2[] textPos;

        Color dimColor, highlight;
        int hoverindex = -1;

        Rectangle[] rect;
        Rectangle separator;

        Rectangle[] btRect;

        bool bMouseDown = false;

        int selectedIndex = 0;
        public int SelectedIndex { get { return selectedIndex; } set { selectedIndex = value; } }

        /// <summary>
        /// Button Row Constructor
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="position">Control Position</param>
        /// <param name="width">Control Width</param>
        /// <param name="titles">Button Titles</param>
        /// <param name="backColor">Backcolor</param>
        /// <param name="foreColor">Forecolor</param>
        public ButtonRow(string name, Vector2 position, float width, string[] titles, Color backColor, Color foreColor)
            : base(name)
        {
            this.Position = position;
            this.size = new Vector2(width, 0f);
            this.titles = titles;
            this.BackColor = backColor;
            this.highlight = new Color(backColor.ToVector3() * 0.9f);
            this.dimColor = new Color(backColor.ToVector3() * 0.85f);
            this.ForeColor = foreColor;
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            buttonTex = Texture2D.FromFile(graphics, @"content\textures\controls\button\default.png");
            
            pixelTex = new Texture2D(graphics, 1, 1);
            pixelTex.SetData<Color>(new Color[] { Color.White });

            sourceRect[0] = new Rectangle(0, 0, buttonTex.Width, buttonTex.Height);
            sourceRect[1] = new Rectangle(buttonTex.Width - 1, 0, 1, buttonTex.Height);

            area = new Rectangle((int)Position.X, (int)Position.Y, (int)size.X, buttonTex.Height);

            float cellWidth = (int)(size.X / titles.Length);
            rect = new Rectangle[titles.Length];
            btRect = new Rectangle[titles.Length];
            textPos = new Vector2[titles.Length];
            for (int i = 0; i < titles.Length; i++)
            {
                btRect[i] = new Rectangle(area.X + (int)(cellWidth * i), area.Y, (int)cellWidth, buttonTex.Height);
                textPos[i] = new Vector2(btRect[i].X + (btRect[i].Width - Font.MeasureString(titles[i]).X) / 2f, btRect[i].Y + (btRect[i].Height - Font.LineSpacing) / 2f);
                textPos[i].X = (int)textPos[i].X;
                textPos[i].Y = (int)textPos[i].Y;

                if (i == 0)
                    rect[i] = new Rectangle((int)Position.X + buttonTex.Width + (int)(cellWidth * i), (int)Position.Y, (int)cellWidth - buttonTex.Width, buttonTex.Height);
                else if (i == titles.Length - 1)
                    rect[i] = new Rectangle((int)Position.X + (int)(cellWidth * i), (int)Position.Y, (int)cellWidth - buttonTex.Width, buttonTex.Height);
                else
                    rect[i] = new Rectangle((int)Position.X + (int)(cellWidth * i), (int)Position.Y, (int)cellWidth, buttonTex.Height);

            }

            tailPos = new Vector2(rect[rect.Length - 1].X + rect[rect.Length - 1].Width, Position.Y);
            separator = new Rectangle(0, 0, 2, buttonTex.Height);

            this.Text = titles[0];

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            buttonTex.Dispose();
            pixelTex.Dispose();

            base.Dispose();
        }
        
        public override void Update(GameTime gameTime)
        {
            Vector2 rectPos = Vector2.Zero;
            Rectangle rect = Rectangle.Empty;

            rectPos.X = (int)Position.X + Owner.Position.X;
            rectPos.Y = (int)Position.Y + Owner.Position.Y;

            hoverindex = -1;
            for (int i = 0; i < btRect.Length; i++)
            {
                rect.X = (int)rectPos.X;
                rect.Y = (int)rectPos.Y;
                rect.Width = btRect[i].Width;
                rect.Height = btRect[i].Height;

                if (rect.Contains(MouseHelper.Cursor.Location) && Owner.area.Contains(rect))
                {
                    hoverindex = i;
                    if (MouseHelper.HasBeenPressed)
                    {
                        bMouseDown = true;
                        if (OnPress != null)
                            OnPress(this, null);
                    }
                    else if (bMouseDown && MouseHelper.HasBeenReleased)
                    {
                        bMouseDown = false;
                        selectedIndex = i;
                        this.Text = titles[selectedIndex];
                        if (OnRelease != null)
                            OnRelease(this, null);
                    }
                }

                rectPos.X += btRect[i].Width;
            }

            if (hoverindex == -1 & bMouseDown)
                bMouseDown = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (selectedIndex == 0)
                spriteBatch.Draw(buttonTex, Position, BackColor);
            else
                spriteBatch.Draw(buttonTex, Position, dimColor);

            for (int i = 0; i < titles.Length; i++)
            {
                if (i == selectedIndex)
                    spriteBatch.Draw(buttonTex, rect[i], sourceRect[1], BackColor);
                else if(i == hoverindex)
                    spriteBatch.Draw(buttonTex, rect[i], sourceRect[1], highlight);
                else
                    spriteBatch.Draw(buttonTex, rect[i], sourceRect[1], dimColor);
                
                if (i > 0)
                    spriteBatch.Draw(buttonTex, new Rectangle(rect[i].X - 1, rect[i].Y, 1, rect[i].Height), sourceRect[1], Color.Black);

                spriteBatch.DrawString(Font, titles[i], textPos[i], ForeColor);
            }            

            if(selectedIndex == titles.Length - 1)
                spriteBatch.Draw(buttonTex, tailPos, sourceRect[0], BackColor, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
            else
                spriteBatch.Draw(buttonTex, tailPos, sourceRect[0], dimColor, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
        }
    }
}
