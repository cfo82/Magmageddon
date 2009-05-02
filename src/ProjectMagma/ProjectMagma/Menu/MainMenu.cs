using Microsoft.Xna.Framework;

namespace ProjectMagma
{
    class MainMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;
        readonly MenuScreen levelMenu;
        readonly MenuScreen settingsMenu;

        public MainMenu(Menu menu)
            : base(menu, new Vector2(30, 650), 200)
        {
            this.menuItems = new MenuItem[] { 
                new MenuItem("new_game", "new_game", new ItemSelectionHandler(NewGameHandler)),
                new MenuItem("settings", "settings", new ItemSelectionHandler(SettingsHandler)),
                new MenuItem("help", "help", new ItemSelectionHandler(HelpHandler)),
                new MenuItem("exit_game", "exit_game", new ItemSelectionHandler(ExitGameHandler))
            };

            levelMenu = new LevelMenu(menu);
            settingsMenu = new SettingsMenu(menu);
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void NewGameHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(levelMenu);
        }

        private void SettingsHandler(MenuItem sender)
        {
            menu.OpenMenuScreen(settingsMenu);
        }

        private void HelpHandler(MenuItem sender)
        {
        }

        private void ExitGameHandler(MenuItem sender)
        {
            Game.Instance.Exit();
        }
    }
}