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
            : base(menu)
        {
            List<LevelInfo> levels = Game.Instance.Levels;
            this.menuItems = new MenuItem[levels.Count];
            for (int i = 0; i < menuItems.Length; i++)
            {
                menuItems[i] = new MenuItem(levels[i].Name, levels[i].Name,
                    new ItemSelectionHandler(LevelSelected));
                menuItems[i].SetActivationHandler(new ItemActivationHandler(LevelActivated));
            }
            playerMenu = new PlayerMenu(menu, this);

            RecomputeWidth();
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void LevelActivated(MenuItem sender)
        {
            Game.Instance.LoadLevel(Game.Instance.Levels[Selected].FileName);
        }

        private void LevelSelected(MenuItem sender)
        {
            menu.OpenMenuScreen(playerMenu);
        }

        protected override bool ResetSelectionOnOpen
        {
            get { return false; }
        }
    }
}