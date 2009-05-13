using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Bugslayer
{
    public class CrashDebugGame : Microsoft.Xna.Framework.Game
    {
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private readonly Exception exception;

        public CrashDebugGame(Exception exception)
        {
            this.exception = exception;
            new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Fonts/kootenay9");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.DrawString(
               font,
               "**** CRASH LOG (Please take a picture of this and send it to the developers!) ****",
               new Vector2(10f, 10f),
               Color.White);
            spriteBatch.DrawString(
               font,
               "Press Back to Exit",
               new Vector2(10f, 22f),
               Color.White);
            spriteBatch.DrawString(
               font,
               string.Format("Exception: {0}", exception.Message),
               new Vector2(10f, 34f),
               Color.White);
            spriteBatch.DrawString(
               font, string.Format("Stack Trace:\n{0}", exception.StackTrace),
               new Vector2(10f, 46f),
               Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
