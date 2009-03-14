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
    public class Textbox : Control
    {
        Texture2D texture;
        Rectangle[] srcRect;
        Rectangle[] destRect;

        Vector2 textOffset;

        KeyboardHelper keyboard = new KeyboardHelper();
        
        SpriteFont cursorFont;

        Point cursorLocation = Point.Zero;
        Point previousLocation = Point.Zero;

        Vector2 cursorOffset = Vector2.Zero;
        Vector2 scrollOffset = Vector2.Zero;

        bool hasFocus = false;
        public bool HasFocus { get { return hasFocus; } set { hasFocus = value; } }
        bool multiline = false;
        List<string> line = new List<string>();
        Vector2 lineOffset = Vector2.Zero;
        int visibleLines = 0;

        bool locked = false;
        public bool Locked { get { return locked; } set { locked = value; } }

        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;
        Texture2D capturedTexture;
        Rectangle sRect, dRect;

        Scrollbar vscrollbar, hscrollbar;
        
        Scrollbars scrollbar = Scrollbars.None;
        public Scrollbars Scrollbar { get { return scrollbar; } set { scrollbar = value; } }
        public enum Scrollbars
        {
            None,
            Horizontal,
            Vertical,
            Both
        }

        bool bCursorVisible = true;
        System.Timers.Timer timer = new System.Timers.Timer(500);

        public Textbox(string name, Vector2 position, int width)
            : base(name, position)
        {
            this.Width = width;
        }
        public Textbox(string name, Vector2 position, int width, string text)
            : base(name, position)
        {
            this.Width = width;
            Add(text);
            cursorLocation.X = this.Text.Length;
        }
        public Textbox(string name, Vector2 position, int width, int height, string text)
            : base(name, position)
        {
            this.Width = width;
            this.Height = height;
            this.multiline = true;
            Add(text);
            cursorLocation.Y = line.Count - 1;
            cursorLocation.X = line[line.Count - 1].Length;
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            // TODO: load your content here
            if (texture == null)
                texture = Texture2D.FromFile(graphics, @"content\textures\controls\textbox.png");
            if (cursorFont == null)
                cursorFont = content.Load<SpriteFont>(@"content\fonts\kootenay9");

            textOffset = new Vector2(5f, (int)((texture.Height - Font.LineSpacing) / 2f));

            if (!multiline)
            {
                srcRect = new Rectangle[3];
                srcRect[0] = new Rectangle(0, 0, texture.Width - 1, texture.Height);
                srcRect[1] = new Rectangle(texture.Width - 1, 0, 1, texture.Height);
                srcRect[2] = new Rectangle(texture.Width - 1, 0, -(texture.Width - 1), texture.Height);
                destRect = new Rectangle[3];
                destRect[0] = new Rectangle(0, 0, srcRect[0].Width, srcRect[0].Height);
                destRect[1] = new Rectangle(0, 0, (int)Width - srcRect[0].Width * 2, srcRect[1].Height);
                destRect[2] = new Rectangle(0, 0, srcRect[0].Width, srcRect[0].Height);
                Height = texture.Height;
            }
            else
            {
                visibleLines = (int)System.Math.Ceiling(Height / Font.LineSpacing);
                Height = visibleLines * Font.LineSpacing + 2;

                //if (IsDisposed)
                //{
                    srcRect = new Rectangle[9];
                    srcRect[0] = new Rectangle(0, 0, texture.Width - 1, texture.Height / 2);
                    srcRect[1] = new Rectangle(texture.Width - 1, 0, 1, texture.Height / 2);
                    srcRect[2] = new Rectangle(texture.Width - 1, 0, -(texture.Width - 1), texture.Height / 2);

                    srcRect[3] = new Rectangle(0, texture.Height / 2, texture.Width - 1, 1);
                    srcRect[4] = new Rectangle(texture.Width - 1, texture.Height / 2, 1, 1);
                    srcRect[5] = new Rectangle(texture.Width - 1, texture.Height / 2, -(texture.Width - 1), 1);

                    srcRect[6] = new Rectangle(0, texture.Height / 2, texture.Width - 1, -(texture.Height / 2));
                    srcRect[7] = new Rectangle(texture.Width - 1, texture.Height / 2, 1, -(texture.Height / 2));
                    srcRect[8] = new Rectangle(texture.Width - 1, texture.Height / 2, -(texture.Width - 1), -(texture.Height / 2));

                    destRect = new Rectangle[9];
                    destRect[0] = new Rectangle(0, 0, srcRect[0].Width, srcRect[0].Height);
                    destRect[1] = new Rectangle(0, 0, (int)Width - srcRect[0].Width * 2, srcRect[0].Height);
                    destRect[2] = new Rectangle(0, 0, srcRect[0].Width, srcRect[0].Height);

                    destRect[3] = new Rectangle(0, 0, srcRect[0].Width, (int)Height - srcRect[0].Height * 2);
                    destRect[4] = new Rectangle(0, 0, destRect[1].Width, destRect[3].Height);
                    destRect[5] = new Rectangle(0, 0, srcRect[0].Width, destRect[3].Height);

                    destRect[6] = new Rectangle(0, 0, srcRect[0].Width, srcRect[0].Height);
                    destRect[7] = new Rectangle(0, 0, destRect[1].Width, srcRect[0].Height);
                    destRect[8] = new Rectangle(0, 0, srcRect[0].Width, srcRect[0].Height);
                //}
            }

            area.Width = (int)Width;
            area.Height = (int)Height;

            if (IsDisposed)
                spriteBatch = new SpriteBatch(graphics);

            try
            {
                renderTarget = new RenderTarget2D(graphics, (int)Width, (int)Height, 1, graphics.PresentationParameters.BackBufferFormat);
            }
            catch { }

            sRect = new Rectangle(0, 0, (int)Width, (int)Height);
            dRect = new Rectangle(0, 0, (int)Width, (int)Height);

            if (keyboard.OnKeyPress == null)
                keyboard.OnKeyPress += Keyboard_OnPress;
            if (keyboard.OnPaste == null)
                keyboard.OnPaste += Keyboard_OnPaste;

            if (multiline && scrollbar != Textbox.Scrollbars.None)
                InitScrollbars(content, graphics);

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            base.Initialize(content, graphics);
        }

        private void InitScrollbars(ContentManager content, GraphicsDevice graphics)
        {
            if (scrollbar == Scrollbars.Vertical)
                vscrollbar = new Scrollbar("vscrollbar", Position + new Vector2(Width - 2, 1), xWinFormsLib.Scrollbar.Type.Vertical, (int)Height - 2);
            else if(scrollbar == Scrollbars.Horizontal)
                hscrollbar = new Scrollbar("hscrollbar", Position + new Vector2(1, Height - 2), xWinFormsLib.Scrollbar.Type.Horizontal, (int)Width - 2);
            else if (scrollbar == Scrollbars.Both)
            {
                vscrollbar = new Scrollbar("vscrollbar", Position + new Vector2(Width - 13, 1), xWinFormsLib.Scrollbar.Type.Vertical, (int)Height - 14);
                hscrollbar = new Scrollbar("hscrollbar", Position + new Vector2(1, Height - 13), xWinFormsLib.Scrollbar.Type.Horizontal, (int)Width - 14);
            }

            if (vscrollbar != null)
            {
                vscrollbar.Owner = this.Owner;
                vscrollbar.OnChangeValue = vScrollbar_OnChangeValue;
                vscrollbar.Initialize(content, graphics);
            }
            if (hscrollbar != null)
            {
                hscrollbar.Owner = this.Owner;
                hscrollbar.OnChangeValue = hScrollbar_OnChangeValue;
                hscrollbar.Initialize(content, graphics);
            }
        }

        public override void Dispose()
        {
            // TODO: dispose of your content here
            capturedTexture.Dispose();
            renderTarget.Dispose();

            if (multiline)
            {
                if (vscrollbar != null)
                    vscrollbar.Dispose();
                if (hscrollbar != null)
                    hscrollbar.Dispose();
            }

            texture.Dispose();
            spriteBatch.Dispose();

            base.Dispose();
        }

        new public string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = "";
                line.Clear();
                Add(value);

                if (cursorLocation.X > base.Text.Length)
                {
                    cursorLocation.X = 0;
                }
            }
        }

        /* Need to be optimized.. */
        private void Add(string text)
        {
            if (!multiline)
            {
                base.Text = base.Text.Insert(cursorLocation.X > base.Text.Length ? 0 : cursorLocation.X, text);
                //cursorLocation.X += text.Length;
            }
            else
            {
                if (text.Contains("\n"))
                {
                    string[] lines = text.Split(new char[] { '\n' });

                    for (int i = 0; i < lines.Length; i++)
                    {
                        char tab = '\u0009';
                        lines[i] = lines[i].Replace(tab.ToString(), "    ");
                        if (i == 0)
                        {
                            if (line.Count == 0)
                                line.Add("");
                            if (cursorLocation.X > line[cursorLocation.Y].Length)
                                cursorLocation.X = line[cursorLocation.Y].Length;
                            line[cursorLocation.Y] = line[cursorLocation.Y].Insert(cursorLocation.X, lines[i]);
                        }
                        else
                            line.Insert(cursorLocation.Y + i, lines[i]);
                    }
                }
                else
                {
                    if (line.Count == 0)
                        line.Add("");

                    if (cursorLocation.X > line[cursorLocation.Y].Length)
                        cursorLocation.X = line[cursorLocation.Y].Length;

                    line[cursorLocation.Y] = line[cursorLocation.Y].Insert(cursorLocation.X, text);
                }

                //base.Text = "";
                for (int i = 0; i < line.Count; i++)
                    base.Text += line[i];
            }

            UpdateScrolling();
        }

        private void Keyboard_OnPress(object obj, EventArgs e)
        {
            if (obj == null)
                return;

            Keys key = (Keys)obj;
            KeyboardEventsArgs args = (KeyboardEventsArgs)e;

            switch (key)
            {
                case Keys.Left:
                    #region Move Cursor Left
                    if (args.ControlDown)
                    {
                        bool foundSpace = false;
                        if (line.Count > 0)
                        {
                            for (int i = cursorLocation.X - 2; i > 0; i--)
                                if (line[cursorLocation.Y].Substring(i, 1) == " ")
                                {
                                    cursorLocation.X = i + 1;
                                    foundSpace = true;
                                    break;
                                }
                        }

                        if (!foundSpace)
                            if (cursorLocation.X != 0)
                                cursorLocation.X = 0;
                            else if (cursorLocation.Y > 0)
                            {
                                cursorLocation.Y -= 1;
                                cursorLocation.X = line[cursorLocation.Y].Length;
                            }
                    }
                    else
                    {
                        if (!multiline && cursorLocation.X > 0)
                            cursorLocation.X -= 1;
                        else if (multiline)
                        {
                            if (cursorLocation.X > 0)
                                cursorLocation.X -= 1;
                            else if (cursorLocation.Y > 0)
                            {
                                cursorLocation.X = line[cursorLocation.Y - 1].Length;
                                cursorLocation.Y -= 1;
                            }
                        }
                    }
                    #endregion
                    break;
                case Keys.Right:
                    #region Move Cursor Right
                    if (args.ControlDown)
                    {
                        bool foundSpace = false;
                        if (line.Count > 0)
                        {
                            for (int i = cursorLocation.X; i < line[cursorLocation.Y].Length; i++)
                                if (line[cursorLocation.Y].Substring(i, 1) == " ")
                                {
                                    cursorLocation.X = i + 1;
                                    foundSpace = true;
                                    break;
                                }

                            if (!foundSpace)
                                if (cursorLocation.X != line[cursorLocation.Y].Length)
                                    cursorLocation.X = line[cursorLocation.Y].Length;
                                else if (cursorLocation.Y < line.Count - 1)
                                {
                                    cursorLocation.X = 0;
                                    cursorLocation.Y += 1;
                                }
                        }
                    }
                    else
                    {
                        if (!multiline && cursorLocation.X < Text.Length)
                            cursorLocation.X += 1;
                        else if (multiline)
                        {
                            if (cursorLocation.X < line[cursorLocation.Y].Length)
                                cursorLocation.X += 1;
                            else if (cursorLocation.Y < line.Count - 1)
                            {
                                cursorLocation.X = 0;
                                cursorLocation.Y += 1;
                            }
                        }
                    }
                    #endregion
                    break;
                case Keys.Up:
                    if (cursorLocation.Y > 0)
                        cursorLocation.Y -= 1;
                    break;
                case Keys.Down:
                    if (cursorLocation.Y < line.Count - 1)
                        cursorLocation.Y += 1;
                    break;
                case Keys.Back:
                    #region Backspace
                    if (!multiline && cursorLocation.X > 0)
                    {
                        if (cursorLocation.X > 1 && Text.Substring(cursorLocation.X - 2, 2) == "\n")
                        {
                            Text = Text.Remove(cursorLocation.X - 2, 2);
                            cursorLocation.X -= 2;
                        }
                        else
                        {
                            Text = Text.Remove(cursorLocation.X - 1, 1);
                            cursorLocation.X -= 1;
                        }
                    }
                    else if (multiline)
                    {
                        if (cursorLocation.X > 0)
                        {
                            line[cursorLocation.Y] = line[cursorLocation.Y].Remove(cursorLocation.X - 1, 1);
                            cursorLocation.X -= 1;
                        }
                        else
                        {
                            if (cursorLocation.Y > 0)
                            {
                                cursorLocation.X = line[cursorLocation.Y - 1].Length;
                                line[cursorLocation.Y - 1] += line[cursorLocation.Y];
                                line.RemoveAt(cursorLocation.Y);
                                cursorLocation.Y -= 1;
                            }
                        }
                    }
                    #endregion
                    break;
                case Keys.Delete:
                    #region Delete
                    if (!multiline && cursorLocation.X < Text.Length)
                    {
                        if (cursorLocation.X < Text.Length - 1 && Text.Substring(cursorLocation.X, 2) == "\n")
                            Text = Text.Remove(cursorLocation.X, 2);
                        else
                            Text = Text.Remove(cursorLocation.X, 1);
                    }
                    else if (multiline)
                    {
                        if (cursorLocation.X < line[cursorLocation.Y].Length)
                            line[cursorLocation.Y] = line[cursorLocation.Y].Remove(cursorLocation.X, 1);
                        else if (cursorLocation.X == line[cursorLocation.Y].Length)
                            if (cursorLocation.Y < line.Count - 1)
                            {
                                if (line[cursorLocation.Y + 1].Length > 0)
                                    line[cursorLocation.Y] += line[cursorLocation.Y + 1];
                                line.RemoveAt(cursorLocation.Y + 1);
                            }
                    }
                    #endregion
                    break;
                case Keys.Home:
                    cursorLocation.X = 0;
                    if (multiline && args.ControlDown)
                    {
                        cursorLocation.Y = 0;
                        scrollOffset.X = 0;
                    }
                    break;
                case Keys.End:
                    if (!multiline)
                        cursorLocation.X = Text.Length;
                    else
                    {
                        if (args.ControlDown)
                        {
                            cursorLocation.Y = line.Count - 1;
                            cursorLocation.X = line[cursorLocation.Y].Length;                            
                        }
                        else
                            cursorLocation.X = line[cursorLocation.Y].Length;
                    }
                    break;
                case Keys.Enter:
                    if (multiline)
                    {
                        string lineEnd = "";

                        if (line[cursorLocation.Y].Length > cursorLocation.X)
                        {
                            lineEnd = line[cursorLocation.Y].Substring(cursorLocation.X, line[cursorLocation.Y].Length - cursorLocation.X);
                            line[cursorLocation.Y] = line[cursorLocation.Y].Substring(0, line[cursorLocation.Y].Length - lineEnd.Length);
                        }

                        line.Insert(cursorLocation.Y + 1, lineEnd);
                        cursorLocation.X = 0;
                        cursorLocation.Y += 1;
                    }
                    break;
                case Keys.Space:
                    Add(" ");
                    cursorLocation.X += 1;
                    break;
                case Keys.Decimal:
                    Add(".");
                    cursorLocation.X += 1;
                    break;
                case Keys.Divide:
                    Add("/");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemBackslash:
                    if (args.ShiftDown) Add("|"); else Add("\\");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemCloseBrackets:
                    if (args.ShiftDown) Add("}"); else Add("]");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemComma:
                    if (args.ShiftDown) Add("<"); else Add(",");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemMinus:
                    if (args.ShiftDown) Add("_"); else Add("-");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemOpenBrackets:
                    if (args.ShiftDown) Add("{"); else Add("[");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemPeriod:
                    if (args.ShiftDown) Add(">"); else Add(".");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemPipe:
                    if (args.ShiftDown) Add("|"); else Add("\\");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemPlus:
                    if (args.ShiftDown) Add("+"); else Add("=");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemQuestion:
                    if (args.ShiftDown) Add("?"); else Add("/");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemQuotes:
                    if (args.ShiftDown) Add("\""); else Add("'");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemSemicolon:
                    if (args.ShiftDown) Add(":"); else Add(";");
                    cursorLocation.X += 1;
                    break;
                case Keys.OemTilde:
                    if (args.ShiftDown) Add("~"); else Add("`");
                    cursorLocation.X += 1;
                    break;
                case Keys.Subtract:
                    Add("-");
                    cursorLocation.X += 1;
                    break;
                case Keys.Multiply:
                    Add("*");
                    cursorLocation.X += 1;
                    break;
                case Keys.Tab:
                    Add("      ");
                    cursorLocation.X += 6;
                    break;
                case Keys.Add:
                    Add("+");
                    cursorLocation.X += 1;
                    break;
                default:
                    string k = key.ToString();
                    if (k.Length == 1)
                    {
                        if (args.CapsLock || args.ShiftDown)
                            Add(k.ToUpper());
                        else
                            Add(k.ToLower());

                        cursorLocation.X += 1;
                    }
                    else if (k.Length == 2 && k.StartsWith("D"))
                    {
                        if (args.ShiftDown)
                        {
                            switch (int.Parse(k.Substring(1, 1)))
                            {
                                case 0:
                                    Add(")");
                                    break;
                                case 1:
                                    Add("!");
                                    break;
                                case 2:
                                    Add("@");
                                    break;
                                case 3:
                                    Add("#");
                                    break;
                                case 4:
                                    Add("$");
                                    break;
                                case 5:
                                    Add("%");
                                    break;
                                case 6:
                                    Add("^");
                                    break;
                                case 7:
                                    Add("&");
                                    break;
                                case 8:
                                    Add("*");
                                    break;
                                case 9:
                                    Add("(");
                                    break;
                            }
                        }
                        else
                            Add(k.Substring(1, 1));

                        cursorLocation.X += 1;
                    }
                    break;
            }

            UpdateScrolling();

            bCursorVisible = true;
            timer.Start();
        }

        private void Keyboard_OnPaste(object obj, EventArgs e)
        {
            if (obj != null)
            {
                System.Windows.Forms.IDataObject dataObj = (System.Windows.Forms.IDataObject)obj;
                if (dataObj.GetDataPresent(System.Windows.Forms.DataFormats.Text))
                {
                    string clipboardText = (string)dataObj.GetData(System.Windows.Forms.DataFormats.Text);
                    clipboardText = clipboardText.Replace("\r", "");
                    if (multiline)
                        clipboardText = clipboardText.Replace("\t", "    ");

                    Add(clipboardText);

                    Vector2 textSize = Font.MeasureString(clipboardText);
                    cursorLocation.Y += (int)(textSize.Y / Font.LineSpacing) - 1;

                    string[] lines = clipboardText.Split(new char[] { '\n' });
                    cursorLocation.X += lines[lines.Length - 1].Length;

                    UpdateScrolling();
                }
            }
        }

        private void vScrollbar_OnChangeValue(object obj, EventArgs e)
        {
            scrollOffset.Y = vscrollbar.Value * Font.LineSpacing;
            hasFocus = true;
        }

        private void hScrollbar_OnChangeValue(object obj, EventArgs e)
        {
            if (multiline)
            {
                scrollOffset.X = hscrollbar.Value;
                hasFocus = true;
            }
        }

        private void UpdateScrollPosition(object obj, EventArgs e)
        {
            scrollOffset.Y = vscrollbar.Value * Font.LineSpacing;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: Add your update logic here
            CheckFocus();

            if (hasFocus && !locked)
            {
                keyboard.Update();
                
                if (multiline && scrollbar == Scrollbars.Vertical && cursorLocation.X > line[cursorLocation.Y].Length)
                    cursorLocation.X = line[cursorLocation.Y].Length;
            }

            if (multiline && scrollbar != Scrollbars.None)
            {
                //UpdateScrolling();
                UpdateScrollbars(gameTime);
            }
        }

        private void UpdateScrollbars(GameTime gameTime)
        {
            if (vscrollbar != null)
            {
                if (hscrollbar != null && hscrollbar.Max > 0)
                    vscrollbar.Max = System.Math.Max(0, line.Count - (visibleLines - 1));
                else
                    vscrollbar.Max = System.Math.Max(0, line.Count - visibleLines);
            }


            if (vscrollbar != null && vscrollbar.Max > 0)
            {
                if (hscrollbar != null && hscrollbar.Max > 0)
                    vscrollbar.Height = Height - 13;
                else
                    vscrollbar.Height = Height - 2;

                vscrollbar.Update(gameTime);
            }

            if (hscrollbar != null && hscrollbar.Max > 0)
            {
                if (vscrollbar != null && vscrollbar.Max > 0)
                    hscrollbar.Width = Width - 13;
                else
                    hscrollbar.Width = Width - 2;

                hscrollbar.Update(gameTime);
            }
        }

        Rectangle focusArea = Rectangle.Empty;
        private void CheckFocus()
        {
            focusArea.X = (int)(Position.X + Owner.Position.X);
            focusArea.Y = (int)(Position.Y + Owner.Position.Y);
            focusArea.Width = sRect.Width;
            focusArea.Height = sRect.Height;

            if (MouseHelper.HasBeenPressed && Owner.area.Contains(MouseHelper.Cursor.Location))
            {
                if (area.Contains(MouseHelper.Cursor.Location))
                    hasFocus = true;
                else if (!Owner.IsDragged)
                    hasFocus = false;
            }
        }

        private void timer_Elapsed(object obj, EventArgs e)
        {
            bCursorVisible = !bCursorVisible;
        }

        private void UpdateScrolling()
        {
            if (cursorOffset.X > scrollOffset.X + (sRect.Width - 20))
                scrollOffset.X = cursorOffset.X - (sRect.Width - 20);
            else if (cursorOffset.X - 20 < scrollOffset.X)
                scrollOffset.X = System.Math.Max(0, cursorOffset.X - 20);

            if (scrollOffset.X < 0f)
                scrollOffset.X = 0f;
            if (cursorLocation.X == 0)
                scrollOffset.X = 0;

            if(multiline)
            {
                #region horizontal

                //if (hscrollbar != null && scrollOffset.X > hscrollbar.Max)
                //    scrollOffset.X = hscrollbar.Max;

                if (hscrollbar != null)
                    hscrollbar.Value = (int)scrollOffset.X;

                #endregion

                #region vertical

                if (Font != null)
                {
                    int offsetY = (int)(scrollOffset.Y / Font.LineSpacing);

                    if (hscrollbar != null && hscrollbar.Max > 0)
                    {
                        if (cursorLocation.Y > offsetY + (visibleLines - 2))
                            scrollOffset.Y = (cursorLocation.Y - (visibleLines - 2)) * Font.LineSpacing;
                        else if (cursorLocation.Y < offsetY)
                            scrollOffset.Y = cursorLocation.Y * Font.LineSpacing;
                    }
                    else
                    {
                        if (cursorLocation.Y > offsetY + (visibleLines - 1))
                            scrollOffset.Y = (cursorLocation.Y - (visibleLines - 1)) * Font.LineSpacing;
                        else if (cursorLocation.Y < offsetY)
                            scrollOffset.Y = cursorLocation.Y * Font.LineSpacing;
                    }

                    if (vscrollbar != null)
                        vscrollbar.Value = (int)(scrollOffset.Y / Font.LineSpacing);
                }

                #endregion
            }
        }

        private void Render(GraphicsDevice graphics)
        {
            graphics.SetRenderTarget(0, renderTarget);

            // TODO: Add your code to draw to the render target here.
            RenderText(graphics);

            graphics.SetRenderTarget(0, null);
            capturedTexture = renderTarget.GetTexture();
            graphics.Clear(Color.TransparentBlack);
        }

        private void RenderText(GraphicsDevice graphics)
        {
            graphics.Clear(Color.TransparentBlack);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            if (!multiline)
            {
                cursorOffset.X = Font.MeasureString(Text.Substring(0, cursorLocation.X)).X + 4f;
                cursorOffset.Y = 1f;
                spriteBatch.DrawString(Font, Text, textOffset - scrollOffset, ForeColor);
                if (hasFocus && !locked && bCursorVisible && Owner == FormCollection.TopMostForm)
                    spriteBatch.DrawString(cursorFont, "|", cursorOffset - scrollOffset, Color.Black);
            }
            else
            {
                float maxWidth = 0;
                for (int i = 0; i < line.Count; i++)
                {
                    lineOffset.Y = i * Font.LineSpacing;
                    spriteBatch.DrawString(Font, line[i], textOffset + lineOffset - scrollOffset, ForeColor);

                    if (vscrollbar != null && vscrollbar.Max > 0 && Font.MeasureString(line[i]).X - Width > maxWidth)
                        maxWidth = Font.MeasureString(line[i]).X - Width;
                    else if (Font.MeasureString(line[i]).X - Width > maxWidth)
                        maxWidth = Font.MeasureString(line[i]).X - Width + 12;                    
                }

                if (hscrollbar != null)
                {
                    hscrollbar.Max = (int)maxWidth;
                    if (vscrollbar != null && vscrollbar.Max > 0 && hscrollbar.Max > 0)
                        hscrollbar.Max += 20;
                    hscrollbar.Step = System.Math.Max(1, hscrollbar.Max / 15);
                }

                if (hasFocus && !locked && bCursorVisible && Owner == FormCollection.TopMostForm)
                {
                    if (cursorLocation.X > line[cursorLocation.Y].Length)
                        cursorLocation.X = line[cursorLocation.Y].Length;
                    
                    cursorOffset.X = (int)Font.MeasureString(line[cursorLocation.Y].Substring(0, cursorLocation.X)).X + 4;
                    cursorOffset.Y = (int)(Font.LineSpacing * cursorLocation.Y) + 1;
                    spriteBatch.DrawString(cursorFont, "|", cursorOffset - scrollOffset, Color.Black);
                }
            }

            spriteBatch.End();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // TODO: Add your drawing code here

            Render(FormCollection.Graphics.GraphicsDevice);

            if (!multiline)
                DrawMonoline(spriteBatch);
            else
                DrawMultiline(spriteBatch);

            //Adjust render source rectangle depending on scrollbars visibility
            if (vscrollbar != null && vscrollbar.Max > 0)
                sRect.Width = (int)Width - 14;
            else
                sRect.Width = (int)Width - 2;

            if (hscrollbar != null && hscrollbar.Max > 0)
                sRect.Height = (int)Height - 14;
            else
                sRect.Height = (int)Height - 2;

            dRect.X = (int)Position.X;
            dRect.Y = (int)Position.Y;
            dRect.Width = sRect.Width;
            dRect.Height = sRect.Height;

            spriteBatch.Draw(capturedTexture, dRect, sRect, Color.White);

            if (vscrollbar != null && vscrollbar.Max > 0)
                vscrollbar.Draw(spriteBatch);
            if (hscrollbar != null && hscrollbar.Max > 0)
                hscrollbar.Draw(spriteBatch);
        }

        private void DrawMonoline(SpriteBatch spriteBatch)
        {
            destRect[0].X = (int)Position.X;
            destRect[0].Y = (int)Position.Y;
            spriteBatch.Draw(texture, destRect[0], srcRect[0], BackColor);

            destRect[1].X = destRect[0].X + destRect[0].Width;
            destRect[1].Y = destRect[0].Y;
            spriteBatch.Draw(texture, destRect[1], srcRect[1], BackColor);

            destRect[2].X = destRect[1].X + destRect[1].Width;
            destRect[2].Y = destRect[0].Y;
            spriteBatch.Draw(texture, destRect[2], srcRect[2], BackColor);
        }

        private void DrawMultiline(SpriteBatch spriteBatch)
        {
            destRect[0].X = (int)Position.X;
            destRect[0].Y = (int)Position.Y;
            spriteBatch.Draw(texture, destRect[0], srcRect[0], BackColor);
            destRect[1].X = destRect[0].X + destRect[0].Width;
            destRect[1].Y = destRect[0].Y;
            spriteBatch.Draw(texture, destRect[1], srcRect[1], BackColor);
            destRect[2].X = destRect[1].X + destRect[1].Width;
            destRect[2].Y = destRect[0].Y;
            spriteBatch.Draw(texture, destRect[2], srcRect[2], BackColor);

            destRect[3].X = destRect[0].X;
            destRect[3].Y = destRect[0].Y + destRect[0].Height;
            spriteBatch.Draw(texture, destRect[3], srcRect[3], BackColor);
            destRect[4].X = destRect[1].X;
            destRect[4].Y = destRect[0].Y + destRect[0].Height;
            spriteBatch.Draw(texture, destRect[4], srcRect[4], BackColor);
            destRect[5].X = destRect[2].X;
            destRect[5].Y = destRect[0].Y + destRect[0].Height;
            spriteBatch.Draw(texture, destRect[5], srcRect[5], BackColor);

            destRect[6].X = destRect[0].X;
            destRect[6].Y = destRect[3].Y + destRect[3].Height;
            spriteBatch.Draw(texture, destRect[6], srcRect[6], BackColor);
            destRect[7].X = destRect[1].X;
            destRect[7].Y = destRect[4].Y + destRect[4].Height;
            spriteBatch.Draw(texture, destRect[7], srcRect[7], BackColor);
            destRect[8].X = destRect[2].X;
            destRect[8].Y = destRect[5].Y + destRect[5].Height;
            spriteBatch.Draw(texture, destRect[8], srcRect[8], BackColor);
        }
    }
}
