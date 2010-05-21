using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma.Profiler
{
    public class Profiler
    {
        public Profiler(WrappedContentManager wrappedContent, string name)
        {
            rootSection = new Section(this, null, "root");
            sectionStack = new List<Section>(100);
            frameNumber = 0;
            inBeginFrame = false;
            this.name = name;

            filterList = new List<string>();
            filterList.Add("root");
            filterList.Add("root.draw");
            filterList.Add("root.draw.beginning_stuff");
            filterList.Add("root.draw.beginning_stuff.particle_systems.flamethrower_system.*");
            filterList.Add("root.draw.rendering");

            if (wrappedContent != null)
            {
                overlayBackground = new Texture2D(Game.Instance.GraphicsDevice, 32, 32);
                Color[] colorData = new Color[overlayBackground.Width * overlayBackground.Height];
                for (int i = 0; i < overlayBackground.Width * overlayBackground.Height; ++i)
                { colorData[i] = Color.Black; }
                overlayBackground.SetData<Color>(colorData);

                overlayBackground2 = new Texture2D(Game.Instance.GraphicsDevice, 32, 32);
                for (int i = 0; i < overlayBackground2.Width * overlayBackground2.Height; ++i)
                { colorData[i] = Color.Yellow; }
                overlayBackground2.SetData<Color>(colorData);

                spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);

                font = wrappedContent.Load<SpriteFont>("Fonts/kootenay20");
            }
        }

        public static Profiler CreateProfiler(WrappedContentManager wrappedContent, string name)
        {
            Console.WriteLine("profiling code using {0}!", name);
            return new Profiler(wrappedContent, name);
        }

        [Conditional("PROFILING")]
        public void BeginFrame()
        {
            inBeginFrame = true;
            rootSection.BeginFrame();

            sectionStack.Clear();
            sectionStack.Add(rootSection);
            rootSection.BeginSection();
        }

        [Conditional("PROFILING")]
        public void BeginSection(string name)
        {
            Section parent = sectionStack[sectionStack.Count - 1];
            Section child = parent[name];
            if (child == null)
            {
                child = parent.AllocateChild(name);
            }

            sectionStack.Add(child);
            child.BeginSection();
        }

        [Conditional("PROFILING")]
        public void EndSection(string name)
        {
            Section child = sectionStack[sectionStack.Count - 1];
            Debug.Assert(child.Name == name);
            child.EndSection();

            sectionStack.RemoveAt(sectionStack.Count - 1);
        }

        [Conditional("PROFILING")]
        public void EndFrame()
        {
            Debug.Assert(sectionStack.Count == 1);
            rootSection.EndSection();
            rootSection.EndFrame();
            ++frameNumber;
            inBeginFrame = false;
        }

        [Conditional("PROFILING")]
        public void TryEndFrame()
        {
            if (inBeginFrame)
            {
                EndFrame();
            }
        }

        [Conditional("PROFILING")]
        public void TryBeginFrame()
        {
            if (!inBeginFrame)
            {
                BeginFrame();
            }
        }

        public long FrameNumber
        {
            get { return frameNumber; }
        }

        [Conditional("PROFILING")]
        public void Write(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
            fs.Close();

            StreamWriter sw = new StreamWriter(filename, true, Encoding.ASCII);
            Write(sw);
            sw.Close();
        }

        [Conditional("PROFILING")]
        public void Write(StorageDevice device, string windowTitle, string filename)
        {
            if (device == null)
                { return; }

            // Open a storage container.StorageContainer container
#if XDK
            bool isTransferredFromOtherPlayer;
            StorageContainer container = device.OpenContainer(windowTitle, false, out isTransferredFromOtherPlayer);
#else
            StorageContainer container = device.OpenContainer(windowTitle);
#endif

            // Get the path of the save game.
            string absoluteFilename = Path.Combine(container.Path, filename);

            // Open the file, creating it if necessary.
            using (FileStream stream = File.Open(absoluteFilename, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(stream);
                Write(writer);
                writer.Close();
            }

            // Dispose the container, to commit changes.
            container.Dispose();
        }

        [Conditional("PROFILING")]
        public void Write(StreamWriter writer)
        {
            writer.WriteLine("profiling information:");
            writer.WriteLine("----------------------");
            writer.WriteLine("");
            writer.WriteLine("Frame Information:");
            for (long i = 0; i < frameNumber; ++i)
            {
                writer.WriteLine("  Frame {0}:", i);
                rootSection.WriteFrame(writer, 2, i);
            }

            writer.WriteLine("General Information:");
            rootSection.WriteGeneral(writer, 1);
        }

        [Conditional("PROFILING")]
        public void HandleInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.F10) ||
                GamePad.GetState(0).IsButtonDown(Buttons.LeftShoulder))
            {
                double at = gameTime.TotalGameTime.TotalMilliseconds;
                if (at > lastChange + OverlaySwitchTimeout)
                { 
                    renderOverlay = !renderOverlay;
                    lastChange = at;
                }
            }
        }

        private int DrawSection(GraphicsDevice graphics, Section section, int x, int y, int titleSafeX, bool showAlways)
        {
            int sectionHeight = 0;

            showAlways = showAlways || filterList.Contains(string.Format("{0}.*", section.FullName));

            if (filterList.Contains(section.FullName) || showAlways)
            {
                spriteBatch.DrawString(font, section.Name, new Vector2(x, y), Color.Yellow);
                sectionHeight += (int)font.MeasureString(section.Name).Y + 2;

                FrameStatistics totalStats = section.TotalStatistics;
                FrameStatistics peakStats = section.PeakStatistics;
                FrameStatistics currentStats = section.CurrentStatistics;

                spriteBatch.DrawString(
                    font,
                    string.Format("{0:0.00000000}", currentStats.AccumulatedTime / (double)currentStats.CallCount),
                    new Vector2(titleSafeX + 500, y), Color.Yellow);
                spriteBatch.DrawString(
                    font,
                    string.Format("{0:0.00000000}", totalStats.AccumulatedTime / (double)totalStats.CallCount),
                    new Vector2(titleSafeX + 750, y), Color.Yellow);
                spriteBatch.DrawString(
                    font,
                    string.Format("{0:0.00000000}", peakStats.AccumulatedTime / (double)peakStats.CallCount),
                    new Vector2(titleSafeX + 1000, y), Color.Yellow);
            }

            for (int i = 0; i < section.ChildCount; ++i)
                { sectionHeight += DrawSection(graphics, section[i], x + 20, y + sectionHeight, titleSafeX, showAlways); }

            return sectionHeight;
        }

        [Conditional("PROFILING")]
        public void DrawOverlay(GraphicsDevice graphics)
        {
            int x = graphics.Viewport.TitleSafeArea.Left;
            int y = graphics.Viewport.TitleSafeArea.Top;
            int width = graphics.Viewport.TitleSafeArea.Width;
            int height = graphics.Viewport.TitleSafeArea.Height;

            spriteBatch.Begin();
            if (renderOverlay)
            {
                spriteBatch.Draw(overlayBackground, new Rectangle(x, y, width, height), new Color(Color.Black, 192));
                x += 10;
                y += 10;
                width -= 20;
                height -= 20;
                
                spriteBatch.DrawString(font, "Section", new Vector2(x, y), Color.Yellow);
                spriteBatch.DrawString(font, "Avg. 20 frames", new Vector2(x+500, y), Color.Yellow);
                spriteBatch.DrawString(font, "Avg. total frames", new Vector2(x+750, y), Color.Yellow);
                spriteBatch.DrawString(font, "Max", new Vector2(x+1000, y), Color.Yellow);

                spriteBatch.End();
                spriteBatch.Begin();
                int lineY = y + (int)font.MeasureString("Section").Y + 2;
                spriteBatch.Draw(overlayBackground2, new Rectangle(x, lineY, width, 1), new Color(Color.Yellow, 255));

                spriteBatch.Draw(overlayBackground2, new Rectangle(x+490, y, 1, height), new Color(Color.Yellow, 255));
                spriteBatch.Draw(overlayBackground2, new Rectangle(x+740, y, 1, height), new Color(Color.Yellow, 255));
                spriteBatch.Draw(overlayBackground2, new Rectangle(x+990, y, 1, height), new Color(Color.Yellow, 255));

                DrawSection(graphics, rootSection, x, lineY +2, x, false);
            }
            else
            {
                string text1 = string.Format("Profiler: {0}.", name);
                string text2 = "Press (F10) or (LS) to display overlay.";

                Vector2 text1Size = font.MeasureString(text1);
                Vector2 text2Size = font.MeasureString(text2);

                //spriteBatch.Draw(testTexture, graphics.Viewport.TitleSafeArea, Color.Black);
                spriteBatch.DrawString(
                    font,
                    text1,
                    new Vector2((float)x + (float)width / 2.0f - text1Size.X / 2.0f, (float)y + 2),
                    new Color(Color.White, 0.7f)
                    );
                spriteBatch.DrawString(
                    font,
                    text2,
                    new Vector2((float)x + (float)width / 2.0f - text2Size.X / 2.0f, (float)y + 2 + text1Size.Y + 2),
                    new Color(Color.White, 0.7f)
                    );
            }
            spriteBatch.End();
        }

        private Section rootSection;
        private List<Section> sectionStack;
        private long frameNumber;
        private bool inBeginFrame;
        private string name;

        // rendering stuff
        private Texture2D overlayBackground;
        private Texture2D overlayBackground2;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private double lastChange;
        private bool renderOverlay;
        public const float OverlaySwitchTimeout = 100;

        private List<string> filterList;
    }
}
