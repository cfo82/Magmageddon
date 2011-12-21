using System;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma
{
    public class MenuItem
    {
        private readonly String name;
        private readonly String text;

        private event ItemSelectionHandler OnItemSelected;
        private event ItemActivationHandler OnItemActivated = null;
        private event ItemDeactivationHandler OnItemDeactivated = null;

        public MenuItem(String name, String text, ItemSelectionHandler itemSelected)
        {
            this.name = name;
            this.text = text;
            this.OnItemSelected = itemSelected;
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
            OnItemSelected(this);
        }

        public void Activate()
        {
            if (OnItemActivated != null)
            {
                OnItemActivated(this);
            }
        }

        public void Deactivate()
        {
            if (OnItemDeactivated != null)
            {
                OnItemDeactivated(this);
            }
        }

        public void SetActivationHandler(ItemActivationHandler handler)
        {
            OnItemActivated = handler;
        }

        public void SetActivationHandler(ItemDeactivationHandler handler)
        {
            OnItemDeactivated = handler;
        }
    }
}