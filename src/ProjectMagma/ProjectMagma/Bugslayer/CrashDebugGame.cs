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
        private readonly string message;
        private static readonly int MAX_LINE_LENGTH = 100;

        public CrashDebugGame(Exception exception)
        {
            new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            message = string.Format(
                "**** CRASH LOG (Please take a picture of this and send it to dpk@student.ethz.ch!) ****\n" + 
                "Press Back to Exit\n" + 
                "Exception: {0}\n" + 
                "Stack Trace:\n{1}",
                exception.Message,
                exception.StackTrace);

            string[] lines = message.Split('\n');
            StringBuilder builder = new StringBuilder();
            foreach (string line in lines)
            {
                string currentLine = line;

                while (currentLine.Length > MAX_LINE_LENGTH)
                {
                    builder.Append(currentLine.Substring(0, MAX_LINE_LENGTH));
                    builder.Append("\n");
                    currentLine = "      " + currentLine.Substring(MAX_LINE_LENGTH);
                }

                builder.Append(currentLine);
                builder.Append("\n");
            }

            message = builder.ToString();
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
            spriteBatch.DrawString(font, this.message, new Vector2(10f, 10f), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
