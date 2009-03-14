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
    public class CheckboxGroup : Control
    {
        Checkbox[] checkbox;
        
        public EventHandler OnChangeSelection;

        int selectedIndex = -1;
        public string Selected
        {
            get
            {
                for (int i = 0; i < checkbox.Length; i++)
                    if (checkbox[i].Value == true)
                        return checkbox[i].Text;

                return "";
            }

            set
            {
                for (int i = 0; i < checkbox.Length; i++)
                    if (checkbox[i].Text.ToLower() == value.ToLower())
                        checkbox[i].Value = true;
                    else
                        checkbox[i].Value = false;
            }
        }

        /// <summary>
        /// Checkbox Constructor
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="checkbox">Checkbox Array</param>
        public CheckboxGroup(string name, Checkbox[] checkbox)
            : base(name)
        {
            this.checkbox = checkbox;            
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            // TODO: load your content here
            for (int i = 0; i < checkbox.Length; i++)
            {
                checkbox[i].Font = this.Font;
                checkbox[i].Owner = this.Owner;
                checkbox[i].Initialize(content, graphics);
                checkbox[i].CanUncheck = false;
                checkbox[i].OnPress += Checkbox_Press;

                if (checkbox[i].Value == true)
                    selectedIndex = i;

                if (Left < checkbox[i].Left)
                    Left = checkbox[i].Left;
                if (Top < checkbox[i].Top)
                    Top = checkbox[i].Top;

                if (Width < checkbox[i].Width)
                    Width = checkbox[i].Width;
                Height += checkbox[i].Height;
            }

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            // TODO: dispose of your content here
            for (int i = 0; i < checkbox.Length; i++)
                checkbox[i].Dispose();

            base.Dispose();
        }

        private void Checkbox_Press(object obj, EventArgs e)
        {
            Checkbox checkbox = (Checkbox)obj;            

            int checkIndex = -1;
            for (int i = 0; i < this.checkbox.Length; i++)
                if (this.checkbox[i] == checkbox)
                {
                    checkIndex = i;
                    break;
                }

            if(checkIndex != selectedIndex)
                if (OnChangeSelection != null)
                    OnChangeSelection(checkbox.Text, null);

            selectedIndex = checkIndex;

            for (int i = 0; i < this.checkbox.Length; i++)
                if (i != checkIndex)
                    this.checkbox[i].Value = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: Add your update logic here
            for (int i = 0; i < checkbox.Length; i++)
                checkbox[i].Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // TODO: Add your drawing code here
            for (int i = 0; i < checkbox.Length; i++)
                checkbox[i].Draw(spriteBatch);
        }
    }
}
