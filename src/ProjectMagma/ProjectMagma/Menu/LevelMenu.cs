using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma
{
    public class LevelMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;

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

            RecomputeWidth();
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void LevelActivated(MenuItem sender)
        {
            Game.Instance.LoadLevel(
                Game.Instance.Levels[Selected].SimulationFileName,
                Game.Instance.Levels[Selected].RendererFileName
            );
        }

        private void LevelSelected(MenuItem sender)
        {
            menu.OpenMenuScreen(menu.PlayerMenu, true);
        }

        protected override bool ResetSelectionOnOpen
        {
            get { return false; }
        }
    }
}