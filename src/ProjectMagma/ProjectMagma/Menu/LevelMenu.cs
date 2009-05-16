using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma
{
    class LevelMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;
        readonly MenuScreen playerMenu;

        public LevelMenu(Menu menu)
            : base(menu, new Vector2(280, 600), 200)
        {
            List<LevelInfo> levels = Game.Instance.Levels;
            this.menuItems = new MenuItem[levels.Count];
            for (int i = 0; i < menuItems.Length; i++)
            {
                menuItems[i] = new MenuItem(levels[i].Name, levels[i].FileName,
                    new ItemSelectionHandler(LevelSelected));
            }
            playerMenu = new PlayerMenu(menu, this);
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void LevelSelected(MenuItem sender)
        {
            Game.Instance.LoadLevel(Game.Instance.Levels[Selected].FileName);
            menu.OpenMenuScreen(playerMenu);
        }

        public override void OnOpen()
        {
            // do nothing (base reverts to first item)
        }
    }
}