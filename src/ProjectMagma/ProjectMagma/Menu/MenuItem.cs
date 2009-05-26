using System;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma
{
    public class MenuItem
    {
        private readonly String name;
        private readonly String text;

        private event ItemSelectionHandler itemSelected;
        private event ItemActivationHandler itemActivated = null;
        private event ItemDeactivationHandler itemDeactivated = null;

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

        public void Activate()
        {
            if (itemActivated != null)
                itemActivated(this);
        }

        public void Deactivate()
        {
            if (itemDeactivated != null)
                itemDeactivated(this);
        }

        public void SetActivationHandler(ItemActivationHandler handler)
        {
            itemActivated = handler;
        }

        public void SetActivationHandler(ItemDeactivationHandler handler)
        {
            itemDeactivated = handler;
        }
    }
}