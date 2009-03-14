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
    public class Menu: Control
    {
        SpriteBatch spriteBatch;
        ResolveTexture2D resolveTexture;
        Vector2 drawPos = Vector2.Zero;

        SpriteFont font;
        string fontName = "kootenay9";
        Vector2 itemPos;
        List<MenuItem> items = new List<MenuItem>();
        
        int hoverIndex = -1;
        Vector2 openPos;
        Rectangle itemArea = Rectangle.Empty;
        Texture2D pixelTex;

        MenuState state = MenuState.Closed;
        public enum MenuState
        {
            Opened,
            Closed,
        }

        public List<MenuItem> Items { get { return items; } }
        public MenuState State { get { return state; } }
        public new SpriteFont Font { get { return font; } }

        KeyboardHelper keyboad;
        bool bAltDown = false;
        bool openedMenu = false;
        int indexToOpen = -1;

        public Menu(string name)
            : base(name)
        {
            this.Name = name;
            this.BackColor = Color.White; //FIX ME! (different color creates a minor bug on sub-sub menus)

            keyboad = new KeyboardHelper();
            keyboad.OnKeyPress += Keyboard_OnPress;
            keyboad.OnKeyRelease += Keyboard_OnRelease;
        }

        public MenuItem this[string name]
        {
            get
            {
                for (int i = 0; i < items.Count; i++)
                    if (items[i].Name == name)
                        return items[i];

                return null;
            }

            set
            {
                for (int i = 0; i < items.Count; i++)
                    if (items[i].Name == name)
                        items[i] = value; ;
            }
        }
        public MenuItem this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public void Add(MenuItem item, SubMenu submenu)
        {
            if (submenu != null)
            {
                submenu.Parent = this;
                submenu.BackColor = BackColor;
                submenu.ForeColor = ForeColor;
            }

            if (this.Owner != null)
                submenu.Owner = this.Owner;

            item.SubMenu = submenu;
            
            this.items.Add(item);
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            spriteBatch = new SpriteBatch(graphics);

            if (Owner != null)
            {
                area = new Rectangle(1, 20, (int)Owner.Width - 1, 18);
                fontName = Owner.FontName;
                font = Owner.Font;
            }
            else
            {
                area = new Rectangle(0, 0, graphics.Viewport.Width, 17);
                font = content.Load<SpriteFont>(@"fonts\" + fontName);
            }

            itemArea.Height = font.LineSpacing;

            resolveTexture = new ResolveTexture2D(FormCollection.Graphics.GraphicsDevice,
                FormCollection.Graphics.GraphicsDevice.Viewport.Width,
                FormCollection.Graphics.GraphicsDevice.Viewport.Height,
                1,
                FormCollection.Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            pixelTex = new Texture2D(FormCollection.Graphics.GraphicsDevice, 1, 1);
            pixelTex.SetData<Color>(new Color[] { Color.White });

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null)
                    items[i].SubMenu.Initialize(content, graphics);

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            pixelTex.Dispose();

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null)
                    items[i].SubMenu.Dispose();

            spriteBatch.Dispose();

            base.Dispose();
        }

        private void Keyboard_OnPress(object obj, EventArgs e)
        {
            Keys key = (Keys)obj;
            switch (key)
            {
                case Keys.LeftAlt:
                    bAltDown = true;
                    break;
                case Keys.RightAlt:
                    bAltDown = true;
                    break;
                default:
                    indexToOpen = -1;
                    for (int i = 0; i < items.Count; i++)
                        if (bAltDown && key == items[i].Key)
                            indexToOpen = i;
                    break;
            }
        }

        private void Keyboard_OnRelease(object obj, EventArgs e)
        {
            Keys key = (Keys)obj;
            switch (key)
            {
                case Keys.LeftAlt:
                    bAltDown = false;
                    if (!openedMenu)
                        CloseSubMenus();
                    openedMenu = false;
                    break;
                case Keys.RightAlt:
                    if (!openedMenu)
                        CloseSubMenus();
                    bAltDown = false;
                    openedMenu = false;
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {            
            CheckSelection();

            keyboad.Update();

            if (state == MenuState.Opened && hoverIndex != -1)
                CheckHovering();

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State != SubMenu.MenuState.Closed)
                    items[i].SubMenu.Update(gameTime);

            base.Update(gameTime);
        }

        private void CheckSelection()
        {
            if (MouseHelper.HasBeenPressed)
            {
                if (hoverIndex != -1)
                {
                    CloseSubMenus();

                    if (items[hoverIndex].SubMenu != null && items[hoverIndex].SubMenu.State != SubMenu.MenuState.Closed)
                    {
                        items[hoverIndex].SubMenu.Close();
                        state = MenuState.Closed;
                        return;
                    }

                    if (items[hoverIndex].SubMenu != null && items[hoverIndex].SubMenu.State == SubMenu.MenuState.Closed)
                    {
                        //if (this.Owner != null && FormCollection.Menu.state != MenuState.Closed)
                        //    FormCollection.Menu.Close();

                        items[hoverIndex].SubMenu.Open(openPos);
                        state = MenuState.Opened;
                    }
                }
                else
                {
                    for (int i = 0; i < items.Count; i++)
                        if (items[i].SubMenu != null && items[i].SubMenu.State == SubMenu.MenuState.Opened && items[i].SubMenu.bCursorIsInside())
                            return;

                    CloseSubMenus();
                }
            }
        }

        private void CheckHovering()
        {
            if (items[hoverIndex].SubMenu != null && items[hoverIndex].SubMenu.State == SubMenu.MenuState.Closed)
            {
                CloseSubMenus();
                items[hoverIndex].SubMenu.Open(openPos);
                state = MenuState.Opened;
            }
        }

        public void Close()
        {
            CloseSubMenus();
            state = MenuState.Closed;
        }

        private void CloseSubMenus()
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State == SubMenu.MenuState.Opened)
                    items[i].SubMenu.Close();

            state = MenuState.Closed;
        }        

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

                if(Owner == null)
                    area = new Rectangle(0, 0, FormCollection.Graphics.GraphicsDevice.Viewport.Width, 17);
            }

            FormCollection.Graphics.GraphicsDevice.SetRenderTarget(0, null);
            FormCollection.Graphics.GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            if (Owner != null)
                area.Width = (int)Owner.Width - 2;

            area.X = (int)Position.X + 1;
            area.Y = (int)Position.Y;
            spriteBatch.Draw(pixelTex, area, BackColor);

            itemPos.X = (int)Position.X + 5;
            itemPos.Y = (int)Position.Y;

            hoverIndex = -1;

            for (int i = 0; i < items.Count; i++)
            {
                itemArea.X = (int)(itemPos.X) - 4;
                itemArea.Y = (int)(itemPos.Y);
                itemArea.Width = (int)font.MeasureString(items[i].CleanText).X + 10;
                itemArea.Height = font.LineSpacing;

                if (Owner == null)
                    itemArea.X -= 1;

                Point cursorLoc;
                if (Owner == null)
                    cursorLoc = MouseHelper.Cursor.Location;
                else
                    cursorLoc = new Point(MouseHelper.Cursor.Location.X - (int)Owner.Position.X, MouseHelper.Cursor.Location.Y - (int)Owner.Position.Y);
                
                //Selected Item
                if (items[i].SubMenu != null && items[i].SubMenu.State != SubMenu.MenuState.Closed)
                {
                    spriteBatch.Draw(pixelTex, itemArea, Color.LightGray);
                    spriteBatch.DrawString(font, items[i].CleanText, itemPos, ForeColor);
                }
                //Hover Item
                else if (itemArea.Contains(cursorLoc))
                {
                    hoverIndex = i;
                    spriteBatch.Draw(pixelTex, itemArea, Color.Silver);
                    spriteBatch.DrawString(font, items[i].CleanText, itemPos, ForeColor);

                    openPos.X = itemArea.X + 1;
                    openPos.Y = itemArea.Y + itemArea.Height + 2;
                }
                //Normal Item
                else
                    spriteBatch.DrawString(font, items[i].CleanText, itemPos, ForeColor);

                if (indexToOpen == i)
                {
                    if (items[indexToOpen].EventHandler != null)
                        items[indexToOpen].EventHandler.Invoke(null, null);

                    if (items[indexToOpen].SubMenu != null && items[indexToOpen].SubMenu.State == SubMenu.MenuState.Closed)
                    {
                        openPos.X = itemArea.X + 1;
                        openPos.Y = itemArea.Y + itemArea.Height + 2;
                        CloseSubMenus();
                        items[indexToOpen].SubMenu.Open(openPos);
                        state = MenuState.Opened;
                        openedMenu = true;
                    }
                    else
                        Close();
                }

                if (items[i].Key != Microsoft.Xna.Framework.Input.Keys.None)
                {
                    Vector2 underscorePos = new Vector2(itemPos.X + font.MeasureString(items[i].CleanText.Substring(0, items[i].KeyIndex)).X, itemPos.Y);
                    spriteBatch.DrawString(font, "_", underscorePos, ForeColor);
                }

                itemPos.X += (int)font.MeasureString(items[i].CleanText).X + 10;
            }

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State != SubMenu.MenuState.Closed)
                    items[i].SubMenu.Draw(spriteBatch);

            spriteBatch.End();
            FormCollection.Graphics.GraphicsDevice.ResolveBackBuffer(resolveTexture, 0);
        }

        public void Draw()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            if (Owner != null)
            {
                drawPos.X = (int)Owner.Position.X;
                drawPos.Y = (int)Owner.Position.Y;
                spriteBatch.Draw(resolveTexture, drawPos, BackColor);
            }
            else
                spriteBatch.Draw(resolveTexture, Vector2.Zero, BackColor);

            spriteBatch.End();
        }
    }
}
