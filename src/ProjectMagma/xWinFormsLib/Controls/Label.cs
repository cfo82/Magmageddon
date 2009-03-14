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
    public class Label : Control
    {
        Vector2 textOffset = Vector2.Zero;
        Vector2 drawPos = Vector2.Zero;
        Rectangle backgroundRect = Rectangle.Empty;

        Align alignment = Align.Left;
        public Align Alignment
        {
            get { return alignment; }
            set
            {
                alignment = value;
                if (!IsDisposed && Text != "")
                    UpdateText();
            }
        }
        public enum Align
        {
            Left,
            Center,
            Right
        }

        bool bMouseOver = false;
        bool bMouseDown = false;

        new public string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                if (!IsDisposed)
                    UpdateText();
            }
        }
        List<string> lines = new List<string>();

        Texture2D pixelTex;

        //bool wordwrap = false;

        public Label(string name, Vector2 position, string text, Color backColor, Color foreColor, int width, Align alignment)
            : base(name, position)
        {
            this.Text = text;
            this.BackColor = backColor;
            this.ForeColor = foreColor;
            this.alignment = alignment;
            this.Width = width;
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            pixelTex = new Texture2D(graphics, 1, 1);
            pixelTex.SetData<Color>(new Color[] { Color.White });

            if (Text != "")
                UpdateText();

            //if (Owner != null)
            //    Font = Owner.Font;
            //else
            //    Font = content.Load<SpriteFont>(@"content\fonts\" + FontName);

            //if (Width == 0)
            //    Width = Font.MeasureString(Text).X;

            if (lines.Count > 0)
                Height = Font.LineSpacing * lines.Count;
            else
                Height = Font.LineSpacing;

            area.Width = (int)Width;
            area.Height = (int)Height;

            backgroundRect.Width = area.Width;
            backgroundRect.Height = area.Height;

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            pixelTex.Dispose();
            base.Dispose();
        }

        private void UpdateText()
        {
            string[] strLines = Text.Split(new char[] { '\n' });

            lines.Clear();
            for (int i = 0; i < strLines.Length; i++)
            {
                strLines[i] = strLines[i].Replace("\r", "");
                strLines[i] = strLines[i].Replace("\u0009", "      ");
                //strLines[i] = strLines[i].Trim();
                lines.Add(strLines[i]);                
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 0)
                {
                    for (int c = 0; c < lines[i].Length; c++)
                        if (Font.MeasureString(lines[i].Substring(0, c)).X > Width)
                        {
                            string newLine = lines[i].Substring(c - 1, lines[i].Length - (c - 1));
                            lines[i] = lines[i].Substring(0, (c - 1));
                            lines.Insert(i + 1, newLine);
                            break;
                        }
                }
            }
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
            backgroundRect.X = (int)(area.X - Owner.Position.X);
            backgroundRect.Y = (int)(area.Y - Owner.Position.Y);
            spriteBatch.Draw(pixelTex,backgroundRect, BackColor);

            if (lines.Count <= 1)
            {
                switch (alignment)
                {
                    case Align.Right:
                        textOffset.X = Width - Font.MeasureString(Text).X;
                        break;
                    case Align.Center:
                        textOffset.X = (Width - Font.MeasureString(Text).X) / 2f;
                        break;
                }

                drawPos = new Vector2((int)(Position.X + textOffset.X), (int)(Position.Y));
                spriteBatch.DrawString(Font, Text, drawPos, ForeColor);
            }
            else
                for (int i = 0; i < lines.Count; i++)
                {
                    if (alignment == Align.Right)
                        drawPos.X = (int)(Position.X + Width - Font.MeasureString(lines[i]).X);
                    else if (alignment == Align.Center)
                        drawPos.X = (int)(Position.X + (Width - Font.MeasureString(lines[i]).X) / 2f);
                    else if (alignment == Align.Left)
                        drawPos.X = (int)Position.X;

                    drawPos.Y = (int)(Position.Y + i * Font.LineSpacing);

                    spriteBatch.DrawString(Font, lines[i], drawPos, ForeColor);
                }

            base.Draw(spriteBatch);
        }
    }
}
