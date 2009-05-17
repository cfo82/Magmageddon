using System;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma
{
    class MenuItem
    {
        private readonly String name;
        private readonly String text;
        private event ItemSelectionHandler itemSelected;

        public MenuItem(String name, String text, ItemSelectionHandler itemSelected)
        {
            this.name = name;
            this.text = text;
            this.itemSelected = itemSelected;
        }

        public String Name
        {
            get { return name; }
        }

        public String Text
        {
            get { return text; }
        }

        public void PerformAction()
        {
            itemSelected(this);
        }
    }
}