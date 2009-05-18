using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma
{
    class ReleaseNotesMenu : MenuScreen
    {
        string funnyReleaseNotes =
            "Improvements from the May 19 release compared to the May 7 release:\n" +
            "  - point 1\n" +
            "  - point 2\n" +
            "  - point 3\n" +
            "  - point 4\n" +
            "  - point 5\n" +
            "  - point 6\n";

        public ReleaseNotesMenu(Menu menu)
            : base(menu, new Vector2(30, 650))
        {
            
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont f = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/menu_releasenotes");
            Menu.DrawCenteredShadowString(spriteBatch, f, "RELEASE NOTES", new Vector2(620, 220), menu.StaticStringColor, 1.0f);
            Menu.DrawShadowString(spriteBatch, f, funnyReleaseNotes, new Vector2(220, 240), menu.StaticStringColor, 0.66f);
        }
    }
}