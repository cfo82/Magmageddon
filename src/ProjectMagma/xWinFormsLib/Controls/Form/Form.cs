/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace xWinFormsLib
{
    public class Form: Control, iForm
    {
        string title;
        Vector2 titleOffset = new Vector2(12f, 2f);

        SpriteBatch spriteBatch;
        string fontName = "kootenay9";
                
        Vector2 minimumSize = new Vector2(100f, 40f);
        Vector2 minimizedSize = new Vector2(100f, 20f);
        Vector2 maximumSize = Vector2.Zero;
        bool isMinimized = false;
        bool isMaximized = false;
        
        Vector2 previousPosition = Vector2.Zero;
        Vector2 previousSize = Vector2.Zero;
        Vector2 minimizedPos = Vector2.Zero;

        public EventHandler OnResized;

        Border border;
        string borderName = "default";
        float borderAlpha;

        ControlCollection controls;

        Color buttonColor = Color.Silver;
        Button btClose, btMinimize, btMaximize, btRestore;
        public Button CloseButton { get { return btClose; } set { btClose = value; } }
        public Button MinimizeButton { get { return btMinimize; } set { btMinimize = value; } }
        public Button MaximizeButton { get { return btMaximize; } set { btMaximize = value; } }
        public Button RestoreButton { get { return btRestore; } set { btRestore = value; } }

        bool hasMinimizeButton = true;
        bool hasMaximizeButton = true;

        Rectangle dragArea = Rectangle.Empty;
        Rectangle resizeArea = new Rectangle(0, 0, 15, 15);
        Texture2D pixelTex;
        bool isDragging = false;
        bool isResizing = false;
        public bool IsResized { get { return isResizing; } }
        bool isMinimizing = false;
        bool isInResizeArea = false;
        Vector2 posOffset = Vector2.Zero;
        Vector2 oldSize = Vector2.Zero;

        bool initialized = false;

        double lastClickTime = 0;

        Menu menu;
        Vector2 menuOffset = new Vector2(0f, 20f);

        /// <summary>
        /// Form Border Style (None = will remove the title, buttons and dragArea)
        /// </summary>
        BorderStyle style = BorderStyle.Sizable;
        /// <summary>
        /// Form Border Style (None = will remove the title, buttons and dragArea)
        /// </summary>
        public enum BorderStyle
        {
            None,
            Fixed,
            Sizable
        }

        WindowState state = WindowState.Normal;
        public enum WindowState
        {
            Normal,
            Minimized,
            Maximized
        }

        public string Title { get { return title; } set { title = value; } }
        public SpriteBatch Spritebatch { get { return spriteBatch; } set { spriteBatch = value; } }
        public ControlCollection Controls { get { return controls; } set { controls = value; } }
        public Control ActiveControl { get { return controls.ActiveControl; } set { controls.ActiveControl = value; } }
        public Vector2 MinimumSize { get { return minimumSize; } set { minimumSize = value; } }
        public Vector2 MinimizedSize { get { return minimizedSize; } set { minimizedSize = value; } }
        public bool HasMinimizeButton { get { return hasMinimizeButton; } set { hasMinimizeButton = value; } }
        public bool HasMaximizeButton { get { return hasMaximizeButton; } set { hasMaximizeButton = value; } }
        public BorderStyle Style { get { return style; } set { style = value; } }
        public WindowState State { get { return state; } set { state = value; } }
        //public bool IsInUse { get { return isInUse; } set { isInUse = value; } }
        public Menu Menu { get { return menu; } set { menu = value; } }
        public bool IsDragged { get { return isDragging; } }
        public bool IsMinimizing { get { return isMinimizing; } }
        public bool IsMinimized { get { return isMinimized; } }
        
        ContentManager content;

        ResolveTexture2D resolveTexture;
        Rectangle sourceRect = Rectangle.Empty;

        /// <summary>
        /// Form
        /// </summary>
        /// <param name="name">Name</param>
        public Form(string name)
            : base(name)
        {
            this.Size = new Vector2(200f, 100f);
            this.Visible = false;
            this.controls = new ControlCollection(this);
        }
        /// <summary>
        /// Form
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="style">Border Style</param>
        public Form(string name, BorderStyle style)
            : base(name)
        {
            this.Size = new Vector2(200f, 100f);
            this.Visible = false;
            this.style = style;
            this.controls = new ControlCollection(this);
        }
        /// <summary>
        /// Form
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="title">Title</param>
        /// <param name="size">Size</param>
        /// <param name="style">Border Style</param>
        public Form(string name, string title, Vector2 size, BorderStyle style)
            : base(name)
        {
            this.Title = title;
            this.Size = size;
            this.Visible = false;
            this.style = style;
            this.controls = new ControlCollection(this);
        }
        /// <summary>
        /// Form
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="title">Title</param>
        /// <param name="size">Size</param>
        /// <param name="position">Position</param>
        /// <param name="style">Border Style</param>
        public Form(string name, string title, Vector2 size, Vector2 position, BorderStyle style)
            : base(name, position)
        {
            this.Title = title;
            this.Size = size;
            this.Visible = false;
            this.style = style;
            this.controls = new ControlCollection(this);
        }
        
        /// <summary>
        /// Returns a control by index
        /// </summary>
        public Control this[int index]
        {
            get { return controls[index]; }
            set { controls[index] = value; }
        }
        /// <summary>
        /// Returns a control by name
        /// </summary>
        public Control this[string name]
        {
            get
            {
                for (int i = 0; i < controls.Count; i++)
                    if (controls[i].Name == name)
                        return controls[i];

                return null;
            }
            set
            {
                for (int i = 0; i < controls.Count; i++)
                    if (controls[i].Name == name)
                    {
                        controls[i] = value;
                        break;
                    }
            }
        }

        public void Remove(Control control)
        {
            controls.Remove(control);
        }
        public void Remove(string name)
        {
            for (int i = 0; i < controls.Count; i++)
                if (controls[i].Name == name)
                {
                    controls.RemoveAt(i);
                    break;
                }
        }

        virtual public void Show()
        {
            if (!initialized)
                Initialize();

            state = WindowState.Normal;

            if (previousPosition != Vector2.Zero)
                Position = previousPosition;
            if (previousSize != Vector2.Zero)
                Size = previousSize;

            Visible = true;
            Focus();
        }
        virtual public void Hide()
        {
            Visible = false;
        }
        virtual public void Close()
        {
            if (menu != null)
                menu.Close();

            if (FormCollection.TopMostForm == this)
                FormCollection.TopMostForm = null;

            Dispose();
        }
        virtual public void Focus()
        {
            if (FormCollection.TopMostForm != null &&
                FormCollection.TopMostForm != this && 
                FormCollection.TopMostForm.Menu != null && 
                FormCollection.TopMostForm.Menu.State == Menu.MenuState.Opened)
                    return;

            //if (FormCollection.Menu != null)
            //    FormCollection.Menu.Close();

            FormCollection.Forms.Remove(this);
            FormCollection.Forms.Insert(0, this);
            FormCollection.TopMostForm = this;
        }
        virtual public void Unfocus()
        {
            if (menu != null && menu.State != Menu.MenuState.Closed)
                menu.Close();
        }
        virtual public void Minimize()
        {            
            Nullable<Vector2> minPos = FormCollection.GetMinimizedPosition(this);
            if (minPos.HasValue)
                minimizedPos = minPos.Value;
            else
                return;

            if (state == WindowState.Normal)
            {
                previousPosition = Position;
                previousSize = Size;
            }

            if (menu != null)
            {
                menu.Close();
                menu.Visible = false;
            }

            state = WindowState.Minimized;
            isMinimizing = true;
        }
        virtual public void Maximize()
        {
            if (state == WindowState.Normal)
            {
                previousPosition = Position;
                previousSize = Size;
            }

            if (menu != null)
                menu.Close();

            Focus();
            maximumSize = FormCollection.GetMaximizedSize(this);
            state = WindowState.Maximized;
        }
        virtual public void Restore()
        {
            Focus();

            if (menu != null)
            {
                menu.Visible = true;
                menu.Close();
            }

            state = WindowState.Normal;
        }

        /// <summary>
        /// Initialize the form and its controls
        /// </summary>
        /// <param name="services"></param>
        /// <param name="graphics"></param>
        public void Initialize()
        {
            content = new ContentManager(FormCollection.Services);

            spriteBatch = new SpriteBatch(FormCollection.Graphics.GraphicsDevice);

            if (File.Exists(@"content\fonts\" + fontName + ".xnb"))
                Font = content.Load<SpriteFont>(@"content\fonts\" + fontName);
            else
                throw new Exception("SpriteFont Not Found. " + @"content\fonts\" + fontName);

            maximumSize = new Vector2(FormCollection.Graphics.GraphicsDevice.Viewport.Width,
                FormCollection.Graphics.GraphicsDevice.Viewport.Height);

            if (menu != null)
            {
                minimumSize.Y = 60;
                menu.Owner = this;
                menu.Position = menuOffset;
                menu.Initialize(content, FormCollection.Graphics.GraphicsDevice);
            }

            border = new Border(borderName, BackColor, style == BorderStyle.Sizable);
            border.Initialize(content, FormCollection.Graphics.GraphicsDevice);

            InitializeButtons();

            for (int i = 0; i < controls.Count; i++)
            {
                #region Initalize the control's font first
                if (controls[i].FontName == "" || controls[i].FontName == null)
                    controls[i].FontName = fontName;    
                controls[i].Font = content.Load<SpriteFont>(@"content\fonts\" + controls[i].FontName);
                #endregion

                controls[i].Initialize(content, FormCollection.Graphics.GraphicsDevice);
            }

            resolveTexture = new ResolveTexture2D(FormCollection.Graphics.GraphicsDevice,
                FormCollection.Graphics.GraphicsDevice.Viewport.Width,
                FormCollection.Graphics.GraphicsDevice.Viewport.Height,
                1,
                FormCollection.Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            pixelTex = new Texture2D(FormCollection.Graphics.GraphicsDevice, 1, 1);
            pixelTex.SetData<Color>(new Color[] { Color.White });

            base.Initialize(content, FormCollection.Graphics.GraphicsDevice);

            initialized = true;
        }

        private void InitializeButtons()
        {
            btClose = new Button("btMinimize", new Vector2(Size.X - 22f, 4f),
                @"content\textures\controls\button\close.png", 1f, buttonColor);

            btClose.Owner = this;            
            btClose.Initialize(content, FormCollection.Graphics.GraphicsDevice);
            btClose.OnRelease += FormClose;

            if (hasMinimizeButton)
            {
                btMinimize = new Button("btMinimize", Vector2.Zero,
                    @"content\textures\controls\button\minimize.png", 1f, buttonColor);

                btMinimize.Owner = this;
                btMinimize.Initialize(content, FormCollection.Graphics.GraphicsDevice);
                btMinimize.OnRelease += FormMinimize;

            }

            if (hasMaximizeButton)
            {
                btMaximize = new Button("btMaximize", Vector2.Zero,
                    @"content\textures\controls\button\maximize.png", 1f, buttonColor);

                btMaximize.Owner = this;
                btMaximize.Initialize(content, FormCollection.Graphics.GraphicsDevice);
                btMaximize.OnRelease += FormMaximize;
            }

            if (hasMinimizeButton || hasMaximizeButton)
            {
                btRestore = new Button("btRestore", Vector2.Zero,
                    @"content\textures\controls\button\restore.png", 1f, buttonColor);

                btRestore.Owner = this;
                btRestore.Initialize(content, FormCollection.Graphics.GraphicsDevice);
                btRestore.OnRelease += FormRestore;
            }
        }

        /// <summary>
        /// Dispose of the form and its controls
        /// </summary>
        public override void Dispose()
        {
            for (int i = 0; i < controls.Count; i++)
                controls[i].Dispose();

            controls.Clear();

            if (btClose != null)
                btClose.Dispose();
            if (btMinimize != null)
                btMinimize.Dispose();
            if (btMaximize != null)
                btMaximize.Dispose();
            if (btRestore != null)
                btRestore.Dispose();

            if (menu != null)
                menu.Dispose();

            if (border != null)
                border.Dispose();

            if (spriteBatch != null)
                spriteBatch.Dispose();
            if (content != null)
                content.Dispose();

            if (FormCollection.TopMostForm == this)
                FormCollection.TopMostForm = FormCollection.Forms[0];

            base.Dispose();
        }

        private void FormClose(object sender, EventArgs e)
        {
            Close();
        }
        private void FormMinimize(object sender, EventArgs e)
        {
            Minimize();
        }
        private void FormMaximize(object sender, EventArgs e)
        {
            Maximize();
        }
        private void FormRestore(object sender, EventArgs e)
        {
            Restore();
        }

        /// <summary>
        /// Update every enabled control
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            area.X = (int)Position.X;
            area.Y = (int)Position.Y;
            area.Width = (int)Size.X;
            area.Height = (int)Size.Y;

            UpdateState();
            UpdateTopMost();

            if (state == WindowState.Normal && !isMaximized && !isMinimized)
            {
                if (!isResizing)
                    CheckDragging();
                if (!isDragging && style == BorderStyle.Sizable)
                    CheckResize();
            }

            CheckDoubleClick(gameTime);

            if (state != WindowState.Minimized && !isMinimized)
            {                
                if (Size.X < minimumSize.X)
                    Width = minimumSize.X;
                if (Size.Y < minimumSize.Y)
                    Height = minimumSize.Y;
            }

            if (FormCollection.TopMostForm == this || (isMinimized && FormCollection.ActiveForm == this))
            {
                if (menu != null && menu.Visible)
                    menu.Update(gameTime);

                if (btClose != null)
                    btClose.Update(gameTime);
                
                if (btMinimize != null && ((hasMinimizeButton && !isMinimized) || (hasMaximizeButton && isMaximized)))
                    btMinimize.Update(gameTime);
                
                if (btMaximize != null && ((hasMaximizeButton && !isMaximized) || (hasMinimizeButton && isMinimized)))
                    btMaximize.Update(gameTime);
                
                if (btRestore != null && ((hasMinimizeButton && isMinimized) || (hasMaximizeButton && isMaximized)))
                    btRestore.Update(gameTime);

                if (menu == null || !menu.Visible || menu.State == Menu.MenuState.Closed)
                {
                    if (state == WindowState.Normal || state == WindowState.Maximized)
                    {
                        //ComboBox fix (so the controls underneath the listbox don't update)
                        bool skip = false;
                        for (int i = 0; i < controls.Count; i++)
                        {
                            if (controls[i].GetType() == typeof(ComboBox) && ((ComboBox)controls[i]).Opened)
                            {
                                controls[i].Update(gameTime);
                                skip = true;
                            }
                        }


                        if (!skip)
                        {
                            for (int i = 0; i < controls.Count; i++)
                                if (!controls[i].IsDisposed && controls[i].Enabled) // && area.Contains(controls[i].area))
                                    controls[i].Update(gameTime);
                        }
                    }                    
                }
            }

            UpdateActive();

            if (FormCollection.ActiveForm != this)
            {
                if (isResizing) isResizing = false;
                if (isDragging) isDragging = false;
            }
        }

        private void UpdateTopMost()
        {
            if (MouseHelper.IsPressed && area.Contains(MouseHelper.Cursor.Location) && FormCollection.ActiveForm == null)
            {
                FormCollection.ActiveForm = this;
                Focus();
            }
        }

        private void UpdateActive()
        {
            if (FormCollection.ActiveForm == this && MouseHelper.IsReleased)                
                FormCollection.ActiveForm = null;
        }

        private void UpdateState()
        {
            //Restore the window to its original size and position
            if (state == WindowState.Normal && (isMaximized || isMinimized))
            {
                if (Vector2.Distance(Position, previousPosition) > 2f)
                    Position += ((previousPosition - Position) * 0.2f);
                else
                    Position = previousPosition;

                if (Vector2.Distance(Size, previousSize) > 2f)
                    Size += ((previousSize - Size) * 0.2f);
                else
                    Size = previousSize;

                if (Position == previousPosition && Size == previousSize)
                {
                    isMaximized = false;
                    isMinimized = false;
                    //isRestoring = false;
                    Focus();
                    if (OnResized != null)
                        OnResized(null, null);
                }
            }
            //Minimize the window
            else if (state == WindowState.Minimized && !isMinimized)
            {
                if (Vector2.Distance(minimizedPos, Position) > 2f)
                    Position += ((minimizedPos - Position) * 0.2f);
                else
                    Position = minimizedPos;

                if (Vector2.Distance(Size, minimizedSize) > 2f)
                    Size += ((minimizedSize - Size) * 0.2f);
                else
                    Size = minimizedSize;

                if (Position == minimizedPos && Size == minimizedSize)
                {
                    isMinimized = true;
                    isMaximized = false;
                    isMinimizing = false;
                    if (OnResized != null)
                        OnResized(null, null);
                }
            }
            //Maximize the window
            else if (state == WindowState.Maximized && !isMaximized)
            {
                if (Vector2.Distance(Vector2.Zero, Position) > 2f)
                    Position += ((Vector2.Zero - Position) * 0.2f);
                else
                    Position = Vector2.Zero;

                if (Vector2.Distance(Size, maximumSize) > 2f)
                    Size += ((maximumSize - Size) * 0.2f);
                else
                    Size = maximumSize;

                if (Position == Vector2.Zero && Size == maximumSize)
                {
                    isMaximized = true;
                    isMinimized = false;
                    //isMaximizing = false;
                    if (OnResized != null)
                        OnResized(null, null);
                }
            }
        }

        private void CheckDragging()
        {
            dragArea.X = (int)Position.X + 7;
            dragArea.Y = (int)Position.Y;
            dragArea.Width = (int)Size.X - 29;
            if (hasMinimizeButton)
                dragArea.Width -= 15;
            if (hasMaximizeButton)
                dragArea.Width -= 15;
            dragArea.Height = 20;

            if (dragArea.Contains(MouseHelper.Cursor.Location) &&
                MouseHelper.HasBeenPressed && FormCollection.ActiveForm == this)
            {
                isDragging = true;
                Focus();
                posOffset.X = MouseHelper.Cursor.X - Position.X;
                posOffset.Y = MouseHelper.Cursor.Y - Position.Y;
            }

            if (isDragging)
            {
                Left = MouseHelper.Cursor.X - posOffset.X;
                Top = MouseHelper.Cursor.Y - posOffset.Y;

                if (MouseHelper.IsReleased || !FormCollection.Window.Focused)
                    isDragging = false;                
            }

            if (FormCollection.Snapping)
            {
                if (Position.X > -FormCollection.SnapSize.X && Position.X < FormCollection.SnapSize.X)
                    Left = 0f;
                else if (Position.X > (FormCollection.Graphics.GraphicsDevice.Viewport.Width - Size.X) - FormCollection.SnapSize.X && Position.X < (FormCollection.Graphics.GraphicsDevice.Viewport.Width - Size.X) + FormCollection.SnapSize.X)
                    Left = FormCollection.Graphics.GraphicsDevice.Viewport.Width - Size.X;
                    
                if (Position.Y > (FormCollection.Graphics.GraphicsDevice.Viewport.Height - Size.Y) - FormCollection.SnapSize.Y && Position.Y < (FormCollection.Graphics.GraphicsDevice.Viewport.Height - Size.Y) + FormCollection.SnapSize.Y)
                    Top = FormCollection.Graphics.GraphicsDevice.Viewport.Height - Size.Y;
            }
        }

        private void CheckDoubleClick(GameTime gameTime)
        {
            if (state != WindowState.Normal)
            {
                dragArea.X = (int)Position.X + 7;
                dragArea.Y = (int)Position.Y;
                dragArea.Width = (int)Size.X - 29;
                if (hasMinimizeButton)
                    dragArea.Width -= 15;
                if (hasMaximizeButton)
                    dragArea.Width -= 15;
                dragArea.Height = 20;
            }

            if (style == BorderStyle.Sizable && dragArea.Contains(MouseHelper.Cursor.Location))
            {
                if (MouseHelper.HasBeenPressed)
                {
                    if (FormCollection.ActiveForm == null)
                        FormCollection.ActiveForm = this;

                    if (FormCollection.ActiveForm == this)
                    {
                        if (gameTime.TotalGameTime.TotalMilliseconds - lastClickTime < 200)
                            ToggleWindowState();

                        lastClickTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
            }
        }

        private void CheckResize()
        {
            resizeArea.X = (int)(Position.X + Size.X) - resizeArea.Width;
            resizeArea.Y = (int)(Position.Y + Size.Y) - resizeArea.Height;

            //FIX ME! (not topMostForm only, if dragArea is in other top form area, dont show the resize cursor)
            if (resizeArea.Contains(MouseHelper.Cursor.Location) && FormCollection.TopMostForm == this)
            {
                if (!isInResizeArea)
                {
                    isInResizeArea = true;
                    FormCollection.Cursor.Type = MouseCursor.CursorType.Resize;
                }

                if (MouseHelper.HasBeenPressed &&
                    FormCollection.ActiveForm == this)
                {
                    isResizing = true;
                    Focus();
                    posOffset.X = MouseHelper.Cursor.X;
                    posOffset.Y = MouseHelper.Cursor.Y;
                    oldSize = Size;
                }
            }
            else if (isInResizeArea && !isResizing)
            {
                isInResizeArea = false;
                FormCollection.Cursor.Type = MouseCursor.CursorType.Default;
            }

            if (isResizing)
            {
                Width = (int)(oldSize.X + (MouseHelper.Cursor.X - posOffset.X));
                Height = (int)(oldSize.Y + (MouseHelper.Cursor.Y - posOffset.Y));

                if (OnResized != null)
                    OnResized(null, null);

                if (MouseHelper.IsReleased)
                    isResizing = false;
            }
        }

        private void ToggleWindowState()
        {
            if (state == WindowState.Normal)
                Maximize();
            else if (state == WindowState.Maximized)
                Restore();
            else if (state == WindowState.Minimized)
                Restore();
        }

        /// <summary>
        /// Render the form controls
        /// </summary>
        public void Render()
        {
            if (resolveTexture.Width != FormCollection.Graphics.GraphicsDevice.Viewport.Width ||
                resolveTexture.Height != FormCollection.Graphics.GraphicsDevice.Viewport.Height)
            {
                resolveTexture = new ResolveTexture2D(FormCollection.Graphics.GraphicsDevice,
                FormCollection.Graphics.GraphicsDevice.Viewport.Width,
                FormCollection.Graphics.GraphicsDevice.Viewport.Height,
                1,
                FormCollection.Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);
            }

            FormCollection.Graphics.GraphicsDevice.SetRenderTarget(0, null);
            FormCollection.Graphics.GraphicsDevice.Clear(Color.TransparentBlack);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            int overlay = 0;
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].GetType() == typeof(ComboBox))
                    overlay = i;
                if (!controls[i].IsDisposed && controls[i].Enabled)
                    controls[i].Draw(spriteBatch);
            }

            if (overlay != 0)
            {
                ComboBox combobox = ((ComboBox)controls[overlay]);
                if(combobox.Opened)
                    combobox.DrawOverlay(spriteBatch);
            }


            spriteBatch.End();

            FormCollection.Graphics.GraphicsDevice.ResolveBackBuffer(resolveTexture, 0);
            FormCollection.Graphics.GraphicsDevice.Clear(Color.Black);

            if (menu != null && !menu.IsDisposed && menu.Visible)
                menu.Render();
        }

        /// <summary>
        /// Draw every visible control
        /// </summary>
        public void Draw()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            //Draw Form Background
            borderAlpha = 0.3f - FormCollection.Forms.IndexOf(this) * 0.04f;
            border.Draw(spriteBatch, Position, Size, borderAlpha);

            //Draw TitleBar
            DrawTitle(spriteBatch);
            DrawButtons(spriteBatch);

            //Draw Form Controls
            if (resolveTexture != null && !resolveTexture.IsDisposed)
            {
                sourceRect.Width = (int)Size.X - 5;
                sourceRect.Height = (int)Size.Y - 5;
                spriteBatch.Draw(resolveTexture, Position, sourceRect, Color.White);
            }

            spriteBatch.End();

            if (menu != null && !menu.IsDisposed && menu.Visible)
                menu.Draw();
        }

        private void DrawTitle(SpriteBatch spriteBatch)
        {
            //Draw Title
            if (Font.MeasureString(title).X >= Size.X - 75)
            {
                for (int i = 0; i < title.Length + 1; i++)
                    if (Font.MeasureString(title.Substring(0, i)).X >= Size.X - 75)
                    {
                        spriteBatch.DrawString(Font, title.Substring(0, i - 1) + "..", Position + titleOffset, ForeColor);
                        break;
                    }                
            }
            else
                spriteBatch.DrawString(Font, title, Position + titleOffset, ForeColor);
        }

        private void DrawButtons(SpriteBatch spriteBatch)
        {
            //Draw Buttons
            if (style != BorderStyle.None)
            {
                btClose.Left = Size.X - 22;
                btClose.Top = 4;
                btClose.Draw(spriteBatch, Position);
            }

            if (this.style == BorderStyle.Sizable)
            {
                if (hasMaximizeButton)
                {
                    if (state != WindowState.Maximized)
                    {
                        btMaximize.Left = Size.X - 37;
                        btMaximize.Top = 4;
                        btMaximize.Draw(spriteBatch, Position);
                    }
                    else
                    {
                        btRestore.Left = Size.X - 37;
                        btRestore.Top = 4;
                        btRestore.Draw(spriteBatch, Position);
                    }

                    if (hasMinimizeButton)
                    {
                        if (state != WindowState.Minimized)
                        {
                            btMinimize.Left = Size.X - 52;
                            btMinimize.Top = 4;
                            btMinimize.Draw(spriteBatch, Position);
                        }
                        else
                        {
                            btRestore.Left = Size.X - 52;
                            btRestore.Top = 4;
                            btRestore.Draw(spriteBatch, Position);
                        }
                    }
                }
                else if (hasMinimizeButton)
                {
                    if (state != WindowState.Minimized)
                    {
                        btMinimize.Left = Size.X - 37;
                        btMinimize.Top = 4;
                        btMinimize.Draw(spriteBatch, Position);
                    }
                    else
                    {
                        btRestore.Left = Size.X - 37;
                        btRestore.Top = 4;
                        btRestore.Draw(spriteBatch, Position);
                    }
                }
            }
        }
    }
}
