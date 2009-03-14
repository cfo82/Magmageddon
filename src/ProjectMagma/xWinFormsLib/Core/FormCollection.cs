/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using WinForms = System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace xWinFormsLib
{
    public class FormCollection
    {
        static IServiceProvider services;
        static GraphicsDeviceManager graphics;

        static private List<Form> forms = new List<Form>();
        static private Form activeForm = null;
        static private Form topMostForm = null;
        //static bool hasMaximizedForm = false;

        //static private Menu menu = null;
        static private SubMenu contextMenu = null;
        static private ContentManager contentManager;
        static private SpriteBatch spriteBatch;

        static private MouseCursor cursor;
        static bool isCursorVisible = true;

        static Label tooltip = new Label("tooltip", Vector2.Zero, "", Color.Beige, Color.Black, 200, Label.Align.Left);

        static bool snapping = true;
        static Vector2 snapSize = new Vector2(10f, 10f);

        static public IServiceProvider Services { get { return services; } }
        static public GraphicsDeviceManager Graphics { get { return graphics; } }
        static public ContentManager ContentManager { get { return contentManager; } }
        static public List<Form> Forms { get { return forms; } set { forms = value; } }
        static public Form ActiveForm { get { return activeForm; } set { activeForm = value; } }
        static public Form TopMostForm { get { return topMostForm; } set { topMostForm = value; } }
        static public MouseCursor Cursor { get { return cursor; } set { cursor = value; } }
        static public bool IsCursorVisible { get { return isCursorVisible; } set { isCursorVisible = value; } }
        static public Label Tooltip { get { return tooltip; } set { tooltip = value; } }
        //static public Menu Menu { get { return menu; } set { menu = value; } }
        static public bool Snapping { get { return snapping; } set { snapping = value; } }
        static public Vector2 SnapSize { get { return snapSize; } set { snapSize = value; } }

        private static WinForms.Form window;
        public static WinForms.Form Window { get { return window; } set { window = value; } }

        public static int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < forms.Count; i++)
                    if (!forms[i].IsDisposed && forms[i].Visible)
                        count++;
                return count;
            }
        }

        public FormCollection(GameWindow window, IServiceProvider services, ref GraphicsDeviceManager graphics)
        {
            FormCollection.window = (WinForms.Form)WinForms.Form.FromHandle(window.Handle);
            FormCollection.window.SizeChanged += Form_SizeChanged;
            
            FormCollection.services = services;
            FormCollection.graphics = graphics;

            contentManager = new ContentManager(services);
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            cursor = new MouseCursor(false, true);

            tooltip.Font = contentManager.Load<SpriteFont>(@"content\fonts\pescadero9");
            tooltip.Initialize(contentManager, graphics.GraphicsDevice);
            tooltip.Visible = false;

            //contextMenu = new SubMenu(null);
            //contextMenu.Add(new MenuItem("mnuRestore", "Restore", null), null);
            //contextMenu.Add(new MenuItem("mnuMinimize", "Minimize", null), null);
            //contextMenu.Add(new MenuItem("mnuMaximize", "Maximize", null), null);
            //contextMenu.Add(new MenuItem("", "-", null), null);
            //contextMenu.Add(new MenuItem("mnuRestore", "Close", null), null);
        }

        /// <summary>
        /// Returns a form by index
        /// </summary>
        /// <param name="index">List index</param>
        /// <returns></returns>
        public Form this[int index]
        {
            get { return forms[index]; }
            set { forms[index] = value; }
        }
        /// <summary>
        /// Returns a form by name
        /// </summary>
        /// <param name="name">Form name</param>
        /// <returns></returns>
        public Form this[string name]
        {
            get
            {
                for (int i = 0; i < forms.Count; i++)
                    if (forms[i].Name == name)
                        return forms[i];

                return null;
            }
            set
            {
                for (int i = 0; i < forms.Count; i++)
                    if (forms[i].Name == name)
                    {
                        forms[i] = value;
                        break;
                    }
            }
        }
        static public Form Form(string name)
        {
            for (int i = 0; i < forms.Count; i++)
                if (forms[i].Name == name)
                    return forms[i];

            return null;
        }

        /// <summary>
        /// Add a form to the collection
        /// </summary>
        /// <param name="form">Form</param>
        public void Add(Form form)
        {            
            forms.Insert(0, form);
            //topMostForm = form;
            //form.Focus();
            //form.Update(null);
        }
        /// <summary>
        /// Remove a form from the collection
        /// </summary>
        /// <param name="form">Form</param>
        public void Remove(Form form)
        {
            form.Dispose();
            forms.Remove(form);
        }
        /// <summary>
        /// Remove a form by name
        /// </summary>
        /// <param name="name">Form name</param>
        public void Remove(string name)
        {
            for (int i = 0; i < forms.Count; i++)
                if (forms[i].Name == name)
                {
                    forms[i].Dispose();
                    forms.RemoveAt(i);
                    break;
                }

        }

        /// <summary>
        /// Dispose of the form collection
        /// </summary>
        public void Dispose()
        {
            for (int i = forms.Count - 1; i > -1; i--)
                forms[i].Dispose();

            forms.Clear();

            //if (menu != null)
            //    menu.Dispose();

            tooltip.Dispose();
            cursor.Dispose();
            contentManager.Dispose();                        
        }

        /// <summary>
        /// Update enabled forms
        /// </summary>
        public void Update(GameTime gameTime)
        {
            //Update MouseHelper
            MouseHelper.Update();

            //Update Cursor
            cursor.Update(gameTime);

            //Update active or topMost form
            if (activeForm != null)
                activeForm.Update(gameTime);
            else if (topMostForm != null)
                topMostForm.Update(gameTime);
            else if (topMostForm == null && forms.Count > 0)
            {
                for (int i = 0; i < forms.Count; i++)
                    if (!forms[i].IsDisposed)
                    {
                        topMostForm = forms[i];
                        topMostForm.Focus();
                        break;
                    }
            }

            //Update other forms
            for (int i = 0; i < forms.Count; i++)
                if (!forms[i].IsDisposed && forms[i].Enabled && 
                    forms[i] != activeForm && forms[i] != topMostForm)
                    forms[i].Update(gameTime);

            //Update Top Menu
            //if (activeForm == null && menu != null && menu.Visible)
            //    if (topMostForm == null || topMostForm.State != xWinFormsLib.Form.WindowState.Maximized)
            //        menu.Update(gameTime);

            //Update Context Menu
            if (contextMenu != null && contextMenu.State != SubMenu.MenuState.Closed && contextMenu.Visible)
                contextMenu.Update(gameTime);
        }

        /// <summary>
        /// Render forms content
        /// </summary>
        public void Render()
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            //if (menu != null && !menu.IsDisposed && menu.Visible && !hasMaximizedForm)
            //    menu.Render();

            //if (contextMenu != null && !contextMenu.IsDisposed && contextMenu.Visible)
            //    contextMenu.Render();

            for (int i = 0; i < forms.Count; i++)
                if (!forms[i].IsDisposed && forms[i].Visible)
                    forms[i].Render();
        }

        /// <summary>
        /// Draw visible forms
        /// </summary>
        public void Draw()
        {
            #region Draw TopMenu

            //if (menu != null && !menu.IsDisposed && menu.Visible)
            //    menu.Draw();

            #endregion

            #region Draw Forms

            for (int i = forms.Count - 1; i > -1; i--)
                if (!forms[i].IsDisposed && forms[i].Visible && forms[i] != topMostForm &&
                    forms[i].State != xWinFormsLib.Form.WindowState.Minimized)
                    forms[i].Draw();

            if (topMostForm != null && !topMostForm.IsDisposed && topMostForm.Visible)
                topMostForm.Draw();

            #endregion

            #region Draw Minimized Forms
            for (int i = forms.Count - 1; i >= 0; i--)
                if (forms[i].State == xWinFormsLib.Form.WindowState.Minimized && 
                    !forms[i].IsDisposed && forms[i].Visible && forms[i] != topMostForm)
                    forms[i].Draw();
            #endregion

            #region Draw Context Menu
            if (contextMenu != null && !contextMenu.IsDisposed && contextMenu.Visible && contextMenu.State != SubMenu.MenuState.Closed)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                contextMenu.Draw(spriteBatch);
                spriteBatch.End();
            }
            #endregion

            #region Draw Cursor
            if (isCursorVisible)
                cursor.Draw();
            #endregion
        }

        public static Nullable<Vector2> GetMinimizedPosition(Form form)
        {
            for (int i = 0; i < forms.Count; i++)
                if (forms[i] != form && forms[i].IsMinimizing)
                    return null;

            //using MinimumSize from the Form Class (100 by 40)
            for (int y = graphics.GraphicsDevice.Viewport.Height - 20; y > 0; y -= 20)
            {
                for (int x = 0; x < graphics.GraphicsDevice.Viewport.Width - 99; x += 100)
                {
                    bool isOccupied = false;
                    for (int i = 0; i < forms.Count; i++)
                    {
                        if (forms[i] != form && !forms[i].IsDisposed && forms[i].Visible &&
                            forms[i].Position.X == x && forms[i].Position.Y == y)
                        {
                            isOccupied = true;
                            break;
                        }
                    }

                    if (!isOccupied)
                        return new Vector2(x, y);
                }
            }

            return Vector2.Zero;
        }

        public static Vector2 GetMaximizedSize(Form form)
        {
            Vector2 maxSize = new Vector2(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            for (int i = 0; i < forms.Count; i++)
                if (forms[i] != form && !forms[i].IsDisposed && forms[i].Visible && forms[i].State == xWinFormsLib.Form.WindowState.Minimized)
                    if (forms[i].Top < maxSize.Y)
                        maxSize.Y = forms[i].Top;

            return maxSize;
        }

        public static void ShowContextMenu()
        {
            contextMenu.Open(MouseHelper.Cursor.Position);
        }

        public static void CloseOpenedMenus()
        {
            //if (menu != null)
            //    for (int i = 0; i < menu.Items.Count; i++)
            //        if (menu.Items[i].SubMenu != null && menu[i].SubMenu.State != SubMenu.MenuState.Closed)
            //            menu.Items[i].SubMenu.Close();

            for (int i = 0; i < forms.Count; i++)
                if (forms[i].Menu != null)
                    for (int j = 0; j < forms[i].Menu.Items.Count; j++)
                        if (forms[i].Menu.Items[j].SubMenu != null && forms[i].Menu.Items[j].SubMenu.State != SubMenu.MenuState.Closed)
                            forms[i].Menu.Items[j].SubMenu.Close();
        }

        public static void FocusNext()
        {
        }

        public static bool IsObstructed(Control control, Point location)
        {
            for (int i = 0; i < forms.Count; i++)
            {
                if (control.Owner != null && forms[i] != control.Owner)
                {
                    if (forms[i].area.Contains(control.area) || forms[i].area.Intersects(control.area))
                    {
                        if (forms[i].area.Contains(location))
                            return true;
                    }
                }
                else if (control.Owner == null)
                {
                    if (forms[i].area.Contains(control.area) || forms[i].area.Intersects(control.area))
                    {
                        if (forms[i].area.Contains(location))
                            return true;
                    }
                }
            }

            return false;
        }

        private void Form_SizeChanged(object obj, EventArgs e)
        {
            Rectangle area = 
                new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            foreach (Form form in forms)
            {
                //If a form is ouf of the working area,
                //we need to put it back where the user can see it.
                if (!area.Contains(form.area))
                {
                    if (form.Position.X + form.Width > graphics.GraphicsDevice.Viewport.Width)
                        form.X = graphics.GraphicsDevice.Viewport.Width - form.Size.X;
                    //else if (form.Position.X + form.Width < 0)
                    //    form.X = 0;
                    
                    if (form.Position.Y + form.Height > graphics.GraphicsDevice.Viewport.Height)
                        form.Y = graphics.GraphicsDevice.Viewport.Height - form.Size.Y;
                    //else if (form.Position.Y + form.Width < 0)
                    //    form.Y = 0;
                }

                //If a form was maximized
                if (form.State == xWinFormsLib.Form.WindowState.Maximized)
                {
                    //resize it
                    form.Width = graphics.GraphicsDevice.Viewport.Width;
                    form.Height = graphics.GraphicsDevice.Viewport.Height;

                    //if the window was previously maximized,
                    //we need to reposition the form.
                    if (window.WindowState == System.Windows.Forms.FormWindowState.Normal)
                    {
                        form.X = 0;
                        form.Y = 0;
                    }
                }
            }
        }
    }
}
