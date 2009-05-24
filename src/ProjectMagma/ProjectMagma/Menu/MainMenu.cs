using Microsoft.Xna.Framework;

namespace ProjectMagma
{
    class MainMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;

        public MainMenu(Menu menu)
            : base(menu)
        {
            this.menuItems = new MenuItem[] { 
                new MenuItem("new_game", "NEW GAME", new ItemSelectionHandler(NewGameHandler)),
                new MenuItem("settings", "SETTINGS", new ItemSelectionHandler(SettingsHandler)),
                new MenuItem("help", "HELP", new ItemSelectionHandler(HelpHandler)),
                new MenuItem("credits", "CREDITS", new ItemSelectionHandler(CreditsHandler)),
                new MenuItem("exit_game", "EXIT", new ItemSelectionHandler(ExitGameHandler))
            };

            RecomputeWidth();
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void NewGameHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(menu.LevelMenu);
        }

        private void SettingsHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(menu.SettingsMenu);
        }

        private void HelpHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(menu.HelpMenu);
        }

        private void CreditsHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(menu.CreditsMenuPage1);
        }

        private void ExitGameHandler(MenuItem sender)
        {
            Game.Instance.Exit();
        }
    }
}