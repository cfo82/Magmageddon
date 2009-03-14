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

namespace xWinFormsLib
{
    public class ComboBox : Control
    {
        Textbox textbox;        
        Button button;
        Listbox listbox;
        
        public List<string> Items { get { return listbox.Items; } set { listbox.Items = value; } }

        public bool Opened { get { return listbox.Visible; } }
        public bool Locked { get { return textbox.Locked; } set { textbox.Locked = value; } }
        public new string Text { get { return textbox.Text; } set { textbox.Text = value; } }

        public EventHandler OnSelectionChanged = null;
        bool justOpened = false;

        string[] items;
        public ComboBox(string name, Vector2 position, int width, string[] items)
            : base(name, position)
        {
            this.Name = name;
            this.Position = position;
            this.Width = width;
            this.items = items;
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            // TODO: load your content here
            textbox = new Textbox("combotext", Position, (int)Width);
            textbox.Font = this.Font;
            textbox.Owner = this.Owner;
            textbox.Locked = true;
            textbox.Initialize(content, graphics);

            button = new Button("btOpen", Position + new Vector2(Width - 16, 0f), @"content\textures\controls\combobox\button.png", 1f, Color.White);
            button.Font = this.Font;
            button.Owner = this.Owner;
            button.OnPress = Button_OnPress;
            button.Initialize(content, graphics);

            listbox = new Listbox("combolist", Position + new Vector2(0, 19), (int)Width, 8 * Font.LineSpacing, items);
            listbox.Font = this.Font;
            listbox.Owner = this.Owner;
            listbox.Visible = false;
            listbox.OnChangeSelection = Listbox_OnChangeSelection;
            listbox.OnPress = Listbox_OnPress;
            listbox.Initialize(content, graphics);

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            // TODO: dispose of your content here
            textbox.Dispose();
            button.Dispose();
            listbox.Dispose();

            base.Dispose();
        }

        private void Button_OnPress(object obj, EventArgs e)
        {
            if (!listbox.Visible)
                Open();
            else
                Close();
        }

        private void Open()
        {
            justOpened = true;
            listbox.Visible = true;
        }

        private void Close()
        {
            listbox.Visible = false;
        }

        private void Listbox_OnChangeSelection(object obj, EventArgs e)
        {
            string previousItem = textbox.Text;
            textbox.Text = listbox.SelectedItem;

            if (textbox.Text != previousItem && OnSelectionChanged != null)
                OnSelectionChanged(listbox.SelectedItem, null);

            Close();
        }

        private void Listbox_OnPress(object obj, EventArgs e)
        {
            Close();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: Add your update logic here
            textbox.Update(gameTime);
            button.Update(gameTime);

            if (listbox.Visible)
            {
                listbox.Update(gameTime);

                if (!listbox.area.Contains(MouseHelper.Cursor.Location) && MouseHelper.HasBeenPressed && !justOpened)
                    Close();

                if (MouseHelper.HasBeenReleased)
                    justOpened = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // TODO: Add your drawing code here
            textbox.Draw(spriteBatch);
            button.Draw(spriteBatch);
        }

        public void DrawOverlay(SpriteBatch spriteBatch)
        {
            if (listbox.Visible)
                listbox.Draw(spriteBatch);
        }
    }
}
