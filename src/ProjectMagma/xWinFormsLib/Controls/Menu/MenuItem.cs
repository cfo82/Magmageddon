/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace xWinFormsLib
{
    public class MenuItem
    {
        string name = string.Empty;
        string text = string.Empty;
        string cleanText = string.Empty;

        int keyIndex = 0;
        Keys key;

        EventHandler eventHandler;
        SubMenu subMenu;

        public string Name { get { return name; } set { name = value; } }
        public string Text { get { return text; } set { text = value; } }
        public string CleanText { get { return cleanText; } }
        public Keys Key { get { return key; } set { key = value; } }
        public int KeyIndex { get { return keyIndex; } }
        public SubMenu SubMenu { get { return subMenu; } set { subMenu = value; } }
        public EventHandler EventHandler { get { return eventHandler; } set { eventHandler = value; } }

        public MenuItem(string name, string value, EventHandler eventHandler)
        {
            this.name = name;
            this.text = value;
            this.eventHandler = eventHandler;

            if (this.text.Contains("&"))
            {
                string[] split = this.text.Split(new char[] { '&' });
                keyIndex = split[0].Length;
                this.key = Utils.GetKey(split[1][0]);
                cleanText = text.Replace("&", "");
            }
            else
                cleanText = text;
        }        
    }
}
