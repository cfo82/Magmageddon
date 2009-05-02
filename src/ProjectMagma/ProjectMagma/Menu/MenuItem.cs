using System;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma
{
    class MenuItem
    {
        private readonly String name;
        private readonly Texture2D sprite;
        private readonly Texture2D selectedSprite;
        private event ItemSelectionHandler itemSelected;

        public MenuItem(String name, String sprite, ItemSelectionHandler itemSelected)
        {
            this.name = name;
            this.sprite = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/" + sprite);
            this.selectedSprite = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/" + sprite + "_selected");
            this.itemSelected = itemSelected;
        }

        public String Name
        {
            get { return name; }
        }

        public Texture2D Sprite
        {
            get { return sprite; }
        }

        public Texture2D SelectedSprite
        {
            get { return selectedSprite; }
        }

        public void PerformAction()
        {
            itemSelected(this);
        }
    }
}