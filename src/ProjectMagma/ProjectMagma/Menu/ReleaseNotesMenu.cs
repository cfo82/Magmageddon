using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    class ReleaseNotesMenu : MenuScreen
    {
        private static readonly string funnyReleaseNotes =
            "Improvements from the May 19 release compared to the May 7 release:\n" +
            "  - New control possibilites: Use repulsion (left trigger) to move the island you\n" + 
            "        are occupying, jump to other islands as soon as they are in range\n" + 
            "        (indicated by an arrow)\n" +
            "  - Check out out the new levels: The game feels different on those maps.\n" + 
            "  - Watch out for power-ups. They help a lot if you are short on health or energy!\n" +
            "  - If you are pushed off an island, save yourself using the jetpack on button A!\n" + 
            "        If the opponent tries to save himself, take him down using an ice spike\n" + 
            "        (this disables the jetpack)\n" + 
            "  - Please report any blue screens you might encounter! It is so easy, take a picture\n" + 
            "        and mail it to us: dpk@student.ethz.ch!\n" + 
            "  - Enjoy the game!\n";
        private string[] funnyReleaseNotesLines = funnyReleaseNotes.Split('\n');

        public ReleaseNotesMenu(Menu menu)
            : base(menu, new Vector2(30, 650))
        {
            
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont f = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/menu_releasenotes");
            DrawTools.DrawCenteredShadowString(spriteBatch, f, "RELEASE NOTES", new Vector2(620, 132), menu.StaticStringColor, 1.0f);

            string[] lines = funnyReleaseNotesLines;

            float offset = 0;
            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].StartsWith("  -")) { offset += 10; }
                DrawTools.DrawShadowString(spriteBatch, f, lines[i], new Vector2(220, 175 + offset), menu.StaticStringColor, 0.66f);
                offset += 30;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (gamePadState.Buttons.Start == ButtonState.Pressed
                || gamePadState.Buttons.A == ButtonState.Pressed
                || keyboardState.IsKeyDown(Keys.Escape))
            {
                menu.CloseActiveMenuScreen(true);
                menu.buttonPressedAt = gameTime.TotalGameTime.TotalMilliseconds;
            }
        }
    }
}