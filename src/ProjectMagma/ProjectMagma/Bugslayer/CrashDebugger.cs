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
        // resources allocated to render the crash debugger
        private SpriteFont font;
        private SpriteBatch spriteBatch;
        //private Texture2D testTexture;

        // data to be shown by the crash debugger
        private string userMail;
        private string message;
        private Exception exception;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="wrappedContent"></param>
        /// <param name="fontName"></param>
        /// <param name="userMail"></param>
        public CrashDebugger(
            GraphicsDevice device,
            WrappedContentManager wrappedContent,
            string fontName,
            string userMail
        )
        {
            font = wrappedContent.Load<SpriteFont>(fontName);
            //testTexture = wrappedContent.Load<Texture2D>("Textures/custom_uv_diag");
            spriteBatch = new SpriteBatch(device);
            this.userMail = userMail;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public void Update(GraphicsDevice device)
        {
            lock (this)
            {
                if (this.message != null)
                    { return; }

                message = string.Format(
                    "**** CRASH LOG (Please take a picture of this and send it to {0}!) ****\n" +
                    "Press Back to Exit\n" +
                    "Exception: {1}\n" +
                    "Stack Trace:\n{2}",
                    userMail,
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
                    // some funny string manipulation until it fits into the
                    // title safe area
                    string currentLine = line;
                    bool lineAdded = false;
                    do
                    {
                        Vector2 size = font.MeasureString(currentLine);
                        string nextline = "";
                        while (size.X > device.Viewport.TitleSafeArea.Width)
                        {
                            char c = currentLine[currentLine.Length - 1];
                            currentLine = currentLine.Substring(0, currentLine.Length - 1);
                            nextline = c + nextline;
                            size = font.MeasureString(currentLine);
                        }

                        builder.Append(currentLine);
                        builder.Append("\n");

                        if (nextline.Length > 0)
                        {
                            currentLine = nextline;
                        }
                        else
                        {
                            lineAdded = true;
                        }
                    } while (!lineAdded);
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        public void Draw(GraphicsDevice graphics)
        {
            lock (this)
            {
                graphics.Clear(Color.Blue);

                int x = graphics.Viewport.TitleSafeArea.Left;
                int y = graphics.Viewport.TitleSafeArea.Top;
                int width = graphics.Viewport.TitleSafeArea.Width;
                int height = graphics.Viewport.TitleSafeArea.Height;

                spriteBatch.Begin();
                //spriteBatch.Draw(testTexture, graphics.Viewport.TitleSafeArea, Color.Black);
                spriteBatch.DrawString(
                    font,
                    message == null ? "Draw without a message being specified!" : message,
                    new Vector2(x, y),
                    Color.White
                    );
                spriteBatch.End();
            }
        }

        public void Crash(Exception exception)
        {
            lock (this)
            {
                if (this.exception != null)
                    { return; } // already crashed... fix first exception...
                this.exception = exception;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Crashed
        {
            get { lock (this) { return exception != null; } }
        }

        /// <summary>
        /// 
        /// </summary>
        public Exception Exception
        {
            get { lock (this) { return exception; } }
        }
    }
}
