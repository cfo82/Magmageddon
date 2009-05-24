using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    public abstract class CreditsMenuBase : MenuScreen
    {
        public CreditsMenuBase(Menu menu, Vector2 position)
        :   base(menu, new Vector2(190, 75))
        {
            DrawPrevious = false;
        }

        public override void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            if (at > menu.buttonPressedAt + Menu.ButtonRepeatTimeout)
            {
                if ((gamePadState.Buttons.A == ButtonState.Pressed
                    && menu.lastGPState.Buttons.A == ButtonState.Released)
                    || (keyboardState.IsKeyDown(Keys.Enter)
                    && menu.lastKBState.IsKeyUp(Keys.Enter)))
                {
                    menu.CloseActiveMenuScreen();
                    menu.OpenMenuScreen(NextPage);
                    menu.buttonPressedAt = at;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "- A - NEXT PAGE                                         - B - BACK",
                new Vector2(640, 620), menu.StaticStringColor, 0.55f);
        }

        public override void OnOpen()
        {
        }

        public abstract MenuScreen NextPage
        {
            get;
        }
    }

    class CreditsMenuPage1 : CreditsMenuBase
    {
        public CreditsMenuPage1(Menu menu)
        :   base(menu, new Vector2(190, 75))
        {
            DrawPrevious = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "CREDITS (PAGE 1)",
                new Vector2(620, 132), menu.StaticStringColor, 1.0f);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "JANICK", new Vector2(350, 220), menu.StaticStringColor, 0.65f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "BERNET", new Vector2(350, 245), menu.StaticStringColor, 0.65f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "GAMEPLAY", new Vector2(350, 275), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "LEVEL DESIGN", new Vector2(350, 295), menu.StaticStringColor, 1.0f);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "DOMINIK", new Vector2(930, 220), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "KAESER", new Vector2(930, 245), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "LEAD", new Vector2(930, 275), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "ARTIST", new Vector2(930, 291), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "GRAPHICS", new Vector2(930, 316), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "PROGRAMMING", new Vector2(930, 332), menu.StaticStringColor, 1.0f);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "CHRISTIAN", new Vector2(640, 220), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "OBERHOLZER", new Vector2(640, 245), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "ENGINE PROGRAMMING", new Vector2(640, 275), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "SPECIAL EFFECTS PROGRAMMING", new Vector2(640, 295), menu.StaticStringColor, 1.0f);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "JOYA", new Vector2(495, 400), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "CLARK", new Vector2(495, 425), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "3D MODELING", new Vector2(495, 450), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "TEXTURING", new Vector2(495, 470), menu.StaticStringColor, 1.0f);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "AUSTIN", new Vector2(785, 400), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "RODERIQUE", new Vector2(785, 425), menu.StaticStringColor, 0.7f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "CHARACTER MODELING", new Vector2(785, 450), menu.StaticStringColor, 1.0f);
            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFontSmall, "CHARACTER ANIMATION", new Vector2(785, 470), menu.StaticStringColor, 1.0f);
        
        }

        public override void OnOpen()
        {
        }

        public override MenuScreen NextPage
        {
            get { return menu.CreditsMenuPage2; }
        }
    }

    class CreditsMenuPage2 : CreditsMenuBase
    {
        public CreditsMenuPage2(Menu menu)
        :   base(menu, new Vector2(190, 75))
        {
            DrawPrevious = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "CREDITS (PAGE 2)",
                new Vector2(620, 132), menu.StaticStringColor, 1.0f);
        }

        public override void OnOpen()
        {
        }

        public override MenuScreen NextPage
        {
            get { return menu.CreditsMenuPage1; }
        }
    }
}