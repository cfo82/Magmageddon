/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace xWinFormsLib
{
    public class SubMenu : Control
    {
        Control parent;
        List<MenuItem> items = new List<MenuItem>();

        public Control Parent { get { return parent; } set { parent = value; } }
        public List<MenuItem> Items { get { return items; } set { items = value; } }
        public MenuState State { get { return state; } set { state = value; } }

        Texture2D pixelTex, arrowTex;
        
        new SpriteFont Font
        {
            get
            {
                if (parent is Menu)
                    return (parent as Menu).Font;
                else if (parent != null)
                    return (parent as SubMenu).Font;
                else
                    return null;
            }
        }

        Vector2 itemPos = Vector2.Zero;
        Vector2 arrowPos = Vector2.Zero;
        Vector2 textPos = Vector2.Zero;
        Rectangle itemArea = Rectangle.Empty;
        Rectangle borderArea = Rectangle.Empty;
        int hoverIndex = -1;
        Vector2 openPos = Vector2.Zero;
        Color shadowColor = new Color(0, 0, 0, 0.5f);

        MenuState state = MenuState.Closed;
        public enum MenuState
        {
            Opened,
            Closed
        }

        Timer timer = new Timer(500);

        KeyboardHelper keyboard;
        int indexToOpen = -1;

        public SubMenu(Form owner)
            : base(string.Empty)
        {
            this.Owner = owner;
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);

            BackColor = Color.White;
            ForeColor = Color.Black;
            if (owner != null)
            {
                BackColor = owner.BackColor;
                ForeColor = owner.ForeColor;
            }

            keyboard = new KeyboardHelper();
            keyboard.OnKeyPress += Keyboard_OnPress;
            keyboard.OnKeyRelease += Keyboard_OnRelease;
        }

        public void Add(MenuItem item, SubMenu submenu)
        {
            if (submenu != null)
            {
                submenu.parent = this;
                submenu.BackColor = BackColor;
                submenu.ForeColor = ForeColor;
            }
            item.SubMenu = submenu;
            this.items.Add(item);
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            pixelTex = new Texture2D(graphics, 1, 1);
            pixelTex.SetData<Color>(new Color[] { Color.White });

            arrowTex = Texture2D.FromFile(graphics, @"content\textures\controls\arrow.png");

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null)
                    items[i].SubMenu.Initialize(content, graphics);

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            CloseSubMenus();

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null)
                    items[i].SubMenu.Dispose();

            arrowTex.Dispose();
            pixelTex.Dispose();

            base.Dispose();
        }

        private void CalculateSize()
        {
            Height = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Text == "-")
                    Height += 11;
                else
                    Height += Font.LineSpacing;

                if (Width < Font.MeasureString(items[i].Text).X + 30)
                    Width = Font.MeasureString(items[i].Text).X + 30;
            }
        }

        public void Open(Vector2 position)
        {
            if (Size == Vector2.Zero)
                CalculateSize();

            this.Position = position;

            state = MenuState.Opened;
        }

        public void Close()
        {
            hoverIndex = -1;
            indexToOpen = -1;

            state = MenuState.Closed;
            if (parent.GetType() == typeof(SubMenu))
                ((SubMenu)parent).Close();
            else if (parent.GetType() == typeof(Menu))
                ((Menu)parent).Close();

            CloseSubMenus();
        }

        private void CloseSubMenus()
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State != MenuState.Closed)
                    items[i].SubMenu.state = MenuState.Closed;
        }

        public bool bCursorIsInside()
        {
            Point cursorLoc;
            if (Owner != null)
                cursorLoc = new Point(MouseHelper.Cursor.Location.X - (int)Owner.Position.X, MouseHelper.Cursor.Location.Y - (int)Owner.Position.Y);
            else
                cursorLoc = MouseHelper.Cursor.Location;

            if (area.Contains(cursorLoc))
                return true;

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State == MenuState.Opened && items[i].SubMenu.bCursorIsInside())
                    return true;

            return false;
        }

        private void Timer_Elapsed(object obj, ElapsedEventArgs e)
        {
            if (hoverIndex != -1 && items[hoverIndex].SubMenu != null && items[hoverIndex].SubMenu.state != MenuState.Opened)
            {
                timer.Stop();
                CloseSubMenus();
                items[hoverIndex].SubMenu.Open(openPos);
            }
        }

        private void Keyboard_OnPress(object obj, EventArgs e)
        {
            Keys key = (Keys)obj;

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.state == MenuState.Opened)
                    return;

            for (int i = 0; i < items.Count; i++)
                if (key == items[i].Key)
                    indexToOpen = i;
        }
        private void Keyboard_OnRelease(object obj, EventArgs e)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (hoverIndex != -1)
                CheckSelection();

            keyboard.Update();

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State != MenuState.Closed)
                    items[i].SubMenu.Update(gameTime);
        }

        private void CheckSelection()
        {
            if (MouseHelper.HasBeenPressed)
            {
                int index = hoverIndex;

                if (items[index].EventHandler != null)
                    items[index].EventHandler.Invoke(null, null);

                if (items[index].SubMenu != null)
                {
                    CloseSubMenus();
                    if (items[index].SubMenu.State == MenuState.Closed)
                        items[index].SubMenu.Open(openPos);
                }
                else
                    Close();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            area.Width = (int)Width;
            area.Height = (int)Height;

            area.X = (int)Position.X + 6;
            area.Y = (int)Position.Y + 4;
            spriteBatch.Draw(pixelTex, area, shadowColor);

            area.X = (int)Position.X;
            area.Y = (int)Position.Y;
            borderArea.X = area.X - 1;
            borderArea.Y = area.Y - 1;
            borderArea.Width = area.Width + 2;
            borderArea.Height = area.Height + 2;
            spriteBatch.Draw(pixelTex, borderArea, Color.Black);
            spriteBatch.Draw(pixelTex, area, BackColor);

            hoverIndex = -1;

            itemPos = Position;            
            for (int i = 0; i < items.Count; i++)
            {
                itemArea.X = (int)itemPos.X;
                itemArea.Y = (int)itemPos.Y;

                if (itemArea.Width == 0)
                    itemArea.Width = (int)Width;

                if (items[i].Text != string.Empty && items[i].Text != "-")
                {

                    if (itemArea.Height == 0)
                        itemArea.Height = Font.LineSpacing;

                    Point cursorLoc;
                    if (Owner != null)
                        cursorLoc = new Point(MouseHelper.Cursor.Location.X - (int)Owner.Position.X, MouseHelper.Cursor.Location.Y - (int)Owner.Position.Y);
                    else
                        cursorLoc = new Point(MouseHelper.Cursor.Location.X, MouseHelper.Cursor.Location.Y);

                    if (items[i].SubMenu != null && items[i].SubMenu.State != MenuState.Closed)
                        spriteBatch.Draw(pixelTex, itemArea, Color.LightGray);
                    else if (itemArea.Contains(cursorLoc))
                    {
                        hoverIndex = i;
                        openPos.X = itemArea.X + itemArea.Width - 3;
                        openPos.Y = itemArea.Y;
                        timer.Start();

                        spriteBatch.Draw(pixelTex, itemArea, Color.Silver);
                    }

                    if (i == indexToOpen)
                    {
                        if (items[indexToOpen].SubMenu != null && items[indexToOpen].SubMenu.state == MenuState.Closed)
                        {
                            openPos.X = itemArea.X + itemArea.Width - 3;
                            openPos.Y = itemArea.Y;
                            items[indexToOpen].SubMenu.Open(openPos);
                            CloseSubMenus();
                        }
                        else if (items[indexToOpen].EventHandler != null)
                            items[indexToOpen].EventHandler.Invoke(null, null);                        

                        indexToOpen = -1;
                    }

                    textPos = itemPos;
                    textPos.X += 2f;
                    spriteBatch.DrawString(Font, items[i].CleanText, textPos, ForeColor);
                    itemPos.Y += Font.LineSpacing;

                    if (items[i].Key != Microsoft.Xna.Framework.Input.Keys.None)
                    {
                        Vector2 underscorePos = new Vector2((int)textPos.X + (int)Font.MeasureString(items[i].CleanText.Substring(0, items[i].KeyIndex)).X, (int)textPos.Y);
                        spriteBatch.DrawString(Font, "_", underscorePos, ForeColor);
                    }

                    if (items[i].SubMenu != null)
                    {
                        arrowPos.X = itemArea.X + itemArea.Width - 15;
                        arrowPos.Y = itemArea.Y + (itemArea.Height - arrowTex.Height) / 2;
                        spriteBatch.Draw(arrowTex, arrowPos, Color.Black);
                    }
                }
                else if (items[i].Text == "-")
                {
                    Rectangle separator = new Rectangle((int)itemPos.X, (int)itemPos.Y + 6, (int)Width, 1);
                    spriteBatch.Draw(pixelTex, separator, Color.Silver);
                    itemPos.Y += 11;
                }
            }

            for (int i = 0; i < items.Count; i++)
                if (items[i].SubMenu != null && items[i].SubMenu.State != MenuState.Closed)
                    items[i].SubMenu.Draw(spriteBatch);

            if (hoverIndex == -1 && timer.Enabled)
                timer.Stop();
        }
    }
}
