using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma
{
    class ReleaseNotesMenu : MenuScreen
    {
        string funnyReleaseNotes =
            "Release Notes\n" +
            "=============\n" +
            "improvements from the 05/18/2009 release compared to the 07/05/2009 release:\n" +
            "  - point 1\n" +
            "  - point 2\n" +
            "  - point 3\n" +
            "  - point 4\n" +
            "  - point 5\n" +
            "  - point 6\n" +
            "  - point 7\n" +
            "  - point 8\n" +
            "  - ...\n" +
            "  - point n\n";

        public ReleaseNotesMenu(Menu menu)
            : base(menu, new Vector2(30, 650))
        {
            
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont f = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/ReleaseNotesFont");
            spriteBatch.DrawString(f, funnyReleaseNotes, new Vector2(50, 50), Color.White);
        }
    }
}