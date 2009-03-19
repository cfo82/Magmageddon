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
    public class ButtonGroup : Control
    {
        Button[] button;
        Color dim, highlight;

        int selectedIndex = 0;
        public string Selected
        {
            get { return button[selectedIndex].Text; }
            set
            {
                for (int i = 0; i < button.Length; i++)
                    if (button[i].Text == value)
                    {
                        selectedIndex = i;
                        break;
                    }
            }
        }

        public EventHandler OnChangeSelection;

        /// <summary>
        /// ButtonGroup Constructor
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="button">Button Array</param>
        /// <param name="selectedIndex">Index of the Selected Button in the array</param>
        public ButtonGroup(string name, Button[] button)
            : base(name)
        {
            this.button = button;
        }

        public ButtonGroup(string name, Button[] button, int selected)
            : base(name)
        {
            this.button = button;
            this.selectedIndex = selected;
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            BackColor = new Color(220, 220, 220, 255);
            dim = new Color(200, 200, 200, 255);
            highlight = new Color(255, 255, 255, 255);

            for (int i = 0; i < button.Length; i++)
            {
                button[i].Font = this.Font;
                button[i].Owner = this.Owner;
                button[i].Initialize(content, graphics);
                button[i].OnRelease += Button_OnRelease;
            }

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            for (int i = 0; i < button.Length; i++)
                button[i].Dispose();

            base.Dispose();
        }

        private void Button_OnRelease(object obj, EventArgs e)
        {
            Button button = obj as Button;
            
            for(int i=0;i<this.button.Length;i++)
                if (this.button[i] == button)
                {
                    if (selectedIndex != i)
                    {
                        selectedIndex = i;
                        if (OnChangeSelection != null)
                            OnChangeSelection(this.button[selectedIndex].Text, null);
                    }

                    break;
                }
        }        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < button.Length; i++)
                button[i].Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < button.Length; i++)
            {
                if (i == selectedIndex && button[i].BackColor != highlight)
                    button[i].Draw(spriteBatch, highlight);
                else if(button[i].area.Contains(MouseHelper.Cursor.Location))
                    button[i].Draw(spriteBatch, BackColor);
                else if (button[i].BackColor != dim)
                    button[i].Draw(spriteBatch, dim);
            }
        }
    }
}
