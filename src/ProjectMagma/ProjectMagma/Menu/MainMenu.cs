using Microsoft.Xna.Framework;

namespace ProjectMagma
{
    class MainMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;
        readonly MenuScreen levelMenu;
        readonly MenuScreen settingsMenu;
        readonly MenuScreen helpMenu;

        public MainMenu(Menu menu)
            //: base(menu, new Vector2(230, 650), 200)
            : base(menu, new Vector2(640, 500), 200)
        {
            this.menuItems = new MenuItem[] { 
                new MenuItem("new_game", "NEW GAME", new ItemSelectionHandler(NewGameHandler)),
                new MenuItem("settings", "SETTINGS", new ItemSelectionHandler(SettingsHandler)),
                new MenuItem("help", "HELP", new ItemSelectionHandler(HelpHandler)),
                new MenuItem("exit_game", "EXIT", new ItemSelectionHandler(ExitGameHandler))
            };

            levelMenu = new LevelMenu(menu);
            settingsMenu = new SettingsMenu(menu);
            helpMenu = new HelpMenu(menu);
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
            menu.OpenMenuScreen(helpMenu);
        }

        private void ExitGameHandler(MenuItem sender)
        {
            Game.Instance.Exit();
        }
    }
}