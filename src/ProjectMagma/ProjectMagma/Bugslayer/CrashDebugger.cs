using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Bugslayer
{
    public class CrashDebugger
    {
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private string message;
        private static readonly int MAX_LINE_LENGTH = 100;
        private Exception exception;

        public CrashDebugger(GraphicsDevice device, WrappedContentManager wrappedContent)
        {
            font = wrappedContent.Load<SpriteFont>("Fonts/kootenay9");
            spriteBatch = new SpriteBatch(device);
        }

        public void SetException(Exception exception)
        {
            if (exception == this.exception)
                { return; }

            this.exception = exception;

            message = string.Format(
                "**** CRASH LOG (Please take a picture of this and send it to dpk@student.ethz.ch!) ****\n" + 
                "Press Back to Exit\n" + 
                "Exception: {0}\n" + 
                "Stack Trace:\n{1}",
                exception.Message,
                exception.StackTrace);

            if (exception.InnerException != null)
            {
                message += 
                    "\n\nInner exception: " +
                    exception.Message +
                    "\nInner Stack Trace:\n" +
                    exception.StackTrace
                ;
            }

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

            char[] messageArray = message.ToCharArray();
            for (int i = 0; i < messageArray.Length; ++i)
            {
                if ((messageArray[i] < 32 || messageArray[i] > 126) &&
                    messageArray[i] != '\n' &&
                    messageArray[i] != '\r' &&
                    messageArray[i] != '\t')
                {
                    messageArray[i] = ' ';
                }
            }
            message = new string(messageArray);
        }

        public void Draw(GraphicsDevice graphics)
        {
            graphics.Clear(Color.Blue);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, this.message, new Vector2(10f, 10f), Color.White);

            spriteBatch.End();
        }

        public Exception Exception
        {
            get { return exception; }
        }
    }
}
