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
    public class RadioButtonGroup : Control
    {
        RadioButton[] radiobutton;
        
        public EventHandler OnChangeSelection;

        int selectedIndex = -1;
        public string Selected
        {
            get
            {
                for (int i = 0; i < radiobutton.Length; i++)
                    if (radiobutton[i].Value == true)
                        return radiobutton[i].Text;

                return "";
            }

            set
            {
                for (int i = 0; i < radiobutton.Length; i++)
                    if (radiobutton[i].Text.ToLower() == value.ToLower())
                        radiobutton[i].Value = true;
                    else
                        radiobutton[i].Value = false;
            }
        }

        /// <summary>
        /// Checkbox Constructor
        /// </summary>
        /// <param name="name">Control Name</param>
        /// <param name="checkbox">Checkbox Array</param>
        public RadioButtonGroup(string name, RadioButton[] radiobutton)
            : base(name)
        {
            this.radiobutton = radiobutton;            
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            for (int i = 0; i < radiobutton.Length; i++)
            {
                radiobutton[i].Font = this.Font;
                radiobutton[i].Owner = this.Owner;
                radiobutton[i].Initialize(content, graphics);
                radiobutton[i].CanUncheck = false;
                radiobutton[i].OnPress += Checkbox_Press;

                if (radiobutton[i].Value == true)
                    selectedIndex = i;

                if (Left < radiobutton[i].Left)
                    Left = radiobutton[i].Left;
                if (Top < radiobutton[i].Top)
                    Top = radiobutton[i].Top;

                if (Width < radiobutton[i].Width)
                    Width = radiobutton[i].Width;
                Height += radiobutton[i].Height;
            }

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            for (int i = 0; i < radiobutton.Length; i++)
                radiobutton[i].Dispose();

            base.Dispose();
        }

        private void Checkbox_Press(object obj, EventArgs e)
        {
            RadioButton radiobutton = (RadioButton)obj;            

            int checkIndex = -1;
            for (int i = 0; i < this.radiobutton.Length; i++)
                if (this.radiobutton[i] == radiobutton)
                {
                    checkIndex = i;
                    break;
                }

            if(checkIndex != selectedIndex)
                if (OnChangeSelection != null)
                    OnChangeSelection(radiobutton.Text, null);

            selectedIndex = checkIndex;

            for (int i = 0; i < this.radiobutton.Length; i++)
                if (i != checkIndex)
                    this.radiobutton[i].Value = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < radiobutton.Length; i++)
                radiobutton[i].Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < radiobutton.Length; i++)
                radiobutton[i].Draw(spriteBatch);
        }
    }
}
